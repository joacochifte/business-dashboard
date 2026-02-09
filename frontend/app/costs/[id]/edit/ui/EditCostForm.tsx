"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useMemo, useState } from "react";

import { updateCost, type CostCreationDto, type CostDto } from "@/lib/costs.api";

type Props = {
  cost: CostDto;
};

type FormState = {
  name: string;
  amount: string;
  dateIncurred: string;
  description: string;
};

function toNumber(v: string) {
  const n = Number(v);
  return Number.isFinite(n) ? n : NaN;
}

function toIsoFromDateInput(dateIncurred: string) {
  return `${dateIncurred}T00:00:00Z`;
}

function toDateInputValue(iso: string) {
  return iso.slice(0, 10);
}

export default function EditCostForm({ cost }: Props) {
  const router = useRouter();
  const [form, setForm] = useState<FormState>({
    name: cost.name ?? "",
    amount: String(cost.amount ?? ""),
    dateIncurred: toDateInputValue(cost.dateIncurred),
    description: cost.description ?? "",
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const clientValidationError = useMemo(() => {
    if (!form.name.trim()) return "Name is required.";
    const amount = toNumber(form.amount);
    if (!Number.isFinite(amount) || amount <= 0) return "Amount must be greater than 0.";
    if (!form.dateIncurred) return "Date incurred is required.";
    return null;
  }, [form]);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);

    if (clientValidationError) {
      setError(clientValidationError);
      return;
    }

    const payload: CostCreationDto = {
      name: form.name.trim(),
      amount: Number(form.amount),
      dateIncurred: toIsoFromDateInput(form.dateIncurred),
      description: form.description.trim() ? form.description.trim() : null,
    };

    setSubmitting(true);
    try {
      await updateCost(cost.id, payload);
      router.push("/costs");
      router.refresh();
    } catch (err) {
      const msg = err instanceof Error ? err.message : "Unknown error";
      setError(msg);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form onSubmit={onSubmit} className="space-y-4">
      {error ? (
        <div className="rounded-xl border border-rose-200 bg-rose-50/70 px-4 py-3 text-sm text-rose-900 shadow-sm backdrop-blur">
          {error}
        </div>
      ) : null}

      <div className="grid gap-4 md:grid-cols-2">
        <label className="grid gap-1">
          <span className="text-sm font-medium text-neutral-800">Name</span>
          <input
            value={form.name}
            onChange={(e) => setForm((s) => ({ ...s, name: e.target.value }))}
            className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
          />
        </label>

        <label className="grid gap-1">
          <span className="text-sm font-medium text-neutral-800">Amount</span>
          <input
            value={form.amount}
            onChange={(e) => setForm((s) => ({ ...s, amount: e.target.value }))}
            className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
            inputMode="decimal"
          />
        </label>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <label className="grid gap-1">
          <span className="text-sm font-medium text-neutral-800">Date incurred</span>
          <input
            type="date"
            value={form.dateIncurred}
            onChange={(e) => setForm((s) => ({ ...s, dateIncurred: e.target.value }))}
            className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
          />
        </label>

        <label className="grid gap-1">
          <span className="text-sm font-medium text-neutral-800">Description</span>
          <input
            value={form.description}
            onChange={(e) => setForm((s) => ({ ...s, description: e.target.value }))}
            className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
            placeholder="Optional"
          />
        </label>
      </div>

      <div className="flex items-center gap-3">
        <button
          type="submit"
          disabled={submitting}
          className="rounded-xl bg-black px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-black/90 disabled:opacity-60"
        >
          {submitting ? "Saving..." : "Save changes"}
        </button>
        <Link href="/costs" className="text-sm font-medium text-neutral-800 hover:underline">
          Cancel
        </Link>
      </div>
    </form>
  );
}

