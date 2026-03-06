"use client";

import { useMemo, useState } from "react";
import Link from "next/link";

import type { SaleDto } from "@/lib/sales.api";
import DateFilterInput from "@/app/ui/DateFilterInput";

function formatMoney(v: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(v);
}

function toLocalDateInputValue(iso: string) {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return "";

  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

type DebtSort = "newest" | "oldest" | "total-desc" | "total-asc";

export default function DebtsTable({ debts }: { debts: SaleDto[] }) {
  const [search, setSearch] = useState("");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [sort, setSort] = useState<DebtSort>("newest");

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();

    const result = debts.filter((sale) => {
      const customer = sale.customerName?.toLowerCase() ?? "";
      const payment = sale.paymentMethod?.toLowerCase() ?? "";
      const localDate = toLocalDateInputValue(sale.createdAt);

      if (q && !customer.includes(q) && !payment.includes(q) && !localDate.includes(q)) {
        return false;
      }

      if (fromDate && localDate < fromDate) return false;
      if (toDate && localDate > toDate) return false;

      return true;
    });

    result.sort((a, b) => {
      switch (sort) {
        case "oldest":
          return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
        case "total-desc":
          return b.total - a.total;
        case "total-asc":
          return a.total - b.total;
        case "newest":
        default:
          return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
      }
    });

    return result;
  }, [debts, fromDate, search, sort, toDate]);

  const totalOwed = useMemo(() => filtered.reduce((sum, debt) => sum + debt.total, 0), [filtered]);

  return (
    <>
      <section className="mt-6 grid gap-4 xl:grid-cols-[1.4fr_1fr]">
        <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
          <div className="grid gap-3 md:grid-cols-12">
            <label className="grid gap-1 md:col-span-8">
              <span className="text-xs font-medium text-neutral-700">Search</span>
              <input
                type="text"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Customer, payment, or date..."
                className="rounded-xl border border-black/10 bg-white/70 px-4 py-2.5 text-sm text-neutral-900 shadow-sm outline-none placeholder:text-neutral-400 focus:border-black/20 focus:ring-2 focus:ring-black/5"
              />
            </label>

            <label className="grid gap-1 md:col-span-4">
              <span className="text-xs font-medium text-neutral-700">Sort</span>
              <select
                value={sort}
                onChange={(e) => setSort(e.target.value as DebtSort)}
                className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
              >
                <option value="newest">Newest first</option>
                <option value="oldest">Oldest first</option>
                <option value="total-desc">Total high to low</option>
                <option value="total-asc">Total low to high</option>
              </select>
            </label>

            <DateFilterInput label="From" value={fromDate} onChange={setFromDate} className="md:col-span-6" />

            <DateFilterInput label="To" value={toDate} onChange={setToDate} className="md:col-span-6" />
          </div>
        </div>

        <div className="rounded-2xl border border-amber-200/70 bg-gradient-to-br from-amber-50/90 to-white/70 p-5 shadow-sm backdrop-blur">
          <div className="text-xs font-semibold uppercase tracking-[0.18em] text-amber-800/80">Debt summary</div>
          <div className="mt-3 text-3xl font-semibold tracking-tight text-neutral-950">{formatMoney(totalOwed)}</div>
          <p className="mt-2 text-sm text-neutral-700">
            Total owed across the currently visible debts.
          </p>
          <div className="mt-4 text-xs text-neutral-600">
            {filtered.length} debt{filtered.length === 1 ? "" : "s"} listed
          </div>
        </div>
      </section>

      <div className="mt-4 overflow-x-auto rounded-2xl border border-black/10 bg-white/60 shadow-sm backdrop-blur">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-black/10 bg-white/40">
              <th className="px-4 py-3 text-left font-medium text-neutral-800">Customer</th>
              <th className="px-4 py-3 text-left font-medium text-neutral-800">Items</th>
              <th className="px-4 py-3 text-left font-medium text-neutral-800">Payment Method</th>
              <th className="px-4 py-3 text-right font-medium text-neutral-800">Total</th>
              <th className="px-4 py-3 text-left font-medium text-neutral-800">Date</th>
              <th className="px-4 py-3 text-center font-medium text-neutral-800">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-black/10">
            {filtered.map((sale) => (
              <tr key={sale.id} className="transition hover:bg-white/40">
                <td className="px-4 py-3 text-neutral-700">{sale.customerName?.trim() ? sale.customerName : "—"}</td>
                <td className="px-4 py-3 text-neutral-700">{sale.items?.length ?? 0} item(s)</td>
                <td className="px-4 py-3 text-neutral-700">{sale.paymentMethod?.trim() ? sale.paymentMethod : "—"}</td>
                <td className="px-4 py-3 text-right font-semibold tabular-nums text-neutral-900">{formatMoney(sale.total)}</td>
                <td className="px-4 py-3 text-neutral-700">
                  {new Date(sale.createdAt).toLocaleDateString("es", {
                    day: "2-digit",
                    month: "2-digit",
                    year: "numeric",
                  })}
                </td>
                <td className="px-4 py-3 text-center">
                  <Link
                    href={`/sales/${sale.id}/edit`}
                    title="View / Edit"
                    className="text-xs font-semibold text-neutral-600 transition hover:text-neutral-900"
                  >
                    View
                  </Link>
                </td>
              </tr>
            ))}
            {filtered.length === 0 && debts.length > 0 ? (
              <tr>
                <td colSpan={6} className="px-4 py-12 text-center text-sm text-neutral-600">
                  No debts match the current filters.
                </td>
              </tr>
            ) : null}
            {debts.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-4 py-12 text-center text-sm text-neutral-600">
                  No debts found.
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </div>
    </>
  );
}
