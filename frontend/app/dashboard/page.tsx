import {
  getDashboardOverview,
  getPerformanceSeries,
  getSalesByPeriod,
  getTopProducts,
  getSalesByCustomer,
  getSpendingByCustomer,
} from "@/lib/dashboard.api";
import { getCosts, type CostSummaryDto } from "@/lib/costs.api";
import TopProductsBarChart from "./ui/TopProductsBarChart";
import PerformanceSeriesChart, { type PerformanceMetric } from "./ui/PerformanceSeriesChart";
import PerformanceSeriesControls from "./ui/PerformanceSeriesControls";
import PageShell from "../ui/PageShell";
import ClientDateTime from "../ui/ClientDateTime";
import TzOffsetField from "./ui/TzOffsetField";

function formatMoney(v: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(v);
}

function formatPercent(v: number) {
  return `${v.toFixed(1)}%`;
}

function formatDeltaPct(v: number | null | undefined) {
  if (v === null || v === undefined) return "No previous period";
  if (v === 0) return "0.0% vs previous period";
  return `${v > 0 ? "+" : ""}${v.toFixed(1)}% vs previous period`;
}

function deltaTone(v: number | null | undefined, reverse = false) {
  if (v === null || v === undefined || v === 0) return "text-neutral-500";
  const positive = reverse ? v < 0 : v > 0;
  return positive ? "text-emerald-700" : "text-red-700";
}

function alertTone(kind: string) {
  switch (kind) {
    case "danger":
      return "border-red-200/80 bg-red-50/85 text-red-950";
    case "warning":
      return "border-amber-200/80 bg-amber-50/90 text-amber-950";
    default:
      return "border-sky-200/80 bg-sky-50/85 text-sky-950";
  }
}

type MetricCardProps = {
  label: string;
  value: string;
  subtitle: string;
  comparison?: number | null;
  reverseComparison?: boolean;
  valueClassName?: string;
};

function MetricCard({ label, value, subtitle, comparison, reverseComparison = false, valueClassName }: MetricCardProps) {
  return (
    <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
      <div className="text-xs font-medium text-neutral-600">{label}</div>
      <div className={`mt-2 text-2xl font-semibold tabular-nums ${valueClassName ?? ""}`}>{value}</div>
      <div className="mt-1 space-y-1">
        <div className="text-xs text-neutral-500">{subtitle}</div>
        {comparison !== undefined ? (
          <div className={`text-xs font-medium ${deltaTone(comparison, reverseComparison)}`}>{formatDeltaPct(comparison)}</div>
        ) : null}
      </div>
    </div>
  );
}

type ViewMode = "daily" | "month" | "year" | "all";

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

function getPeriodStartForCost(iso: string, groupBy: "day" | "month") {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;

  if (groupBy === "month") {
    return `${d.getUTCFullYear()}-${String(d.getUTCMonth() + 1).padStart(2, "0")}`;
  }

  return new Date(Date.UTC(d.getUTCFullYear(), d.getUTCMonth(), d.getUTCDate(), 0, 0, 0, 0)).toISOString();
}

type DateFilterStateInputsProps = {
  mode: ViewMode;
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
  compareYears: number[];
  compareMonthsRaw: string;
  includeForecast: boolean;
  forecastPeriods: number;
};

function PerformanceStateInputs({
  performanceMetric,
  compareYears,
  compareMonthsRaw,
  includeForecast,
  forecastPeriods,
}: PerformanceStateInputsProps) {
  return (
    <>
      <input type="hidden" name="performanceMetric" value={performanceMetric} />
      {compareYears.map((value) => (
        <input key={value} type="hidden" name="compareYear" value={value} />
      ))}
      {compareMonthsRaw ? <input type="hidden" name="compareMonths" value={compareMonthsRaw} /> : null}
      {includeForecast ? <input type="hidden" name="includeForecast" value="1" /> : null}
      <input type="hidden" name="forecastPeriods" value={forecastPeriods} />
    </>
  );
}

export default async function DashboardPage({ searchParams }: Props) {
  const sp = searchParams ? await searchParams : {};

  const now = new Date();
  // In local dev, the Next.js server runs on your machine, so local time matches your PC.
  // In a hosted environment, "local" would be the server locale; tzOffset param can override on form submit.
  const currentYear = now.getFullYear();
  const currentMonth = now.getMonth() + 1;
  const currentDay = now.getDate();

  const mode = (pickFirst(sp.view) as ViewMode | undefined) ?? "month";
  const tzOffsetRaw = pickFirst(sp.tzOffset);
  const tzOffsetMinutes =
    tzOffsetRaw === undefined ? clampTzOffsetMinutes(now.getTimezoneOffset()) : clampTzOffsetMinutes(Number(tzOffsetRaw) || 0);
  const topProductsSortBy = (pickFirst(sp.topProductsSortBy) as "revenue" | "quantity" | undefined) ?? "revenue";
  const performanceMetricRaw = pickFirst(sp.performanceMetric);
  const performanceMetric: PerformanceMetric =
    performanceMetricRaw === "marginPct" || performanceMetricRaw === "gains" || performanceMetricRaw === "avgTicket"
      ? performanceMetricRaw
      : "revenue";
  const includeForecast = pickFirst(sp.includeForecast) === "1";
  const forecastPeriods = clamp(Number(pickFirst(sp.forecastPeriods) ?? 3) || 3, 1, 12);
  const compareYears = Array.from(
    new Set(
      pickAll(sp.compareYear)
        .map((value) => Number(value))
        .filter((value) => Number.isInteger(value) && value > 0),
    ),
  ).sort((a, b) => a - b);
  const compareMonthsRaw = (pickFirst(sp.compareMonths) ?? "").trim();
  const compareMonths = Array.from(
    new Set(
      compareMonthsRaw
        .split(",")
        .map((value) => value.trim())
        .filter((value) => /^\d{4}-\d{2}$/.test(value)),
    ),
  ).sort();

  const ym = pickFirst(sp.ym); // legacy "YYYY-MM"
  const y = pickFirst(sp.y); // "YYYY"
  const m = pickFirst(sp.m); // "01".."12"
  const d = pickFirst(sp.d); // "YYYY-MM-DD"

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
  const effectiveCompareMonths = comparisonRangeEnabled && range.groupBy === "day" ? compareMonths : [];

  const [overview, performanceSeries, byPeriod, topProducts, costs, salesByCustomer, spendingByCustomer] = await Promise.all([
    getDashboardOverview({ from: range.from, to: range.to }),
    getPerformanceSeries({
      groupBy: range.groupBy,
      from: range.from,
      to: range.to,
      tzOffsetMinutes,
      compareYears: effectiveCompareYears,
      compareMonths: effectiveCompareMonths,
      includeForecast: includeForecast && performanceMetric === "revenue",
      forecastPeriods,
    }),
    getSalesByPeriod({ groupBy: range.groupBy, from: range.from, to: range.to, tzOffsetMinutes }),
    getTopProducts({ limit: 10, from: range.from, to: range.to, sortBy: topProductsSortBy }),
    getCosts({ startDate: range.from, endDate: range.to }),
    getSalesByCustomer({ limit: 10, from: range.from, to: range.to, excludeDebts: true }),
    getSpendingByCustomer({ limit: 10, from: range.from, to: range.to, excludeDebts: true }),
  ]);

  const points = byPeriod.points ?? [];
  const maxRevenue = points.reduce((m, p) => Math.max(m, p.revenue), 0);

  const costsGroupBy: "day" | "month" = range.groupBy === "month" ? "month" : "day";
  const costPointsMap = new Map<string, number>();
  for (const c of costs as CostSummaryDto[]) {
    const key = getPeriodStartForCost(c.dateIncurred, costsGroupBy);
    costPointsMap.set(key, (costPointsMap.get(key) ?? 0) + c.amount);
  }
  const costsByPeriodPoints = Array.from(costPointsMap.entries())
    .map(([periodStart, amount]) => ({ periodStart, amount }))
    .sort((a, b) => a.periodStart.localeCompare(b.periodStart));
  const maxCostAmount = costsByPeriodPoints.reduce((m, p) => Math.max(m, p.amount), 0);

  return (
    <PageShell>
      <p className="text-sm text-neutral-600">High-level metrics for your business.</p>
      <section className="mt-4 rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
        <form className="grid gap-3 md:grid-cols-12" method="GET">
          <TzOffsetField />
          <input type="hidden" name="topProductsSortBy" value={topProductsSortBy} />
          <PerformanceStateInputs
            performanceMetric={performanceMetric}
            compareYears={compareYears}
            compareMonthsRaw={compareMonthsRaw}
            includeForecast={includeForecast}
            forecastPeriods={forecastPeriods}
          />
          <div className="md:col-span-4">
            <label className="grid gap-1">
              <span className="text-xs font-medium text-neutral-700">View</span>
              <select
                name="view"
                defaultValue={mode}
                className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
              >
                <option value="daily">Daily</option>
                <option value="month">Month</option>
                <option value="year">Year</option>
                <option value="all">All time</option>
              </select>
            </label>
          </div>

          <div className="md:col-span-4">
            <label className="grid gap-1">
              <span className="text-xs font-medium text-neutral-700">Month</span>
              <select
                name="m"
                defaultValue={String(month).padStart(2, "0")}
                disabled={mode !== "month"}
                className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5 disabled:bg-white/40 disabled:text-neutral-500"
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
          </div>

          <div className="md:col-span-2">
            <label className="grid gap-1">
              <span className="text-xs font-medium text-neutral-700">Year</span>
              <input
                type="number"
                name="y"
                min={2000}
                max={2100}
                defaultValue={year}
                disabled={mode !== "year" && mode !== "month"}
                className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5 disabled:bg-white/40 disabled:text-neutral-500"
              />
            </label>
          </div>

          <div className="md:col-span-2">
            <label className="grid gap-1">
              <span className="text-xs font-medium text-neutral-700">Day</span>
              <input
                type="date"
                name="d"
                defaultValue={`${String(year).padStart(4, "0")}-${String(month).padStart(2, "0")}-${String(day).padStart(2, "0")}`}
                disabled={mode !== "daily"}
                className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5 disabled:bg-white/40 disabled:text-neutral-500"
              />
            </label>
          </div>

          <div className="flex items-end md:col-span-2">
            <button
              type="submit"
              className="w-full rounded-xl bg-black px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-black/90"
            >
              Apply
            </button>
          </div>

        </form>
      </section>

      <section className="mt-6 grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <MetricCard
          label="Revenue total"
          value={formatMoney(overview.revenueTotal)}
          subtitle="In selected period"
          comparison={overview.comparison.revenueDeltaPct}
        />
        <MetricCard
          label="Costs total"
          value={formatMoney(overview.costsTotal)}
          subtitle="In selected period"
          comparison={overview.comparison.costsDeltaPct}
          reverseComparison
          valueClassName="text-amber-700"
        />
        <MetricCard
          label="Gains"
          value={formatMoney(overview.gains)}
          subtitle="Revenue - costs"
          comparison={overview.comparison.gainsDeltaPct}
          valueClassName={overview.gains < 0 ? "text-red-700" : "text-emerald-700"}
        />
        <MetricCard
          label="Margin"
          value={formatPercent(overview.marginPct)}
          subtitle="Gains / revenue"
        />
        <MetricCard
          label="Sales count"
          value={String(overview.salesCount)}
          subtitle="Transactions"
          comparison={overview.comparison.salesCountDeltaPct}
        />
        <MetricCard
          label="Units sold"
          value={String(overview.unitsSold)}
          subtitle="Items in non-debt sales"
          comparison={overview.comparison.unitsSoldDeltaPct}
        />
        <MetricCard
          label="Average ticket"
          value={formatMoney(overview.avgTicket)}
          subtitle="Revenue / sales"
        />
        <MetricCard
          label="Debt ratio"
          value={formatPercent(overview.debtRatioPct)}
          subtitle={`${formatMoney(overview.debtsTotal)} in debt sales`}
          valueClassName={overview.debtRatioPct >= 20 ? "text-red-700" : "text-amber-800"}
        />
      </section>

      {overview.alerts.length > 0 ? (
        <section className="mt-6 grid gap-3 lg:grid-cols-2">
          {overview.alerts.map((alert) => (
            <div
              key={`${alert.kind}-${alert.title}`}
              className={`rounded-2xl border px-4 py-4 shadow-sm backdrop-blur ${alertTone(alert.kind)}`}
            >
              <div className="text-xs font-semibold uppercase tracking-[0.16em] opacity-80">{alert.kind}</div>
              <div className="mt-2 text-sm font-semibold">{alert.title}</div>
              <p className="mt-1 text-sm opacity-85">{alert.detail}</p>
            </div>
          ))}
        </section>
      ) : null}

      <section className="mt-6 grid gap-4 lg:grid-cols-3">
        <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
          <div className="text-xs font-medium text-neutral-600">Top customer</div>
          <div className="mt-2 text-lg font-semibold text-neutral-950">
            {overview.topCustomer?.customerName?.trim() ? overview.topCustomer.customerName : "No customer sales"}
          </div>
          <div className="mt-1 text-sm tabular-nums text-neutral-700">
            {overview.topCustomer ? formatMoney(overview.topCustomer.totalSpent) : "No data in selected period"}
          </div>
        </div>

        <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
          <div className="text-xs font-medium text-neutral-600">Top product by quantity</div>
          <div className="mt-2 text-lg font-semibold text-neutral-950">
            {overview.topProductByQuantity?.productName?.trim() ? overview.topProductByQuantity.productName : "No product data"}
          </div>
          <div className="mt-1 text-sm tabular-nums text-neutral-700">
            {overview.topProductByQuantity ? `${overview.topProductByQuantity.quantity} units sold` : "No data in selected period"}
          </div>
        </div>

        <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
          <div className="text-xs font-medium text-neutral-600">Stock risk</div>
          <div className="mt-2 text-lg font-semibold text-neutral-950">
            {overview.lowStockCount} low stock / {overview.outOfStockCount} out of stock
          </div>
          <div className="mt-1 text-sm text-neutral-700">Active products with tracked stock</div>
        </div>
      </section>

      <section className="mt-6 rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
        <div className="flex flex-col gap-2 lg:flex-row lg:items-center lg:justify-between">
          <div className="space-y-0.5">
            <h2 className="text-sm font-semibold text-neutral-900">Performance over time</h2>
            <p className="text-xs text-neutral-600">
              Current period in solid line, optional comparisons as overlays, forecast as dashed continuation.
            </p>
          </div>
          <div className="text-xs text-neutral-500">Range: {range.label}</div>
        </div>

        <PerformanceSeriesControls
          performanceMetric={performanceMetric}
          compareYears={compareYears}
          compareMonthsRaw={compareMonthsRaw}
          includeForecast={includeForecast}
          forecastPeriods={forecastPeriods}
          comparisonRangeEnabled={comparisonRangeEnabled}
          allowSpecificMonths={range.groupBy === "day"}
        />

        <div className="mt-4">
          <PerformanceSeriesChart series={performanceSeries} metric={performanceMetric} />
        </div>

        <div className="mt-3 flex flex-wrap gap-x-6 gap-y-2 text-xs text-neutral-500">
          <span>Metric: {performanceMetric === "marginPct" ? "Margin %" : performanceMetric === "avgTicket" ? "Average ticket" : performanceMetric === "gains" ? "Gains" : "Revenue"}</span>
          <span>Comparisons: {effectiveCompareYears.length + effectiveCompareMonths.length}</span>
          <span>
            Forecast: {includeForecast && performanceMetric === "revenue" ? `${performanceSeries.forecastSeries?.points.length ?? 0} visible point(s)` : "Revenue only"}
          </span>
        </div>
      </section>

      <section className="mt-6 grid gap-4 lg:grid-cols-2">
        <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <h2 className="text-sm font-semibold text-neutral-900">Sales by period</h2>
              <p className="text-xs text-neutral-600">Grouped by: {byPeriod.groupBy}</p>
            </div>
          </div>

          <div className="mt-4 max-h-80 space-y-2 overflow-y-auto pr-1">
            {points.length === 0 ? (
              <div className="rounded-2xl border border-black/10 bg-white/50 px-3 py-8 text-center text-sm text-neutral-600">
                No data yet.
              </div>
            ) : (
              points.map((p) => {
                const width = maxRevenue <= 0 ? 0 : Math.round((p.revenue / maxRevenue) * 100);
                return (
                  <div key={p.periodStart} className="grid grid-cols-12 items-center gap-3">
                    <div className="col-span-4 text-xs text-neutral-700">
                      {byPeriod.groupBy === "month" ? (
                        <span className="tabular-nums">{p.periodStart.slice(0, 7)}</span>
                      ) : (
                        <ClientDateTime iso={p.periodStart} variant="date" />
                      )}
                    </div>
                    <div className="col-span-6">
                      <div className="h-2 w-full rounded-full bg-black/5">
                        <div
                          className="h-2 rounded-full bg-neutral-900/80"
                          style={{ width: `${width}%` }}
                          aria-label={`Revenue ${p.revenue}`}
                        />
                      </div>
                    </div>
                    <div className="col-span-2 text-right text-xs tabular-nums text-neutral-700">
                      {formatMoney(p.revenue)}
                    </div>
                  </div>
                );
              })
              )}
          </div>
        </div>

        <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
          <div className="space-y-0.5">
            <h2 className="text-sm font-semibold text-neutral-900">Costs by period</h2>
            <p className="text-xs text-neutral-600">Grouped by: {costsGroupBy}</p>
          </div>

          <div className="mt-4 max-h-80 space-y-2 overflow-y-auto pr-1">
            {costsByPeriodPoints.length === 0 ? (
              <div className="rounded-2xl border border-black/10 bg-white/50 px-3 py-8 text-center text-sm text-neutral-600">
                No data yet.
              </div>
            ) : (
              costsByPeriodPoints.map((p) => {
                const width = maxCostAmount <= 0 ? 0 : Math.round((p.amount / maxCostAmount) * 100);
                const barWidth = width > 0 ? `max(${width}%, 10px)` : "0%";
                return (
                  <div key={p.periodStart} className="grid grid-cols-12 items-center gap-3">
                    <div className="col-span-4 text-xs text-neutral-700">
                      {costsGroupBy === "month" ? (
                        <span className="tabular-nums">{p.periodStart.slice(0, 7)}</span>
                      ) : (
                        <ClientDateTime iso={p.periodStart} variant="date" />
                      )}
                    </div>
                    <div className="col-span-6">
                      <div className="h-2 w-full rounded-full bg-black/5">
                        <div
                          className="h-2 rounded-full bg-neutral-900/80"
                          style={{ width: barWidth }}
                          aria-label={`Costs ${p.amount}`}
                        />
                      </div>
                    </div>
                    <div className="col-span-2 text-right text-xs tabular-nums text-neutral-700">
                      {formatMoney(p.amount)}
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </div>
      </section>

      <section className="mt-6">
        <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <h2 className="text-sm font-semibold text-neutral-900">Top products</h2>
              <p className="text-xs text-neutral-600">
                {topProductsSortBy === "quantity" ? "By quantity sold" : "By revenue"}
              </p>
            </div>
            <form method="get" className="flex gap-2">
              <DateFilterStateInputs mode={mode} year={year} month={month} day={day} tzOffsetMinutes={tzOffsetMinutes} />
              <PerformanceStateInputs
                performanceMetric={performanceMetric}
                compareYears={compareYears}
                compareMonthsRaw={compareMonthsRaw}
                includeForecast={includeForecast}
                forecastPeriods={forecastPeriods}
              />
              <select
                name="topProductsSortBy"
                defaultValue={topProductsSortBy}
                className="rounded-xl border border-black/10 bg-white/70 px-3 py-1.5 text-xs font-medium text-neutral-800 shadow-sm backdrop-blur transition hover:bg-white/90 focus:border-black/20 focus:outline-none focus:ring-2 focus:ring-black/5"
              >
                <option value="revenue">By Revenue</option>
                <option value="quantity">By Quantity</option>
              </select>
              <button
                type="submit"
                className="rounded-xl border border-black/10 bg-black px-3 py-1.5 text-xs font-semibold text-white shadow-sm transition hover:bg-black/90"
              >
                Apply
              </button>
            </form>
          </div>

          <div className="mt-4">
            <TopProductsBarChart data={topProducts} sortBy={topProductsSortBy} />
          </div>

          <div className="mt-4 overflow-x-auto rounded-2xl border border-black/10 bg-white/40">
            <table className="w-full border-collapse text-sm">
              <thead>
                <tr className="bg-white/40 text-left">
                  <th className="px-4 py-3 font-medium text-neutral-700">Product</th>
                  <th className="px-4 py-3 text-right font-medium text-neutral-700">Quantity</th>
                  <th className="px-4 py-3 text-right font-medium text-neutral-700">Revenue</th>
                </tr>
              </thead>
              <tbody>
                {topProducts.map((p) => (
                  <tr key={p.productId} className="border-t border-black/10">
                    <td className="px-4 py-3 font-medium text-neutral-900">{p.productName || p.productId}</td>
                    <td className="px-4 py-3 text-right tabular-nums text-neutral-900">{p.quantity}</td>
                    <td className="px-4 py-3 text-right tabular-nums text-neutral-900">{formatMoney(p.revenue)}</td>
                  </tr>
                ))}
                {topProducts.length === 0 ? (
                  <tr>
                    <td colSpan={3} className="px-4 py-12 text-center text-sm text-neutral-600">
                      No data yet.
                    </td>
                  </tr>
                ) : null}
              </tbody>
            </table>
          </div>
        </div>
      </section>

      <section className="mt-6 grid gap-4 lg:grid-cols-2">
        <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
          <div className="space-y-0.5">
            <h2 className="text-sm font-semibold text-neutral-900">Purchases by customer</h2>
            <p className="text-xs text-neutral-600">Number of transactions</p>
          </div>

          <div className="mt-4 space-y-2">
            {salesByCustomer.length === 0 ? (
              <div className="rounded-2xl border border-black/10 bg-white/50 px-3 py-8 text-center text-sm text-neutral-600">
                No data yet.
              </div>
            ) : (
              (() => {
                const maxSales = salesByCustomer.reduce((m, x) => Math.max(m, x.salesCount), 0);
                return salesByCustomer.map((x) => {
                  const width = maxSales <= 0 ? 0 : Math.round((x.salesCount / maxSales) * 100);
                  return (
                    <div key={x.customerId} className="grid grid-cols-12 items-center gap-3">
                      <div className="col-span-4 truncate text-xs text-neutral-700">{x.customerName || "—"}</div>
                      <div className="col-span-6">
                        <div className="h-2 w-full rounded-full bg-black/5">
                          <div
                            className="h-2 rounded-full bg-blue-500/80"
                            style={{ width: `${width}%` }}
                            aria-label={`Sales count ${x.salesCount}`}
                          />
                        </div>
                      </div>
                      <div className="col-span-2 text-right text-xs font-semibold">{x.salesCount}</div>
                    </div>
                  );
                });
              })()
            )}
          </div>
        </div>

        <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
          <div className="space-y-0.5">
            <h2 className="text-sm font-semibold text-neutral-900">Spending by customer</h2>
            <p className="text-xs text-neutral-600">Total revenue</p>
          </div>

          <div className="mt-4 space-y-2">
            {spendingByCustomer.length === 0 ? (
              <div className="rounded-2xl border border-black/10 bg-white/50 px-3 py-8 text-center text-sm text-neutral-600">
                No data yet.
              </div>
            ) : (
              (() => {
                const maxSpending = spendingByCustomer.reduce((m, x) => Math.max(m, x.totalSpent), 0);
                return spendingByCustomer.map((x) => {
                  const width = maxSpending <= 0 ? 0 : Math.round((x.totalSpent / maxSpending) * 100);
                  return (
                    <div key={x.customerId} className="grid grid-cols-12 items-center gap-3">
                      <div className="col-span-4 truncate text-xs text-neutral-700">{x.customerName || "—"}</div>
                      <div className="col-span-6">
                        <div className="h-2 w-full rounded-full bg-black/5">
                          <div
                            className="h-2 rounded-full bg-green-500/80"
                            style={{ width: `${width}%` }}
                            aria-label={`Total spent ${x.totalSpent}`}
                          />
                        </div>
                      </div>
                      <div className="col-span-2 text-right text-xs font-semibold tabular-nums">{formatMoney(x.totalSpent)}</div>
                    </div>
                  );
                });
              })()
            )}
          </div>
        </div>
      </section>
    </PageShell>
  );
}
