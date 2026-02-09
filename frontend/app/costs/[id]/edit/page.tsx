import Link from "next/link";

import { getCostById, type CostDto } from "@/lib/costs.api";
import EditCostForm from "./ui/EditCostForm";
import PageShell from "../../../ui/PageShell";
import AppNav from "../../../ui/AppNav";

type Props = {
  params: Promise<{ id: string }>;
};

export default async function EditCostPage({ params }: Props) {
  const { id } = await params;

  let cost: CostDto | null = null;
  try {
    cost = await getCostById(id);
  } catch {
    cost = null;
  }

  return (
    <PageShell>
      <div className="flex items-end justify-between gap-4">
        <h1 className="text-3xl font-semibold tracking-tight text-neutral-950">Edit cost</h1>
        <div className="flex items-center gap-2">
          <AppNav className="hidden md:flex" />
          <Link
            href="/costs"
            className="rounded-xl border border-black/10 bg-white/60 px-4 py-2.5 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
          >
            Back
          </Link>
        </div>
      </div>

      <div className="mt-6 rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
        {cost ? (
          <EditCostForm cost={cost} />
        ) : (
          <p className="text-sm text-neutral-700">
            Cost not found: <span className="font-mono">{id}</span>
          </p>
        )}
      </div>
    </PageShell>
  );
}

