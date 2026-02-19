import Link from "next/link";
import { getCustomers, type CustomerDto } from "@/lib/customers.api";
import CustomerRowActions from "./ui/CustomerRowActions";
import PageShell from "../ui/PageShell";
import AppNav from "../ui/AppNav";

function formatMoney(v: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(v);
}

function formatDate(iso: string | null | undefined) {
  if (!iso) return "-";
  return new Date(iso).toLocaleDateString();
}

export default async function CustomersPage() {
  const customers = await getCustomers();

  return (
    <PageShell>
      <header className="flex items-center justify-between">
        <div className="space-y-1">
          <h1 className="text-3xl font-semibold tracking-tight text-neutral-950">Customers</h1>
        </div>
        <div className="flex items-center gap-2">
          <AppNav className="hidden md:flex" />
          <Link
            href="/customers/new"
            className="rounded-xl bg-black px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-black/90"
          >
            Add customer
          </Link>
        </div>
      </header>

      <div className="mt-6 overflow-x-auto rounded-2xl border border-black/10 bg-white/60 shadow-sm backdrop-blur">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="bg-white/40 text-left">
              <th className="px-4 py-3 font-medium text-neutral-700">Name</th>
              <th className="px-4 py-3 font-medium text-neutral-700">Email</th>
              <th className="px-4 py-3 font-medium text-neutral-700">Phone</th>
              <th className="px-4 py-3 font-medium text-neutral-700">Birthday</th>
              <th className="px-4 py-3 text-right font-medium text-neutral-700">Purchases</th>
              <th className="px-4 py-3 text-right font-medium text-neutral-700">Lifetime value</th>
              <th className="px-4 py-3 font-medium text-neutral-700">Active</th>
              <th className="px-4 py-3 text-right font-medium text-neutral-700">Actions</th>
            </tr>
          </thead>
          <tbody>
            {customers.map((c: CustomerDto) => (
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
    </PageShell>
  );
}
