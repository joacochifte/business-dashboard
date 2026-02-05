import Link from "next/link";

import NewSaleForm from "./ui/NewSaleForm";

export default function NewSalePage() {
  return (
    <main className="p-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">New sale</h1>
        <Link href="/sales" className="text-sm text-neutral-700 hover:underline">
          Back
        </Link>
      </div>

      <div className="mt-4 rounded-lg border border-neutral-200 bg-white p-4">
        <NewSaleForm />
      </div>
    </main>
  );
}

