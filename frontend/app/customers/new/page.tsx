import Link from "next/link";
import NewCustomerForm from "./ui/NewCustomerForm";
import PageShell from "../../ui/PageShell";
import AppNav from "../../ui/AppNav";

export default function NewCustomerPage() {
  return (
    <PageShell>
      <div className="flex items-end justify-between gap-4">
        <h1 className="text-3xl font-semibold tracking-tight text-neutral-950">New customer</h1>
        <div className="flex items-center gap-2">
          <AppNav className="hidden md:flex" />
          <Link
            href="/customers"
            className="rounded-xl border border-black/10 bg-white/60 px-4 py-2.5 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
          >
            Back
          </Link>
        </div>
      </div>

      <div className="mt-6 rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
        <NewCustomerForm />
      </div>
    </PageShell>
  );
}
