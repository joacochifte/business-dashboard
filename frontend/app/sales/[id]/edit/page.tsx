import Link from "next/link";

import { getSaleById, type SaleDto } from "@/lib/sales.api";
import EditSaleForm from "./ui/EditSaleForm";

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
    <main className="p-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Edit sale</h1>
        <Link href="/sales" className="text-sm text-neutral-700 hover:underline">
          Back
        </Link>
      </div>

      <div className="mt-4 rounded-lg border border-neutral-200 bg-white p-4">
        {sale ? (
          <EditSaleForm sale={sale} />
        ) : (
          <p className="text-sm text-neutral-700">
            Sale not found: <span className="font-mono">{id}</span>
          </p>
        )}
      </div>
    </main>
  );
}

