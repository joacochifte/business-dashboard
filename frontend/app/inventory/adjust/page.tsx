import Link from "next/link";

import AdjustInventoryForm from "./ui/AdjustInventoryForm";
import PageShell from "../../ui/PageShell";

export default function AdjustInventoryPage() {
  return (
    <PageShell
      actions={
        <Link
          href="/inventory"
          className="rounded-xl border border-black/10 bg-white/60 px-4 py-2.5 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
        >
          Back
        </Link>
      }
    >
      <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
        <div className="mb-4 text-sm text-neutral-600">Manual inventory adjustments.</div>
        <AdjustInventoryForm />
      </div>
    </PageShell>
  );
}
