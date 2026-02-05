import Link from "next/link";

import { getSales, type SaleDto } from "@/lib/sales.api";
import SaleRowActions from "./ui/SaleRowActions";

function formatMoney(v: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(v);
}

function formatDateTime(iso: string) {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  return new Intl.DateTimeFormat("en-US", { dateStyle: "medium", timeStyle: "short" }).format(d);
}

export default async function SalesPage() {
  const sales = await getSales();

  return (
    <main className="p-6">
      <header className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Sales</h1>
        <Link
          href="/sales/new"
          className="rounded-md bg-black px-3 py-2 text-sm font-medium text-white hover:bg-black/90"
        >
          Add sale
        </Link>
      </header>

      <div className="mt-4 overflow-x-auto rounded-lg border border-neutral-200">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="bg-neutral-50 text-left">
              <th className="px-3 py-2 font-medium text-neutral-700">Date</th>
              <th className="px-3 py-2 font-medium text-neutral-700">Customer</th>
              <th className="px-3 py-2 font-medium text-neutral-700">Payment</th>
              <th className="px-3 py-2 text-right font-medium text-neutral-700">Items</th>
              <th className="px-3 py-2 text-right font-medium text-neutral-700">Total</th>
              <th className="px-3 py-2 text-right font-medium text-neutral-700">Actions</th>
            </tr>
          </thead>
          <tbody>
            {sales.map((s: SaleDto) => (
              <tr key={s.id} className="border-t border-neutral-200">
                <td className="px-3 py-2 whitespace-nowrap">{formatDateTime(s.createdAt)}</td>
                <td className="px-3 py-2">{s.customerName?.trim() ? s.customerName : "-"}</td>
                <td className="px-3 py-2">{s.paymentMethod?.trim() ? s.paymentMethod : "-"}</td>
                <td className="px-3 py-2 text-right tabular-nums">{s.items.length}</td>
                <td className="px-3 py-2 text-right tabular-nums">{formatMoney(s.total)}</td>
                <td className="px-3 py-2">
                  <SaleRowActions saleId={s.id} />
                </td>
              </tr>
            ))}
            {sales.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-3 py-10 text-center text-sm text-neutral-600">
                  No sales yet.
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </div>
    </main>
  );
}

