import Link from "next/link";
import { getCustomers } from "@/lib/customers.api";
import CustomerTable from "./ui/CustomerTable";
import PageShell from "../ui/PageShell";

export default async function CustomersPage() {
  const customers = await getCustomers();

  return (
    <PageShell
      actions={
        <Link
          href="/customers/new"
          className="rounded-xl bg-black px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-black/90"
        >
          Add customer
        </Link>
      }
    >
      <CustomerTable customers={customers} />
    </PageShell>
  );
}
