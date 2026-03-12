import Link from "next/link";
import { getCustomerById, type CustomerDto } from "@/lib/customers.api";
import EditCustomerForm from "./ui/EditCustomerForm";
import PageShell from "../../../ui/PageShell";

type Props = {
  params: Promise<{ id: string }>;
};

export default async function EditCustomerPage({ params }: Props) {
  const { id } = await params;

  let customer: CustomerDto | null = null;
  try {
    customer = await getCustomerById(id);
  } catch {
    customer = null;
  }

  return (
    <PageShell
      actions={
        <Link
          href="/customers"
          className="rounded-xl border border-black/10 bg-white/60 px-4 py-2.5 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
        >
          Back
        </Link>
      }
    >
      <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
        {customer ? (
          <EditCustomerForm customer={customer} />
        ) : (
          <p className="text-sm text-neutral-700">
            Customer not found: <span className="font-mono">{id}</span>
          </p>
        )}
      </div>
    </PageShell>
  );
}
