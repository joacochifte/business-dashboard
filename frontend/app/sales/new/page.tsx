import Link from "next/link";

import NewSaleForm from "./ui/NewSaleForm";
import PageShell from "../../ui/PageShell";

export default function NewSalePage() {
  return (
    <PageShell
      actions={
        <Link
          href="/sales"
          className="rounded-xl border border-black/10 bg-white/60 px-4 py-2.5 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
        >
          Back
        </Link>
      }
    >
      <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
        <NewSaleForm />
      </div>
    </PageShell>
  );
}
