import Link from "next/link";

import { getSales } from "@/lib/sales.api";
import PageShell from "../ui/PageShell";
import AppNav from "../ui/AppNav";
import SalesTable from "./ui/SalesTable";

export default async function SalesPage() {
  const sales = await getSales();

  return (
    <PageShell>
      <header className="flex items-center justify-between">
        <div className="space-y-1">
          <h1 className="text-3xl font-semibold tracking-tight text-neutral-950">Sales</h1>
        </div>
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

      <SalesTable sales={sales} />
    </PageShell>
  );
}
