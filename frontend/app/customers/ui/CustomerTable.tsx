"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import type { CustomerDto } from "@/lib/customers.api";
import CustomerRowActions from "./CustomerRowActions";

function formatMoney(v: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(v);
}

function formatDate(iso: string | null | undefined) {
  if (!iso) return "-";
  // Parse date-only portion to avoid UTC timezone shift
  const [y, m, d] = iso.slice(0, 10).split("-").map(Number);
  const local = new Date(y, (m ?? 1) - 1, d ?? 1);
  if (Number.isNaN(local.getTime())) return iso;
  return local.toLocaleDateString("es", { day: "2-digit", month: "2-digit", year: "numeric" });
}

type SortField = "name" | "birthDate" | "totalPurchases" | "totalLifetimeValue";
type SortDir = "asc" | "desc";

export default function CustomerTable({ customers }: { customers: CustomerDto[] }) {
  const [search, setSearch] = useState("");
  const [sortField, setSortField] = useState<SortField>("name");
  const [sortDir, setSortDir] = useState<SortDir>("asc");

  function toggleSort(field: SortField) {
    if (sortField === field) {
      setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    } else {
      setSortField(field);
      setSortDir("asc");
    }
  }

  const filtered = useMemo(() => {
    const q = search.toLowerCase().trim();
    if (!q) return customers;
    return customers.filter(
      (c) =>
        c.name.toLowerCase().includes(q) ||
        (c.email ?? "").toLowerCase().includes(q),
    );
  }, [customers, search]);

  const sorted = useMemo(() => {
    const list = [...filtered];
    const dir = sortDir === "asc" ? 1 : -1;

    list.sort((a, b) => {
      switch (sortField) {
        case "name":
          return dir * a.name.localeCompare(b.name);
        case "birthDate": {
          const da = a.birthDate ?? "";
          const db = b.birthDate ?? "";
          if (!da && !db) return 0;
          if (!da) return 1;
          if (!db) return -1;
          return dir * da.localeCompare(db);
        }
        case "totalPurchases":
          return dir * (a.totalPurchases - b.totalPurchases);
        case "totalLifetimeValue":
          return dir * (a.totalLifetimeValue - b.totalLifetimeValue);
        default:
          return 0;
      }
    });

    return list;
  }, [filtered, sortField, sortDir]);

  const arrow = (field: SortField) => {
    if (sortField !== field) return <span className="ml-1 text-neutral-300">↕</span>;
    return <span className="ml-1">{sortDir === "asc" ? "↑" : "↓"}</span>;
  };

  const thBtn = "flex items-center gap-0.5 hover:text-neutral-950 transition cursor-pointer select-none";

  return (
    <>
      <div className="mt-6">
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search by name or email…"
          className="w-full rounded-xl border border-black/10 bg-white/70 px-4 py-2.5 text-sm text-neutral-900 shadow-sm outline-none backdrop-blur placeholder:text-neutral-400 focus:border-black/20 focus:ring-2 focus:ring-black/5"
        />
      </div>

      <div className="mt-4 overflow-x-auto rounded-2xl border border-black/10 bg-white/60 shadow-sm backdrop-blur">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="bg-white/40 text-left">
              <th className="px-4 py-3 font-medium text-neutral-700">
                <button type="button" className={thBtn} onClick={() => toggleSort("name")}>
                  Name{arrow("name")}
                </button>
              </th>
              <th className="px-4 py-3 font-medium text-neutral-700">Email</th>
              <th className="px-4 py-3 font-medium text-neutral-700">Phone</th>
              <th className="px-4 py-3 font-medium text-neutral-700">
                <button type="button" className={thBtn} onClick={() => toggleSort("birthDate")}>
                  Birthday{arrow("birthDate")}
                </button>
              </th>
              <th className="px-4 py-3 text-right font-medium text-neutral-700">
                <button type="button" className={`${thBtn} ml-auto`} onClick={() => toggleSort("totalPurchases")}>
                  Purchases{arrow("totalPurchases")}
                </button>
              </th>
              <th className="px-4 py-3 text-right font-medium text-neutral-700">
                <button type="button" className={`${thBtn} ml-auto`} onClick={() => toggleSort("totalLifetimeValue")}>
                  Lifetime value{arrow("totalLifetimeValue")}
                </button>
              </th>
              <th className="px-4 py-3 font-medium text-neutral-700">Active</th>
              <th className="px-4 py-3 text-right font-medium text-neutral-700">Actions</th>
            </tr>
          </thead>
          <tbody>
            {sorted.map((c) => (
              <tr key={c.id} className="border-t border-black/10">
                <td className="px-4 py-3 font-medium text-neutral-900">{c.name}</td>
                <td className="px-4 py-3 text-neutral-700">{c.email ?? "-"}</td>
                <td className="px-4 py-3 text-neutral-700">{c.phone ?? "-"}</td>
                <td className="px-4 py-3 text-neutral-700">{formatDate(c.birthDate)}</td>
                <td className="px-4 py-3 text-right tabular-nums text-neutral-900">{c.totalPurchases}</td>
                <td className="px-4 py-3 text-right tabular-nums text-neutral-900">{formatMoney(c.totalLifetimeValue)}</td>
                <td className="px-4 py-3">
                  <span
                    className={
                      c.isActive
                        ? "inline-flex rounded-full border border-emerald-200 bg-emerald-50 px-2 py-0.5 text-xs font-medium text-emerald-700"
                        : "inline-flex rounded-full border border-black/10 bg-white/60 px-2 py-0.5 text-xs font-medium text-neutral-700"
                    }
                  >
                    {c.isActive ? "Yes" : "No"}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <CustomerRowActions customerId={c.id} />
                </td>
              </tr>
            ))}
            {sorted.length === 0 && customers.length > 0 ? (
              <tr>
                <td colSpan={8} className="px-4 py-12 text-center text-sm text-neutral-600">
                  No customers match your search.
                </td>
              </tr>
            ) : null}
            {customers.length === 0 ? (
              <tr>
                <td colSpan={8} className="px-4 py-12 text-center text-sm text-neutral-600">
                  No customers yet.{" "}
                  <Link href="/customers/new" className="font-medium underline">
                    Add the first one
                  </Link>
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </div>
    </>
  );
}
