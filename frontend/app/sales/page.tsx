import Link from "next/link";

import { getSales } from "@/lib/sales.api";
import PageShell from "../ui/PageShell";
import SalesTable from "./ui/SalesTable";

export default async function SalesPage() {
  const sales = await getSales();

  return (
    <PageShell
      actions={
        <Link
          href="/sales/new"
          className="rounded-xl bg-black px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-black/90"
        >
          Add sale
        </Link>
      }
    >
      <SalesTable sales={sales} />
    </PageShell>
  );
}
