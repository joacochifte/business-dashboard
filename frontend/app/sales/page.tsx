import Link from "next/link";

import { getSales, type SaleDto } from "@/lib/sales.api";
import SaleRowActions from "./ui/SaleRowActions";
import PageShell from "../ui/PageShell";
import AppNav from "../ui/AppNav";
import ClientDateTime from "../ui/ClientDateTime";

function formatMoney(v: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(v);
}

export default async function SalesPage() {
  const sales = await getSales();

  return (
    <PageShell>
      <header className="flex items-end justify-between gap-4">
        <h1 className="text-3xl font-semibold tracking-tight text-neutral-950">Sales</h1>
        <div className="flex items-center gap-2">
          <AppNav className="hidden md:flex" />
          <Link
            href="/sales/new"
            className="rounded-xl bg-black px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-black/90"
          >
            Add sale
          </Link>
        </div>
      </header>

      <div className="mt-6 overflow-x-auto rounded-2xl border border-black/10 bg-white/60 shadow-sm backdrop-blur">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="bg-white/40 text-left">
              <th className="px-4 py-3 font-medium text-neutral-700">Date (local)</th>
              <th className="px-4 py-3 font-medium text-neutral-700">Customer</th>
              <th className="px-4 py-3 font-medium text-neutral-700">Payment</th>
              <th className="px-4 py-3 text-right font-medium text-neutral-700">Items</th>
              <th className="px-4 py-3 text-right font-medium text-neutral-700">Total</th>
              <th className="px-4 py-3 text-right font-medium text-neutral-700">Actions</th>
            </tr>
          </thead>
          <tbody>
            {sales.map((s: SaleDto) => (
              <tr key={s.id} className="border-t border-black/10">
                <td className="px-4 py-3 whitespace-nowrap text-neutral-900">
                  <ClientDateTime iso={s.createdAt} />
                </td>
                <td className="px-4 py-3 text-neutral-900">{s.customerName?.trim() ? s.customerName : "-"}</td>
                <td className="px-4 py-3 text-neutral-900">{s.paymentMethod?.trim() ? s.paymentMethod : "-"}</td>
                <td className="px-4 py-3 text-right tabular-nums text-neutral-900">{s.items.length}</td>
                <td className="px-4 py-3 text-right tabular-nums text-neutral-900">{formatMoney(s.total)}</td>
                <td className="px-4 py-3">
                  <SaleRowActions saleId={s.id} />
                </td>
              </tr>
            ))}
            {sales.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-4 py-12 text-center text-sm text-neutral-600">
                  No sales yet.
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </div>
    </PageShell>
  );
}
