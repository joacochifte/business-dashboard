import Link from "next/link";

import { getCosts, type CostSummaryDto } from "@/lib/costs.api";
import PageShell from "../ui/PageShell";
import AppNav from "../ui/AppNav";
import ClientDateTime from "../ui/ClientDateTime";
import CostRowActions from "./ui/CostRowActions";

function formatMoney(v: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(v);
}

export default async function CostsPage() {
  const costs = await getCosts();

  return (
    <PageShell>
      <header className="flex items-end justify-between gap-4">
        <h1 className="text-3xl font-semibold tracking-tight text-neutral-950">Costs</h1>
        <div className="flex items-center gap-2">
          <AppNav className="hidden md:flex" />
          <Link
            href="/costs/new"
            className="rounded-xl bg-black px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-black/90"
          >
            Add cost
          </Link>
        </div>
      </header>

      <div className="mt-6 overflow-x-auto rounded-2xl border border-black/10 bg-white/60 shadow-sm backdrop-blur">
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
            {costs.map((c: CostSummaryDto) => (
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
    </PageShell>
  );
}
