"use client";

import { useMemo, useState } from "react";

import type { SaleDto } from "@/lib/sales.api";
import ClientDateTime from "@/app/ui/ClientDateTime";
import DateFilterInput from "@/app/ui/DateFilterInput";
import SaleRowActions from "./SaleRowActions";

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

function previewNotes(notes?: string | null, max = 40) {
  if (!notes || !notes.trim()) return "-";
  const trimmed = notes.trim();
  return trimmed.length > max ? `${trimmed.slice(0, max)}…` : trimmed;
}

type TotalSort = "newest" | "oldest" | "total-desc" | "total-asc";

export default function SalesTable({ sales }: { sales: SaleDto[] }) {
  const [search, setSearch] = useState("");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [sort, setSort] = useState<TotalSort>("newest");

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();

    const result = sales.filter((sale) => {
      const customer = sale.customerName?.toLowerCase() ?? "";
      const payment = sale.paymentMethod?.toLowerCase() ?? "";
      const note = sale.notes?.toLowerCase() ?? "";
      const localDate = toLocalDateInputValue(sale.createdAt);

      if (q && !customer.includes(q) && !payment.includes(q) && !localDate.includes(q) && !note.includes(q)) {
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
  }, [fromDate, sales, search, sort, toDate]);

  return (
    <>
      <section className="mt-6 rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
        <div className="grid gap-3 md:grid-cols-12">
          <label className="grid gap-1 md:col-span-5">
            <span className="text-xs font-medium text-neutral-700">Search</span>
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Customer, payment, or date..."
              className="rounded-xl border border-black/10 bg-white/70 px-4 py-2.5 text-sm text-neutral-900 shadow-sm outline-none placeholder:text-neutral-400 focus:border-black/20 focus:ring-2 focus:ring-black/5"
            />
          </label>

          <DateFilterInput label="From" value={fromDate} onChange={setFromDate} className="md:col-span-2" />

          <DateFilterInput label="To" value={toDate} onChange={setToDate} className="md:col-span-2" />

          <label className="grid gap-1 md:col-span-3">
            <span className="text-xs font-medium text-neutral-700">Sort</span>
            <select
              value={sort}
              onChange={(e) => setSort(e.target.value as TotalSort)}
              className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
            >
              <option value="newest">Newest first</option>
              <option value="oldest">Oldest first</option>
              <option value="total-desc">Total high to low</option>
              <option value="total-asc">Total low to high</option>
            </select>
          </label>
        </div>
      </section>

      <div className="mt-4 overflow-x-auto rounded-2xl border border-black/10 bg-white/60 shadow-sm backdrop-blur">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="bg-white/40 text-left">
              <th className="px-4 py-3 font-medium text-neutral-700">Date</th>
              <th className="px-4 py-3 font-medium text-neutral-700">Customer</th>
              <th className="px-4 py-3 font-medium text-neutral-700">Payment</th>
              <th className="px-4 py-3 text-right font-medium text-neutral-700">Items</th>
              <th className="px-4 py-3 text-right font-medium text-neutral-700">Total</th>
              <th className="px-4 py-3 font-medium text-neutral-700">Notes</th>
              <th className="px-4 py-3 text-right font-medium text-neutral-700">Actions</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((s) => (
              <tr key={s.id} className="border-t border-black/10">
                <td className="px-4 py-3 whitespace-nowrap text-neutral-900">
                  <ClientDateTime iso={s.createdAt} />
                </td>
                <td className="px-4 py-3 text-neutral-900">{s.customerName?.trim() ? s.customerName : "-"}</td>
                <td className="px-4 py-3 text-neutral-900">{s.paymentMethod?.trim() ? s.paymentMethod : "-"}</td>
                <td className="px-4 py-3 text-right tabular-nums text-neutral-900">{s.items.length}</td>
                <td className="px-4 py-3 text-right tabular-nums text-neutral-900">{formatMoney(s.total)}</td>
                <td className="px-4 py-3 text-neutral-900">{previewNotes(s.notes)}</td>
                <td className="px-4 py-3">
                  <SaleRowActions saleId={s.id} />
                </td>
              </tr>
            ))}
            {filtered.length === 0 && sales.length > 0 ? (
              <tr>
                <td colSpan={7} className="px-4 py-12 text-center text-sm text-neutral-600">
                  No sales match the current filters.
                </td>
              </tr>
            ) : null}
            {sales.length === 0 ? (
              <tr>
                <td colSpan={7} className="px-4 py-12 text-center text-sm text-neutral-600">
                  No sales yet.
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </div>
    </>
  );
}
