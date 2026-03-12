import Link from "next/link";

import { getCosts } from "@/lib/costs.api";
import PageShell from "../ui/PageShell";
import CostsTable from "./ui/CostsTable";

export default async function CostsPage() {
  const costs = await getCosts();

  return (
    <PageShell
      actions={
        <Link
          href="/costs/new"
          className="rounded-xl bg-black px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-black/90"
        >
          Add cost
        </Link>
      }
    >
      <CostsTable costs={costs} />
    </PageShell>
  );
}
