import Link from "next/link";
import { getCustomers } from "@/lib/customers.api";
import CustomerTable from "./ui/CustomerTable";
import PageShell from "../ui/PageShell";
import AppNav from "../ui/AppNav";

export default async function CustomersPage() {
  const customers = await getCustomers();

  return (
    <PageShell>
      <header className="flex items-center justify-between">
        <div className="space-y-1">
          <h1 className="text-3xl font-semibold tracking-tight text-neutral-950">Customers</h1>
        </div>
        <div className="flex items-center gap-2">
          <AppNav className="hidden md:flex" />
          <Link
            href="/customers/new"
            className="rounded-xl bg-black px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-black/90"
          >
            Add customer
          </Link>
        </div>
      </header>

      <CustomerTable customers={customers} />
    </PageShell>
  );
}
