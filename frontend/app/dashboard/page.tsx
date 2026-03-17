import {
  type ForecastModelKey,
  getPromotionRecommendations,
  getDashboardOverview,
  getPerformanceSeries,
  getSalesByCustomer,
  getSalesByPeriod,
  getSpendingByCustomer,
  getTopProducts,
} from "@/lib/dashboard.api";
import { getCosts } from "@/lib/costs.api";

import DashboardView, { type DashboardViewMode } from "./ui/DashboardView";
import type { PerformanceMetric } from "./ui/PerformanceSeriesChart";

type Props = {
  searchParams?: Promise<Record<string, string | string[] | undefined>>;
};

function pickFirst(v: string | string[] | undefined): string | undefined {
  if (Array.isArray(v)) return v[0];
  return v;
}

function pickAll(v: string | string[] | undefined): string[] {
  if (Array.isArray(v)) return v;
  return v ? [v] : [];
}

function clamp(n: number, min: number, max: number) {
  return Math.max(min, Math.min(max, n));
}

function toIso(dt: Date) {
  return dt.toISOString();
}

function clampTzOffsetMinutes(n: number) {
  return clamp(n, -12 * 60, 14 * 60);
}

function localStartOfDayUtc(year: number, month: number, day: number, tzOffsetMinutes: number) {
  const ms = Date.UTC(year, month - 1, day, 0, 0, 0, 0) + tzOffsetMinutes * 60 * 1000;
  return new Date(ms);
}

function localEndOfDayUtc(year: number, month: number, day: number, tzOffsetMinutes: number) {
  const ms = Date.UTC(year, month - 1, day, 23, 59, 59, 999) + tzOffsetMinutes * 60 * 1000;
  return new Date(ms);
}

function localStartOfMonthUtc(year: number, month: number, tzOffsetMinutes: number) {
  const ms = Date.UTC(year, month - 1, 1, 0, 0, 0, 0) + tzOffsetMinutes * 60 * 1000;
  return new Date(ms);
}

function localEndOfMonthUtc(year: number, month: number, tzOffsetMinutes: number) {
  const nextMonthStartMs = Date.UTC(year, month, 1, 0, 0, 0, 0) + tzOffsetMinutes * 60 * 1000;
  return new Date(nextMonthStartMs - 1);
}

function localStartOfYearUtc(year: number, tzOffsetMinutes: number) {
  const ms = Date.UTC(year, 0, 1, 0, 0, 0, 0) + tzOffsetMinutes * 60 * 1000;
  return new Date(ms);
}

function localEndOfYearUtc(year: number, tzOffsetMinutes: number) {
  const nextYearStartMs = Date.UTC(year + 1, 0, 1, 0, 0, 0, 0) + tzOffsetMinutes * 60 * 1000;
  return new Date(nextYearStartMs - 1);
}

export default async function DashboardPage({ searchParams }: Props) {
  const sp = searchParams ? await searchParams : {};

  const now = new Date();
  const currentYear = now.getFullYear();
  const currentMonth = now.getMonth() + 1;
  const currentDay = now.getDate();

  const mode = (pickFirst(sp.view) as DashboardViewMode | undefined) ?? "month";
  const tzOffsetRaw = pickFirst(sp.tzOffset);
  const tzOffsetMinutes =
    tzOffsetRaw === undefined ? clampTzOffsetMinutes(now.getTimezoneOffset()) : clampTzOffsetMinutes(Number(tzOffsetRaw) || 0);
  const topProductsSortBy = (pickFirst(sp.topProductsSortBy) as "revenue" | "quantity" | undefined) ?? "revenue";
  const performanceMetricRaw = pickFirst(sp.performanceMetric);
  const performanceMetric: PerformanceMetric =
    performanceMetricRaw === "marginPct" || performanceMetricRaw === "gains" || performanceMetricRaw === "avgTicket"
      ? performanceMetricRaw
      : "revenue";
  const forecastModelRaw = pickFirst(sp.forecastModel);
  const forecastModel: "auto" | ForecastModelKey =
    forecastModelRaw === "historical_average" || forecastModelRaw === "year_regression" ? forecastModelRaw : "auto";
  const includeForecast = pickFirst(sp.includeForecast) === "1";

  const compareYears = Array.from(
    new Set(
      pickAll(sp.compareYear)
        .map((value) => Number(value))
        .filter((value) => Number.isInteger(value) && value > 0),
    ),
  ).sort((a, b) => a - b);

  const ym = pickFirst(sp.ym);
  const y = pickFirst(sp.y);
  const m = pickFirst(sp.m);
  const d = pickFirst(sp.d);

  let year = currentYear;
  let month = currentMonth;
  let day = currentDay;

  if (d && /^\d{4}-\d{2}-\d{2}$/.test(d)) {
    const [yy, mm, dd] = d.split("-").map((x) => Number(x));
    if (Number.isFinite(yy) && Number.isFinite(mm) && Number.isFinite(dd)) {
      year = clamp(yy, 2000, 2100);
      month = clamp(mm, 1, 12);
      day = clamp(dd, 1, 31);
    }
  }

  if (ym && /^\d{4}-\d{2}$/.test(ym)) {
    const [yy, mm] = ym.split("-").map((x) => Number(x));
    if (Number.isFinite(yy) && Number.isFinite(mm)) {
      year = clamp(yy, 2000, 2100);
      month = clamp(mm, 1, 12);
    }
  }

  if (m && /^\d{2}$/.test(m)) {
    const mm = Number(m);
    if (Number.isFinite(mm)) month = clamp(mm, 1, 12);
  }

  if (y && /^\d{4}$/.test(y)) {
    const yy = Number(y);
    if (Number.isFinite(yy)) year = clamp(yy, 2000, 2100);
  }

  const range =
    mode === "all"
      ? { from: undefined, to: undefined, label: "All time", groupBy: "month" as const }
      : mode === "year"
        ? {
            from: toIso(localStartOfYearUtc(year, tzOffsetMinutes)),
            to: toIso(localEndOfYearUtc(year, tzOffsetMinutes)),
            label: String(year),
            groupBy: "month" as const,
          }
        : mode === "daily"
          ? {
              from: toIso(localStartOfDayUtc(year, month, day, tzOffsetMinutes)),
              to: toIso(localEndOfDayUtc(year, month, day, tzOffsetMinutes)),
              label: `${String(year).padStart(4, "0")}-${String(month).padStart(2, "0")}-${String(day).padStart(2, "0")}`,
              groupBy: "day" as const,
            }
          : {
              from: toIso(localStartOfMonthUtc(year, month, tzOffsetMinutes)),
              to: toIso(localEndOfMonthUtc(year, month, tzOffsetMinutes)),
              label: `${String(year).padStart(4, "0")}-${String(month).padStart(2, "0")}`,
              groupBy: "day" as const,
            };

  const comparisonRangeEnabled = Boolean(range.from && range.to);
  const effectiveCompareYears = comparisonRangeEnabled ? compareYears : [];
  const effectiveIncludeForecast = includeForecast && comparisonRangeEnabled && performanceMetric === "revenue";

  const [overview, performanceSeries, byPeriod, topProducts, costs, salesByCustomer, spendingByCustomer, promotionRecommendations] = await Promise.all([
    getDashboardOverview({ from: range.from, to: range.to }),
    getPerformanceSeries({
      groupBy: range.groupBy,
      from: range.from,
      to: range.to,
      tzOffsetMinutes,
      compareYears: effectiveCompareYears,
      forecastModel: forecastModel === "auto" ? undefined : forecastModel,
      includeForecast: effectiveIncludeForecast,
    }),
    getSalesByPeriod({ groupBy: range.groupBy, from: range.from, to: range.to, tzOffsetMinutes }),
    getTopProducts({ limit: 10, from: range.from, to: range.to, sortBy: topProductsSortBy }),
    getCosts({ startDate: range.from, endDate: range.to }),
    getSalesByCustomer({ limit: 10, from: range.from, to: range.to, excludeDebts: true }),
    getSpendingByCustomer({ limit: 10, from: range.from, to: range.to, excludeDebts: true }),
    getPromotionRecommendations({ limit: 5 }),
  ]);

  return (
    <DashboardView
      mode={mode}
      year={year}
      month={month}
      day={day}
      tzOffsetMinutes={tzOffsetMinutes}
      topProductsSortBy={topProductsSortBy}
      performanceMetric={performanceMetric}
      forecastModel={forecastModel}
      compareYears={compareYears}
      includeForecast={effectiveIncludeForecast}
      comparisonRangeEnabled={comparisonRangeEnabled}
      rangeLabel={range.label}
      rangeGroupBy={range.groupBy}
      overview={overview}
      performanceSeries={performanceSeries}
      byPeriod={byPeriod}
      costs={costs}
      topProducts={topProducts}
      salesByCustomer={salesByCustomer}
      spendingByCustomer={spendingByCustomer}
      promotionRecommendations={promotionRecommendations}
      effectiveCompareYears={effectiveCompareYears}
    />
  );
}
