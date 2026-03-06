import Link from "next/link";

import { getCosts } from "@/lib/costs.api";
import PageShell from "../ui/PageShell";
import AppNav from "../ui/AppNav";
import CostsTable from "./ui/CostsTable";

export default async function CostsPage() {
  const costs = await getCosts();

  return (
    <PageShell>
      <header className="flex items-center justify-between">
        <div className="space-y-1">
          <h1 className="text-3xl font-semibold tracking-tight text-neutral-950">Costs</h1>
        </div>
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

      <CostsTable costs={costs} />
    </PageShell>
  );
}
