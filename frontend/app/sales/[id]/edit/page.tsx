import Link from "next/link";

import { getSaleById, type SaleDto } from "@/lib/sales.api";
import EditSaleForm from "./ui/EditSaleForm";
import PageShell from "../../../ui/PageShell";
import AppNav from "../../../ui/AppNav";

type Props = {
  params: Promise<{ id: string }>;
};

export default async function EditSalePage({ params }: Props) {
  const { id } = await params;

  let sale: SaleDto | null = null;
  try {
    sale = await getSaleById(id);
  } catch {
    sale = null;
  }

  return (
    <PageShell>
      <div className="flex items-end justify-between gap-4">
        <h1 className="text-3xl font-semibold tracking-tight text-neutral-950">Edit sale</h1>
        <div className="flex items-center gap-2">
          <AppNav className="hidden md:flex" />
          <Link
            href="/sales"
            className="rounded-xl border border-black/10 bg-white/60 px-4 py-2.5 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
          >
            Back
          </Link>
        </div>
      </div>

      <div className="mt-6 rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
        {sale ? (
          <EditSaleForm sale={sale} />
        ) : (
          <p className="text-sm text-neutral-700">
            Sale not found: <span className="font-mono">{id}</span>
          </p>
        )}
      </div>
    </PageShell>
  );
}
