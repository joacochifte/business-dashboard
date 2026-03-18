import type { ReactNode } from "react";

import type {
  CustomerSalesDto,
  CustomerSpendingDto,
  DashboardOverviewDto,
  DashboardPerformanceSeriesDto,
  ForecastModelKey,
  PromotionRecommendationDto,
  SalesByPeriodDto,
  TopProductDto,
} from "@/lib/dashboard.api";
import type { CostSummaryDto } from "@/lib/costs.api";

import PageShell from "@/app/ui/PageShell";
import ClientDateTime from "@/app/ui/ClientDateTime";

import TzOffsetField from "./TzOffsetField";
import TopProductsBarChart from "./TopProductsBarChart";
import PerformanceSeriesChart, { type PerformanceMetric } from "./PerformanceSeriesChart";
import PerformanceSeriesControls from "./PerformanceSeriesControls";

export type DashboardViewMode = "daily" | "month" | "year" | "all";

type Props = {
  mode: DashboardViewMode;
  year: number;
  month: number;
  day: number;
  tzOffsetMinutes: number;
  topProductsSortBy: "revenue" | "quantity";
  performanceMetric: PerformanceMetric;
  forecastModel: "auto" | ForecastModelKey;
  compareYears: number[];
  includeForecast: boolean;
  comparisonRangeEnabled: boolean;
  rangeLabel: string;
  rangeGroupBy: "day" | "month";
  overview: DashboardOverviewDto;
  performanceSeries: DashboardPerformanceSeriesDto;
  byPeriod: SalesByPeriodDto;
  costs: CostSummaryDto[];
  topProducts: TopProductDto[];
  salesByCustomer: CustomerSalesDto[];
  spendingByCustomer: CustomerSpendingDto[];
  promotionRecommendations: PromotionRecommendationDto[];
  effectiveCompareYears: number[];
};

function formatMoney(v: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(v);
}

function formatPercent(v: number) {
  return `${v.toFixed(1)}%`;
}

function formatScore(v: number) {
  return `${v.toFixed(0)}/100`;
}

function formatDeltaPct(v: number | null | undefined) {
  if (v === null || v === undefined) return "n/a *";
  if (v === 0) return "0.0% *";
  return `${v > 0 ? "+" : ""}${v.toFixed(1)}% *`;
}

function comparisonBadgeTone(v: number | null | undefined) {
  if (v === null || v === undefined || v === 0) return "border-black/10 bg-black/[0.04] text-neutral-600";
  return v > 0 ? "border-emerald-200 bg-emerald-50 text-emerald-800" : "border-red-200 bg-red-50 text-red-800";
}

function comparisonAccentTone(v: number | null | undefined) {
  if (v === null || v === undefined || v === 0) return "from-neutral-400 to-stone-300";
  return v > 0 ? "from-emerald-500 to-teal-500" : "from-red-500 to-rose-500";
}

function alertTone(kind: string) {
  switch (kind) {
    case "danger":
      return "border-red-200/80 bg-red-50/90 text-red-950";
    case "warning":
      return "border-amber-200/80 bg-amber-50/95 text-amber-950";
    default:
      return "border-sky-200/80 bg-sky-50/95 text-sky-950";
  }
}

function metricLabel(metric: PerformanceMetric) {
  switch (metric) {
    case "marginPct":
      return "Margin %";
    case "gains":
      return "Gains";
    case "avgTicket":
      return "Average ticket";
    default:
      return "Revenue";
  }
}

function getPeriodStartForCost(iso: string, groupBy: "day" | "month") {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;

  if (groupBy === "month") {
    return `${d.getUTCFullYear()}-${String(d.getUTCMonth() + 1).padStart(2, "0")}`;
  }

  return new Date(Date.UTC(d.getUTCFullYear(), d.getUTCMonth(), d.getUTCDate(), 0, 0, 0, 0)).toISOString();
}

type DateFilterStateInputsProps = {
  mode: DashboardViewMode;
  year: number;
  month: number;
  day: number;
  tzOffsetMinutes: number;
};

function DateFilterStateInputs({ mode, year, month, day, tzOffsetMinutes }: DateFilterStateInputsProps) {
  return (
    <>
      <input type="hidden" name="view" value={mode} />
      {mode === "daily" ? (
        <input
          type="hidden"
          name="d"
          value={`${String(year).padStart(4, "0")}-${String(month).padStart(2, "0")}-${String(day).padStart(2, "0")}`}
        />
      ) : null}
      {mode === "month" ? (
        <>
          <input type="hidden" name="y" value={year} />
          <input type="hidden" name="m" value={String(month).padStart(2, "0")} />
        </>
      ) : null}
      {mode === "year" ? <input type="hidden" name="y" value={year} /> : null}
      <input type="hidden" name="tzOffset" value={tzOffsetMinutes} />
    </>
  );
}

type PerformanceStateInputsProps = {
  performanceMetric: PerformanceMetric;
  forecastModel: "auto" | ForecastModelKey;
  compareYears: number[];
  includeForecast: boolean;
};

function PerformanceStateInputs({
  performanceMetric,
  forecastModel,
  compareYears,
  includeForecast,
}: PerformanceStateInputsProps) {
  return (
    <>
      <input type="hidden" name="performanceMetric" value={performanceMetric} />
      {forecastModel !== "auto" ? <input type="hidden" name="forecastModel" value={forecastModel} /> : null}
      {compareYears.map((value) => (
        <input key={value} type="hidden" name="compareYear" value={value} />
      ))}
      {includeForecast ? <input type="hidden" name="includeForecast" value="1" /> : null}
    </>
  );
}

type SectionCardProps = {
  eyebrow: string;
  title: string;
  description?: string;
  aside?: ReactNode;
  children: ReactNode;
  className?: string;
};

function SectionCard({ eyebrow, title, description, aside, children, className }: SectionCardProps) {
  return (
    <section
      className={`relative overflow-hidden rounded-[30px] border border-black/10 bg-white/76 p-5 shadow-[0_24px_80px_rgba(58,34,12,0.08)] backdrop-blur-xl sm:p-6 ${className ?? ""}`}
    >
      <div className="pointer-events-none absolute inset-x-0 top-0 h-24 bg-gradient-to-b from-white/80 to-transparent" />
      <div className="relative">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div className="space-y-1.5">
            <div className="text-[11px] font-semibold uppercase tracking-[0.22em] text-neutral-500">{eyebrow}</div>
            <div className="space-y-1">
              <h2 className="text-lg font-semibold text-neutral-950 sm:text-[1.4rem]">{title}</h2>
              {description ? <p className="max-w-2xl text-sm leading-6 text-neutral-600">{description}</p> : null}
            </div>
          </div>
          {aside ? <div className="shrink-0">{aside}</div> : null}
        </div>
        <div className="mt-6">{children}</div>
      </div>
    </section>
  );
}

type MetricCardProps = {
  label: string;
  value: string;
  subtitle: string;
  comparison?: number | null;
  valueClassName?: string;
};

function MetricCard({
  label,
  value,
  subtitle,
  comparison,
  valueClassName,
}: MetricCardProps) {
  return (
    <div className="relative overflow-hidden rounded-[28px] border border-black/10 bg-white/82 p-5 shadow-[0_14px_40px_rgba(50,35,20,0.06)] backdrop-blur">
      <div className={`absolute inset-x-0 top-0 h-1.5 bg-gradient-to-r ${comparisonAccentTone(comparison)}`} />
      <div className="relative">
        <div className="flex items-start justify-between gap-3">
          <div className="text-[11px] font-semibold uppercase tracking-[0.22em] text-neutral-500">{label}</div>
          {comparison !== undefined ? (
            <span className={`inline-flex rounded-full border px-2.5 py-1 text-[11px] font-medium ${comparisonBadgeTone(comparison)}`}>
              {formatDeltaPct(comparison)}
            </span>
          ) : null}
        </div>
        <div className={`mt-5 text-3xl font-semibold tracking-tight text-neutral-950 sm:text-[2rem] ${valueClassName ?? ""}`}>{value}</div>
        <div className="mt-2 text-sm text-neutral-600">{subtitle}</div>
      </div>
    </div>
  );
}

type InsightCardProps = {
  eyebrow: string;
  title: string;
  detail: string;
  meta: string;
};

function InsightCard({ eyebrow, title, detail, meta }: InsightCardProps) {
  return (
    <div className="rounded-[26px] border border-black/10 bg-white/82 p-5 shadow-sm">
      <div className="h-1.5 w-14 rounded-full bg-gradient-to-r from-neutral-500 to-stone-400" />
      <div className="mt-4">
        <div className="text-[11px] font-semibold uppercase tracking-[0.2em] text-neutral-500">{eyebrow}</div>
        <div className="mt-3 text-xl font-semibold text-neutral-950">{title}</div>
        <div className="mt-2 text-sm leading-6 text-neutral-700">{detail}</div>
        <div className="mt-4 inline-flex rounded-full border border-black/10 bg-white px-3 py-1 text-xs font-medium text-neutral-700">
          {meta}
        </div>
      </div>
    </div>
  );
}

function EmptyState({ label }: { label: string }) {
  return (
    <div className="rounded-[24px] border border-dashed border-black/10 bg-white/55 px-4 py-10 text-center text-sm text-neutral-600">
      {label}
    </div>
  );
}

type ProgressRowProps = {
  label: ReactNode;
  value: ReactNode;
  width: number;
  toneClass: string;
  ariaLabel: string;
};

function ProgressRow({ label, value, width, toneClass, ariaLabel }: ProgressRowProps) {
  const clampedWidth = Math.max(0, Math.min(100, width));
  const barWidth = clampedWidth > 0 ? `max(${clampedWidth}%, 12px)` : "0%";

  return (
    <div className="grid gap-3 rounded-[22px] border border-black/5 bg-white/55 px-4 py-3 sm:grid-cols-[minmax(0,1fr)_minmax(0,1.5fr)_auto] sm:items-center">
      <div className="min-w-0 text-sm font-medium text-neutral-800">{label}</div>
      <div className="h-2.5 w-full rounded-full bg-black/[0.06]">
        <div
          className={`h-2.5 rounded-full bg-gradient-to-r ${toneClass}`}
          style={{ width: barWidth }}
          aria-label={ariaLabel}
        />
      </div>
      <div className="text-right text-sm font-semibold tabular-nums text-neutral-900">{value}</div>
    </div>
  );
}

function PromotionRecommendationCard({ recommendation }: { recommendation: PromotionRecommendationDto }) {
  return (
    <article className="min-w-[280px] snap-start rounded-[24px] border border-black/10 bg-white/82 p-4 shadow-sm">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <div className="truncate text-base font-semibold text-neutral-950">{recommendation.customerName}</div>
          <div className="mt-1 truncate text-xs text-neutral-600">{recommendation.reason}</div>
        </div>
        <span className="inline-flex shrink-0 rounded-full border border-emerald-200 bg-emerald-50 px-2.5 py-1 text-[11px] font-semibold text-emerald-800">
          {formatScore(recommendation.score)}
        </span>
      </div>

      <div className="mt-4 rounded-[16px] border border-black/8 bg-white/80 px-3 py-2.5">
        <div className="text-[10px] font-semibold uppercase tracking-[0.16em] text-neutral-500">Recommended product</div>
        <div className="mt-1 truncate font-medium text-neutral-950">
          {recommendation.recommendedProductName?.trim() ? recommendation.recommendedProductName : "No product suggestion"}
        </div>
        {recommendation.productRecommendationReason ? (
          <div className="mt-1 text-xs text-neutral-600">{recommendation.productRecommendationReason}</div>
        ) : null}
      </div>

      <div className="mt-4 grid grid-cols-2 gap-2 text-sm">
        <div className="rounded-[16px] border border-black/8 bg-white/80 px-3 py-2">
          <div className="text-[10px] font-semibold uppercase tracking-[0.16em] text-neutral-500">Last purchase</div>
          <div className="mt-1 font-medium text-neutral-900">{recommendation.daysSinceLastPurchase}d ago</div>
        </div>
        <div className="rounded-[16px] border border-black/8 bg-white/80 px-3 py-2">
          <div className="text-[10px] font-semibold uppercase tracking-[0.16em] text-neutral-500">Frequency</div>
          <div className="mt-1 font-medium text-neutral-900">{recommendation.purchasesLast90Days} in 90d</div>
        </div>
        <div className="rounded-[16px] border border-black/8 bg-white/80 px-3 py-2">
          <div className="text-[10px] font-semibold uppercase tracking-[0.16em] text-neutral-500">Avg ticket</div>
          <div className="mt-1 font-medium text-neutral-900">{formatMoney(recommendation.avgTicket)}</div>
        </div>
        <div className="rounded-[16px] border border-black/8 bg-white/80 px-3 py-2">
          <div className="text-[10px] font-semibold uppercase tracking-[0.16em] text-neutral-500">Debt ratio</div>
          <div className={`mt-1 font-medium ${recommendation.debtRatioPct >= 40 ? "text-red-700" : "text-neutral-900"}`}>
            {formatPercent(recommendation.debtRatioPct)}
          </div>
        </div>
      </div>
    </article>
  );
}

export default function DashboardView({
  mode,
  year,
  month,
  day,
  tzOffsetMinutes,
  topProductsSortBy,
  performanceMetric,
  forecastModel,
  compareYears,
  includeForecast,
  comparisonRangeEnabled,
  rangeLabel,
  rangeGroupBy,
  overview,
  performanceSeries,
  byPeriod,
  costs,
  topProducts,
  salesByCustomer,
  spendingByCustomer,
  promotionRecommendations,
  effectiveCompareYears,
}: Props) {
  const points = byPeriod.points ?? [];
  const maxRevenue = points.reduce((m, p) => Math.max(m, p.revenue), 0);

  const costsGroupBy: "day" | "month" = rangeGroupBy === "month" ? "month" : "day";
  const costPointsMap = new Map<string, number>();
  for (const c of costs) {
    const key = getPeriodStartForCost(c.dateIncurred, costsGroupBy);
    costPointsMap.set(key, (costPointsMap.get(key) ?? 0) + c.amount);
  }
  const costsByPeriodPoints = Array.from(costPointsMap.entries())
    .map(([periodStart, amount]) => ({ periodStart, amount }))
    .sort((a, b) => a.periodStart.localeCompare(b.periodStart));
  const maxCostAmount = costsByPeriodPoints.reduce((m, p) => Math.max(m, p.amount), 0);

  const visibleForecastPoints = performanceSeries.forecastSeries?.points.length ?? 0;
  const activeComparisons = effectiveCompareYears.length;

  return (
    <PageShell>
      <SectionCard
        eyebrow="Filters"
        title="Filters"
        description="Range, mode and comparison settings."
      >
        <form className="grid gap-4 lg:grid-cols-12" method="GET">
          <TzOffsetField />
          <input type="hidden" name="topProductsSortBy" value={topProductsSortBy} />
          <PerformanceStateInputs
            performanceMetric={performanceMetric}
            forecastModel={forecastModel}
            compareYears={compareYears}
            includeForecast={includeForecast}
          />

          <label className="grid gap-1.5 lg:col-span-4">
            <span className="text-[11px] font-semibold uppercase tracking-[0.18em] text-neutral-500">View</span>
            <select
              name="view"
              defaultValue={mode}
              className="rounded-[20px] border border-black/10 bg-white/75 px-4 py-3 text-sm font-medium text-neutral-900 outline-none transition focus:border-black/20 focus:ring-2 focus:ring-black/5"
            >
              <option value="daily">Daily</option>
              <option value="month">Month</option>
              <option value="year">Year</option>
              <option value="all">All time</option>
            </select>
          </label>

          <label className="grid gap-1.5 lg:col-span-4">
            <span className="text-[11px] font-semibold uppercase tracking-[0.18em] text-neutral-500">Month</span>
            <select
              name="m"
              defaultValue={String(month).padStart(2, "0")}
              disabled={mode !== "month"}
              className="rounded-[20px] border border-black/10 bg-white/75 px-4 py-3 text-sm font-medium text-neutral-900 outline-none transition focus:border-black/20 focus:ring-2 focus:ring-black/5 disabled:bg-white/40 disabled:text-neutral-500"
            >
              {Array.from({ length: 12 }, (_, i) => {
                const value = String(i + 1).padStart(2, "0");
                return (
                  <option key={value} value={value}>
                    {value}
                  </option>
                );
              })}
            </select>
          </label>

          <label className="grid gap-1.5 lg:col-span-2">
            <span className="text-[11px] font-semibold uppercase tracking-[0.18em] text-neutral-500">Year</span>
            <input
              type="number"
              name="y"
              min={2000}
              max={2100}
              defaultValue={year}
              disabled={mode !== "year" && mode !== "month"}
              className="rounded-[20px] border border-black/10 bg-white/75 px-4 py-3 text-sm font-medium text-neutral-900 outline-none transition focus:border-black/20 focus:ring-2 focus:ring-black/5 disabled:bg-white/40 disabled:text-neutral-500"
            />
          </label>

          <label className="grid gap-1.5 lg:col-span-2">
            <span className="text-[11px] font-semibold uppercase tracking-[0.18em] text-neutral-500">Day</span>
            <input
              type="date"
              name="d"
              defaultValue={`${String(year).padStart(4, "0")}-${String(month).padStart(2, "0")}-${String(day).padStart(2, "0")}`}
              disabled={mode !== "daily"}
              className="rounded-[20px] border border-black/10 bg-white/75 px-4 py-3 text-sm font-medium text-neutral-900 outline-none transition focus:border-black/20 focus:ring-2 focus:ring-black/5 disabled:bg-white/40 disabled:text-neutral-500"
            />
          </label>

          <div className="flex items-end lg:col-span-12">
            <button
              type="submit"
              className="inline-flex rounded-[20px] bg-neutral-950 px-5 py-3 text-sm font-semibold text-white shadow-[0_12px_24px_rgba(15,23,42,0.18)] transition hover:bg-neutral-900"
            >
              Apply dashboard view
            </button>
          </div>
        </form>
      </SectionCard>

      <section className="mt-6">
        <div className="mb-3 text-xs font-medium text-neutral-500">* compared with previous period</div>
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <MetricCard
          label="Revenue total"
          value={formatMoney(overview.revenueTotal)}
          subtitle="Gross revenue in the selected window."
          comparison={overview.comparison.revenueDeltaPct}
        />
        <MetricCard
          label="Costs total"
          value={formatMoney(overview.costsTotal)}
          subtitle="Operating spend captured in the same period."
          comparison={overview.comparison.costsDeltaPct}
          valueClassName="text-amber-700"
        />
        <MetricCard
          label="Gains"
          value={formatMoney(overview.gains)}
          subtitle="Revenue minus tracked costs."
          comparison={overview.comparison.gainsDeltaPct}
          valueClassName={overview.gains < 0 ? "text-red-700" : "text-emerald-700"}
        />
        <MetricCard
          label="Margin"
          value={formatPercent(overview.marginPct)}
          subtitle="Efficiency of every revenue dollar."
        />
        <MetricCard
          label="Sales count"
          value={String(overview.salesCount)}
          subtitle="Transactions in the visible window."
          comparison={overview.comparison.salesCountDeltaPct}
        />
        <MetricCard
          label="Units sold"
          value={String(overview.unitsSold)}
          subtitle="Non-debt units shipped or handed over."
          comparison={overview.comparison.unitsSoldDeltaPct}
        />
        <MetricCard
          label="Average ticket"
          value={formatMoney(overview.avgTicket)}
          subtitle="Average order value per sale."
        />
        <MetricCard
          label="Debt ratio"
          value={formatPercent(overview.debtRatioPct)}
          subtitle={`${formatMoney(overview.debtsTotal)} currently sits in debt sales.`}
          valueClassName={overview.debtRatioPct >= 20 ? "text-red-700" : "text-amber-800"}
        />
        </div>
      </section>

      {overview.alerts.length > 0 ? (
        <section className="mt-6 grid gap-3 xl:grid-cols-3">
          {overview.alerts.map((alert) => (
            <div
              key={`${alert.kind}-${alert.title}`}
              className={`rounded-[26px] border px-5 py-4 shadow-sm backdrop-blur ${alertTone(alert.kind)}`}
            >
              <div className="text-[11px] font-semibold uppercase tracking-[0.2em] opacity-80">{alert.kind}</div>
              <div className="mt-3 text-base font-semibold">{alert.title}</div>
              <p className="mt-2 text-sm leading-6 opacity-90">{alert.detail}</p>
            </div>
          ))}
        </section>
      ) : null}

      <section className="mt-6 grid gap-4 xl:grid-cols-3">
        <InsightCard
          eyebrow="Customer pulse"
          title={overview.topCustomer?.customerName?.trim() ? overview.topCustomer.customerName : "No customer sales yet"}
          detail={overview.topCustomer ? formatMoney(overview.topCustomer.totalSpent) : "No customer spending detected for the selected period."}
          meta="Highest spending customer"
        />
        <InsightCard
          eyebrow="Product traction"
          title={overview.topProductByQuantity?.productName?.trim() ? overview.topProductByQuantity.productName : "No product movement yet"}
          detail={
            overview.topProductByQuantity
              ? `${overview.topProductByQuantity.quantity} units sold in the selected period.`
              : "No product volume detected for the selected period."
          }
          meta="Most demanded product"
        />
        <InsightCard
          eyebrow="Inventory pressure"
          title={`${overview.lowStockCount} low stock / ${overview.outOfStockCount} out of stock`}
          detail="Use this as the fast signal for replenishment risk before revenue gets hit."
          meta="Tracked active products only"
        />
      </section>

      <SectionCard
        eyebrow="Promotion opportunities"
        title="Customers with the strongest promo signal"
        description="Baseline RFM scoring prioritizes recent, frequent and healthy customers while penalizing high debt exposure."
        className="mt-6"
      >
        <div className="space-y-3">
          {promotionRecommendations.length === 0 ? (
            <EmptyState label="No promotion recommendations yet." />
          ) : (
            <div className="flex snap-x snap-mandatory gap-3 overflow-x-auto pb-2">
              {promotionRecommendations.map((recommendation) => (
                <PromotionRecommendationCard key={recommendation.customerId} recommendation={recommendation} />
              ))}
            </div>
          )}
        </div>
      </SectionCard>

      <SectionCard
        eyebrow="Performance"
        title="Trend line, comparisons and forecast in one place"
        description="The chart is now paired with context cards so it feels less like a raw graph and more like a guided decision panel."
        aside={
          <div className="inline-flex rounded-full border border-black/10 bg-white/80 px-3.5 py-1.5 text-xs font-medium text-neutral-700">
            Range: {rangeLabel}
          </div>
        }
        className="mt-6"
      >
        <div className="grid gap-4 xl:grid-cols-[1.3fr_0.7fr]">
          <div className="rounded-[28px] border border-black/10 bg-[linear-gradient(180deg,rgba(255,255,255,0.85),rgba(250,245,237,0.8))] p-4 shadow-inner sm:p-5">
            <PerformanceSeriesControls
              performanceMetric={performanceMetric}
              forecastModel={forecastModel}
              compareYears={compareYears}
              includeForecast={includeForecast}
              comparisonRangeEnabled={comparisonRangeEnabled}
            />

            <div className="mt-4 rounded-[26px] border border-black/10 bg-white/82 p-3 shadow-[inset_0_1px_0_rgba(255,255,255,0.7)] sm:p-4">
              <PerformanceSeriesChart series={performanceSeries} metric={performanceMetric} />
            </div>
          </div>

          <div className="grid gap-4">
            <div className="rounded-[26px] border border-black/10 bg-white/80 p-5 shadow-sm">
              <div className="text-[11px] font-semibold uppercase tracking-[0.2em] text-neutral-500">Selected metric</div>
              <div className="mt-3 text-2xl font-semibold text-neutral-950">{metricLabel(performanceMetric)}</div>
              <p className="mt-2 text-sm leading-6 text-neutral-600">
                Switch between revenue, gains, margin and average ticket without resetting your comparison setup.
              </p>
            </div>

            <div className="rounded-[26px] border border-black/10 bg-white/80 p-5 shadow-sm">
              <div className="text-[11px] font-semibold uppercase tracking-[0.2em] text-neutral-500">Comparison context</div>
              <div className="mt-3 text-2xl font-semibold text-neutral-950">{activeComparisons}</div>
              <p className="mt-2 text-sm leading-6 text-neutral-600">
                {activeComparisons > 0
                  ? "Previous-year overlays stay aligned to the same position inside the selected period."
                  : "Add previous years to turn the chart into a stronger reference tool."}
              </p>
            </div>

            <div className="rounded-[26px] border border-black/10 bg-white/80 p-5 shadow-sm">
              <div className="text-[11px] font-semibold uppercase tracking-[0.2em] text-neutral-500">Forecast logic</div>
              <div className="mt-3 text-2xl font-semibold text-neutral-950">
                {includeForecast && performanceMetric === "revenue" ? `${visibleForecastPoints} future point(s)` : "Revenue only"}
              </div>
              <p className="mt-2 text-sm leading-6 text-neutral-600">
                {forecastModel === "auto"
                  ? "When forecast is enabled, the backend picks the best available strategy automatically."
                  : `Using ${forecastModel === "historical_average" ? "Historical average" : "Year regression"} as the requested forecast model.`}
              </p>
            </div>
          </div>
        </div>
      </SectionCard>

      <section className="mt-6 grid gap-4 xl:grid-cols-2">
        <SectionCard
          eyebrow="Revenue flow"
          title="Sales by period"
          description={`Grouped by ${byPeriod.groupBy}. Useful for spotting peaks, weak stretches and consistency.`}
        >
          <div className="space-y-3">
            {points.length === 0 ? (
              <EmptyState label="No sales data yet." />
            ) : (
              points.map((p) => {
                const width = maxRevenue <= 0 ? 0 : Math.round((p.revenue / maxRevenue) * 100);
                return (
                  <ProgressRow
                    key={p.periodStart}
                    label={
                      byPeriod.groupBy === "month" ? (
                        <span className="tabular-nums">{p.periodStart.slice(0, 7)}</span>
                      ) : (
                        <ClientDateTime iso={p.periodStart} variant="date" />
                      )
                    }
                    value={formatMoney(p.revenue)}
                    width={width}
                    toneClass="from-neutral-900 via-neutral-800 to-amber-700"
                    ariaLabel={`Revenue ${p.revenue}`}
                  />
                );
              })
            )}
          </div>
        </SectionCard>

        <SectionCard
          eyebrow="Cost flow"
          title="Costs by period"
          description={`Grouped by ${costsGroupBy}. Lets you compare pressure from spending against the revenue rhythm.`}
        >
          <div className="space-y-3">
            {costsByPeriodPoints.length === 0 ? (
              <EmptyState label="No cost data yet." />
            ) : (
              costsByPeriodPoints.map((p) => {
                const width = maxCostAmount <= 0 ? 0 : Math.round((p.amount / maxCostAmount) * 100);
                return (
                  <ProgressRow
                    key={p.periodStart}
                    label={
                      costsGroupBy === "month" ? (
                        <span className="tabular-nums">{p.periodStart.slice(0, 7)}</span>
                      ) : (
                        <ClientDateTime iso={p.periodStart} variant="date" />
                      )
                    }
                    value={formatMoney(p.amount)}
                    width={width}
                    toneClass="from-amber-700 via-orange-600 to-rose-600"
                    ariaLabel={`Costs ${p.amount}`}
                  />
                );
              })
            )}
          </div>
        </SectionCard>
      </section>

      <SectionCard
        eyebrow="Product mix"
        title="Top products"
        description={topProductsSortBy === "quantity" ? "A volume-first view of demand." : "A revenue-first view of contribution."}
        aside={
          <form method="get" className="flex flex-wrap gap-2">
            <DateFilterStateInputs mode={mode} year={year} month={month} day={day} tzOffsetMinutes={tzOffsetMinutes} />
            <PerformanceStateInputs
              performanceMetric={performanceMetric}
              forecastModel={forecastModel}
              compareYears={compareYears}
              includeForecast={includeForecast}
            />
            <select
              name="topProductsSortBy"
              defaultValue={topProductsSortBy}
              className="rounded-[18px] border border-black/10 bg-white/80 px-3.5 py-2 text-xs font-medium text-neutral-800 shadow-sm transition hover:bg-white focus:border-black/20 focus:outline-none focus:ring-2 focus:ring-black/5"
            >
              <option value="revenue">By revenue</option>
              <option value="quantity">By quantity</option>
            </select>
            <button
              type="submit"
              className="rounded-[18px] bg-neutral-950 px-3.5 py-2 text-xs font-semibold text-white shadow-sm transition hover:bg-neutral-900"
            >
              Apply
            </button>
          </form>
        }
        className="mt-6"
      >
        <div className="rounded-[28px] border border-black/10 bg-[linear-gradient(180deg,rgba(255,255,255,0.9),rgba(249,244,236,0.86))] p-4">
          <TopProductsBarChart data={topProducts} sortBy={topProductsSortBy} />
        </div>

        <div className="mt-5 overflow-x-auto rounded-[26px] border border-black/10 bg-white/65">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="bg-white/75 text-left">
                <th className="px-4 py-3 font-semibold text-neutral-700">Product</th>
                <th className="px-4 py-3 text-right font-semibold text-neutral-700">Quantity</th>
                <th className="px-4 py-3 text-right font-semibold text-neutral-700">Revenue</th>
              </tr>
            </thead>
            <tbody>
              {topProducts.map((p) => (
                <tr key={p.productId} className="border-t border-black/5 transition hover:bg-white/70">
                  <td className="px-4 py-3 font-medium text-neutral-900">{p.productName || p.productId}</td>
                  <td className="px-4 py-3 text-right tabular-nums text-neutral-900">{p.quantity}</td>
                  <td className="px-4 py-3 text-right tabular-nums text-neutral-900">{formatMoney(p.revenue)}</td>
                </tr>
              ))}
              {topProducts.length === 0 ? (
                <tr>
                  <td colSpan={3} className="px-4 py-12 text-center text-sm text-neutral-600">
                    No product data yet.
                  </td>
                </tr>
              ) : null}
            </tbody>
          </table>
        </div>
      </SectionCard>

      <section className="mt-6 grid gap-4 xl:grid-cols-2">
        <SectionCard
          eyebrow="Customer activity"
          title="Purchases by customer"
          description="Transaction count is a quick proxy for stickiness and visit frequency."
        >
          <div className="space-y-3">
            {salesByCustomer.length === 0 ? (
              <EmptyState label="No customer purchases yet." />
            ) : (
              (() => {
                const maxSales = salesByCustomer.reduce((m, x) => Math.max(m, x.salesCount), 0);
                return salesByCustomer.map((x) => {
                  const width = maxSales <= 0 ? 0 : Math.round((x.salesCount / maxSales) * 100);
                  return (
                    <ProgressRow
                      key={x.customerId}
                      label={<span className="block truncate">{x.customerName || "—"}</span>}
                      value={x.salesCount}
                      width={width}
                      toneClass="from-sky-500 via-blue-500 to-indigo-600"
                      ariaLabel={`Sales count ${x.salesCount}`}
                    />
                  );
                });
              })()
            )}
          </div>
        </SectionCard>

        <SectionCard
          eyebrow="Customer value"
          title="Spending by customer"
          description="Shows who drives the most revenue, not just the most visits."
        >
          <div className="space-y-3">
            {spendingByCustomer.length === 0 ? (
              <EmptyState label="No customer spending yet." />
            ) : (
              (() => {
                const maxSpending = spendingByCustomer.reduce((m, x) => Math.max(m, x.totalSpent), 0);
                return spendingByCustomer.map((x) => {
                  const width = maxSpending <= 0 ? 0 : Math.round((x.totalSpent / maxSpending) * 100);
                  return (
                    <ProgressRow
                      key={x.customerId}
                      label={<span className="block truncate">{x.customerName || "—"}</span>}
                      value={formatMoney(x.totalSpent)}
                      width={width}
                      toneClass="from-emerald-500 via-teal-500 to-green-600"
                      ariaLabel={`Total spent ${x.totalSpent}`}
                    />
                  );
                });
              })()
            )}
          </div>
        </SectionCard>
      </section>
    </PageShell>
  );
}
