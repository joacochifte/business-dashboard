import Link from "next/link";

import { getSaleById, type SaleDto } from "@/lib/sales.api";
import EditSaleForm from "./ui/EditSaleForm";
import PageShell from "../../../ui/PageShell";

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

  const backAction = (
    <Link
      href="/sales"
      className="rounded-xl border border-black/10 bg-white/60 px-4 py-2.5 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
    >
      Back
    </Link>
  );

  return (
    <PageShell actions={backAction}>
      <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
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
