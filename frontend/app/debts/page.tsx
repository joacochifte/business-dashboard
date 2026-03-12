import { getSalesByDebt, type SaleDto } from "@/lib/sales.api";
import PageShell from "@/app/ui/PageShell";
import DebtsTable from "./ui/DebtsTable";

export default async function DebtsPage() {
  let debts: SaleDto[] = [];
  let error: string | null = null;

  try {
    debts = await getSalesByDebt(true);
  } catch (err) {
    error = err instanceof Error ? err.message : "Failed to load debts.";
  }

  return (
    <PageShell>
      {error ? (
        <div className="mt-6 rounded-xl border border-rose-200 bg-rose-50/70 px-4 py-3 text-sm text-rose-900 shadow-sm backdrop-blur">
          {error}
        </div>
      ) : null}

      <DebtsTable debts={debts} />
    </PageShell>
  );
}
