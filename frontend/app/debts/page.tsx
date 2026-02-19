import Link from "next/link";
import { getSalesByDebt } from "@/lib/sales.api";
import type { SaleDto } from "@/lib/sales.api";
import PageShell from "@/app/ui/PageShell";
import { Suspense } from "react";
import AppNav from "../ui/AppNav";

async function DebtsTable() {
  let debts: SaleDto[] = [];
  let error: string | null = null;

  try {
    debts = await getSalesByDebt(true);
  } catch (err) {
    error = err instanceof Error ? err.message : "Failed to load debts.";
  }

  if (error) {
    return (
      <div className="rounded-xl border border-rose-200 bg-rose-50/70 px-4 py-3 text-sm text-rose-900 shadow-sm backdrop-blur">
        {error}
      </div>
    );
  }

  if (!debts.length) {
    return (
      <div className="rounded-xl border border-blue-200 bg-blue-50/70 px-4 py-3 text-sm text-blue-900 shadow-sm backdrop-blur">
        No debts found.
      </div>
    );
  }

  return (
    <div className="overflow-x-auto rounded-2xl border border-black/10 bg-white/60 shadow-sm backdrop-blur">
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
          {debts.map((sale) => (
            <tr key={sale.id} className="hover:bg-white/40 transition">
              <td className="px-4 py-3 text-neutral-700">
                {sale.customerName || "—"}
              </td>
              <td className="px-4 py-3 text-neutral-700">
                {sale.items?.length ?? 0} item(s)
              </td>
              <td className="px-4 py-3 text-neutral-700">
                {sale.paymentMethod || "—"}
              </td>
              <td className="px-4 py-3 text-right font-semibold tabular-nums text-neutral-900">
                {sale.total.toFixed(2)}
              </td>
              <td className="px-4 py-3 text-neutral-700">
                {new Date(sale.createdAt).toLocaleDateString("es", { day: '2-digit', month: '2-digit', year: 'numeric' })}
              </td>
              <td className="px-4 py-3 text-center">
                <Link
                  href={`/sales/${sale.id}/edit`}
                  title="View / Edit"
                  className="text-xs font-semibold text-neutral-600 hover:text-neutral-900 transition"
                >
                  View
                </Link>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default function DebtsPage() {
  return (
    <PageShell>
      <header className="flex items-center justify-between">
        <div className="space-y-1">
          <h1 className="text-3xl font-semibold tracking-tight text-neutral-950">Debts</h1>
        </div>
        <div className="flex items-center gap-2">
          <AppNav className="hidden md:flex" />
        </div>
      </header>

      <div className="mt-6">
        <Suspense fallback={<div className="text-sm text-neutral-600">Loading debts...</div>}>
          <DebtsTable />
        </Suspense>
      </div>
    </PageShell>
  );
}
