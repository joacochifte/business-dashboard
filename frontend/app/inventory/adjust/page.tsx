import Link from "next/link";

import AdjustInventoryForm from "./ui/AdjustInventoryForm";

export default function AdjustInventoryPage() {
  return (
    <main className="p-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Adjust stock</h1>
        <Link href="/inventory" className="text-sm text-neutral-700 hover:underline">
          Back
        </Link>
      </div>

      <div className="mt-4 rounded-lg border border-neutral-200 bg-white p-4">
        <AdjustInventoryForm />
      </div>
    </main>
  );
}

