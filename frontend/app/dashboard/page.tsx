import Link from "next/link";

import { getDashboardSummary, getSalesByPeriod, getTopProducts } from "@/lib/dashboard.api";

function formatMoney(v: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(v);
}

function formatDate(iso: string) {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  return new Intl.DateTimeFormat("en-US", { dateStyle: "medium" }).format(d);
}

export default async function DashboardPage() {
  const [summary, byPeriod, topProducts] = await Promise.all([
    getDashboardSummary(),
    getSalesByPeriod({ groupBy: "day" }),
    getTopProducts({ limit: 10 }),
  ]);

  const points = byPeriod.points ?? [];
  const maxRevenue = points.reduce((m, p) => Math.max(m, p.revenue), 0);

  return (
    <main className="p-6 space-y-6">
      <header className="flex items-center justify-between">
        <div className="space-y-1">
          <h1 className="text-2xl font-semibold">Dashboard</h1>
          <p className="text-sm text-neutral-600">High-level metrics for your business.</p>
        </div>
        <div className="flex items-center gap-2">
          <Link
            href="/products"
            className="rounded-md border border-neutral-300 bg-white px-3 py-2 text-sm font-medium text-neutral-800 shadow-sm hover:bg-neutral-50"
          >
            Products
          </Link>
          <Link
            href="/sales"
            className="rounded-md border border-neutral-300 bg-white px-3 py-2 text-sm font-medium text-neutral-800 shadow-sm hover:bg-neutral-50"
          >
            Sales
          </Link>
        </div>
      </header>

      <section className="grid gap-4 md:grid-cols-3">
        <div className="rounded-xl border border-neutral-200 bg-white p-4">
          <div className="text-xs font-medium text-neutral-600">Revenue total</div>
          <div className="mt-2 text-2xl font-semibold tabular-nums">{formatMoney(summary.revenueTotal)}</div>
          <div className="mt-1 text-xs text-neutral-500">In selected period</div>
        </div>

        <div className="rounded-xl border border-neutral-200 bg-white p-4">
          <div className="text-xs font-medium text-neutral-600">Sales count</div>
          <div className="mt-2 text-2xl font-semibold tabular-nums">{summary.salesCount}</div>
          <div className="mt-1 text-xs text-neutral-500">Transactions</div>
        </div>

        <div className="rounded-xl border border-neutral-200 bg-white p-4">
          <div className="text-xs font-medium text-neutral-600">Avg ticket</div>
          <div className="mt-2 text-2xl font-semibold tabular-nums">{formatMoney(summary.avgTicket)}</div>
          <div className="mt-1 text-xs text-neutral-500">Revenue / sales</div>
        </div>
      </section>

      <section className="grid gap-4 lg:grid-cols-2">
        <div className="rounded-xl border border-neutral-200 bg-white p-4">
          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <h2 className="text-sm font-semibold text-neutral-900">Sales by period</h2>
              <p className="text-xs text-neutral-600">Grouped by: {byPeriod.groupBy}</p>
            </div>
          </div>

          <div className="mt-4 space-y-2">
            {points.length === 0 ? (
              <div className="rounded-lg border border-neutral-200 bg-neutral-50 px-3 py-8 text-center text-sm text-neutral-600">
                No data yet.
              </div>
            ) : (
              points.map((p) => {
                const width = maxRevenue <= 0 ? 0 : Math.round((p.revenue / maxRevenue) * 100);
                return (
                  <div key={p.periodStart} className="grid grid-cols-12 items-center gap-3">
                    <div className="col-span-4 text-xs text-neutral-700">{formatDate(p.periodStart)}</div>
                    <div className="col-span-6">
                      <div className="h-2 w-full rounded-full bg-neutral-100">
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

        <div className="rounded-xl border border-neutral-200 bg-white p-4">
          <div className="space-y-0.5">
            <h2 className="text-sm font-semibold text-neutral-900">Top products</h2>
            <p className="text-xs text-neutral-600">By revenue</p>
          </div>

          <div className="mt-4 overflow-x-auto rounded-lg border border-neutral-200">
            <table className="w-full border-collapse text-sm">
              <thead>
                <tr className="bg-neutral-50 text-left">
                  <th className="px-3 py-2 font-medium text-neutral-700">Product</th>
                  <th className="px-3 py-2 text-right font-medium text-neutral-700">Qty</th>
                  <th className="px-3 py-2 text-right font-medium text-neutral-700">Revenue</th>
                </tr>
              </thead>
              <tbody>
                {topProducts.map((p) => (
                  <tr key={p.productId} className="border-t border-neutral-200">
                    <td className="px-3 py-2">{p.productName || p.productId}</td>
                    <td className="px-3 py-2 text-right tabular-nums">{p.quantity}</td>
                    <td className="px-3 py-2 text-right tabular-nums">{formatMoney(p.revenue)}</td>
                  </tr>
                ))}
                {topProducts.length === 0 ? (
                  <tr>
                    <td colSpan={3} className="px-3 py-10 text-center text-sm text-neutral-600">
                      No data yet.
                    </td>
                  </tr>
                ) : null}
              </tbody>
            </table>
          </div>
        </div>
      </section>
    </main>
  );
}

