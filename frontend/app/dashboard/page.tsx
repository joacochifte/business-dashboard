import { getDashboardSummary, getSalesByPeriod, getTopProducts } from "@/lib/dashboard.api";
import TopProductsBarChart from "./ui/TopProductsBarChart";
import PageShell from "../ui/PageShell";
import AppNav from "../ui/AppNav";
import ClientDateTime from "../ui/ClientDateTime";
import TzOffsetField from "./ui/TzOffsetField";

function formatMoney(v: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(v);
}

type ViewMode = "daily" | "month" | "year" | "all";

type Props = {
  searchParams?: Promise<Record<string, string | string[] | undefined>>;
};

function pickFirst(v: string | string[] | undefined): string | undefined {
  if (Array.isArray(v)) return v[0];
  return v;
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
  // In local dev, the Next.js server runs on your machine, so local time matches your PC.
  // In a hosted environment, "local" would be the server locale; tzOffset param can override on form submit.
  const currentYear = now.getFullYear();
  const currentMonth = now.getMonth() + 1;
  const currentDay = now.getDate();

  const mode = (pickFirst(sp.view) as ViewMode | undefined) ?? "month";
  const tzOffsetRaw = pickFirst(sp.tzOffset);
  const tzOffsetMinutes =
    tzOffsetRaw === undefined ? clampTzOffsetMinutes(now.getTimezoneOffset()) : clampTzOffsetMinutes(Number(tzOffsetRaw) || 0);

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

  const [summary, byPeriod, topProducts] = await Promise.all([
    getDashboardSummary({ from: range.from, to: range.to }),
    getSalesByPeriod({ groupBy: range.groupBy, from: range.from, to: range.to }),
    getTopProducts({ limit: 10, from: range.from, to: range.to }),
  ]);

  const points = byPeriod.points ?? [];
  const maxRevenue = points.reduce((m, p) => Math.max(m, p.revenue), 0);

  return (
    <PageShell>
      <header className="flex items-center justify-between">
        <div className="space-y-1">
          <h1 className="text-3xl font-semibold tracking-tight text-neutral-950">Dashboard</h1>
          <p className="text-sm text-neutral-600">
            High-level metrics for your business.{" "}
            <span className="font-medium text-neutral-800">
              {range.label} (local)
            </span>
          </p>
        </div>
        <AppNav className="hidden md:flex" />
      </header>

      <section className="mt-6 rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
        <form className="grid gap-3 md:grid-cols-12" method="GET">
          <TzOffsetField />
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

      <section className="mt-6 grid gap-4 md:grid-cols-3">
        <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
          <div className="text-xs font-medium text-neutral-600">Revenue total</div>
          <div className="mt-2 text-2xl font-semibold tabular-nums">{formatMoney(summary.revenueTotal)}</div>
          <div className="mt-1 text-xs text-neutral-500">In selected period</div>
        </div>

        <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
          <div className="text-xs font-medium text-neutral-600">Sales count</div>
          <div className="mt-2 text-2xl font-semibold tabular-nums">{summary.salesCount}</div>
          <div className="mt-1 text-xs text-neutral-500">Transactions</div>
        </div>

        <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
          <div className="text-xs font-medium text-neutral-600">Average ticket</div>
          <div className="mt-2 text-2xl font-semibold tabular-nums">{formatMoney(summary.avgTicket)}</div>
          <div className="mt-1 text-xs text-neutral-500">Revenue / sales</div>
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

          <div className="mt-4 space-y-2">
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
            <h2 className="text-sm font-semibold text-neutral-900">Top products</h2>
            <p className="text-xs text-neutral-600">By revenue</p>
          </div>

          <div className="mt-4">
            <TopProductsBarChart data={topProducts} />
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
    </PageShell>
  );
}
