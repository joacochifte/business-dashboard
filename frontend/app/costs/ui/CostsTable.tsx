"use client";

import { useMemo, useState } from "react";

import type { CostSummaryDto } from "@/lib/costs.api";
import ClientDateTime from "@/app/ui/ClientDateTime";
import DateFilterInput from "@/app/ui/DateFilterInput";
import CostRowActions from "./CostRowActions";

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

type SortMode = "newest" | "oldest" | "amount-desc" | "amount-asc";

export default function CostsTable({ costs }: { costs: CostSummaryDto[] }) {
  const [search, setSearch] = useState("");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [sort, setSort] = useState<SortMode>("newest");

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();

    const result = costs.filter((cost) => {
      const localDate = toLocalDateInputValue(cost.dateIncurred);

      if (q && !cost.name.toLowerCase().includes(q) && !localDate.includes(q)) {
        return false;
      }

      if (fromDate && localDate < fromDate) return false;
      if (toDate && localDate > toDate) return false;

      return true;
    });

    result.sort((a, b) => {
      switch (sort) {
        case "oldest":
          return new Date(a.dateIncurred).getTime() - new Date(b.dateIncurred).getTime();
        case "amount-desc":
          return b.amount - a.amount;
        case "amount-asc":
          return a.amount - b.amount;
        case "newest":
        default:
          return new Date(b.dateIncurred).getTime() - new Date(a.dateIncurred).getTime();
      }
    });

    return result;
  }, [costs, fromDate, search, sort, toDate]);

  return (
    <>
      <section className="mt-6 rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
        <div className="grid gap-3 md:grid-cols-12">
          <label className="grid gap-1 md:col-span-8">
            <span className="text-xs font-medium text-neutral-700">Search</span>
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search by name or date..."
              className="rounded-xl border border-black/10 bg-white/70 px-4 py-2.5 text-sm text-neutral-900 shadow-sm outline-none placeholder:text-neutral-400 focus:border-black/20 focus:ring-2 focus:ring-black/5"
            />
          </label>

          <label className="grid gap-1 md:col-span-4">
            <span className="text-xs font-medium text-neutral-700">Sort</span>
            <select
              value={sort}
              onChange={(e) => setSort(e.target.value as SortMode)}
              className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
            >
              <option value="newest">Newest first</option>
              <option value="oldest">Oldest first</option>
              <option value="amount-desc">Amount high to low</option>
              <option value="amount-asc">Amount low to high</option>
            </select>
          </label>

          <DateFilterInput label="From" value={fromDate} onChange={setFromDate} className="md:col-span-6" />

          <DateFilterInput label="To" value={toDate} onChange={setToDate} className="md:col-span-6" />
        </div>
      </section>

      <div className="mt-4 overflow-x-auto rounded-2xl border border-black/10 bg-white/60 shadow-sm backdrop-blur">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="bg-white/40 text-left">
              <th className="px-4 py-3 font-medium text-neutral-700">Date</th>
              <th className="px-4 py-3 font-medium text-neutral-700">Name</th>
              <th className="px-4 py-3 text-right font-medium text-neutral-700">Amount</th>
              <th className="px-4 py-3 text-right font-medium text-neutral-700">Actions</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((c) => (
              <tr key={c.id} className="border-t border-black/10">
                <td className="px-4 py-3 whitespace-nowrap text-neutral-900">
                  <ClientDateTime iso={c.dateIncurred} />
                </td>
                <td className="px-4 py-3 text-neutral-900">{c.name}</td>
                <td className="px-4 py-3 text-right tabular-nums text-neutral-900">{formatMoney(c.amount)}</td>
                <td className="px-4 py-3">
                  <CostRowActions costId={c.id} />
                </td>
              </tr>
            ))}
            {filtered.length === 0 && costs.length > 0 ? (
              <tr>
                <td colSpan={4} className="px-4 py-12 text-center text-sm text-neutral-600">
                  No costs match the current filters.
                </td>
              </tr>
            ) : null}
            {costs.length === 0 ? (
              <tr>
                <td colSpan={4} className="px-4 py-12 text-center text-sm text-neutral-600">
                  No costs yet.
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </div>
    </>
  );
}
