"use client";

import { useRouter } from "next/navigation";
import { useMemo, useState } from "react";

type FormState = {
  name: string;
  description: string;
  price: string;
  trackStock: boolean;
  initialStock: string;
};

export default function NewProductForm() {
  const router = useRouter();

  const [form, setForm] = useState<FormState>({
    name: "",
    description: "",
    price: "",
    trackStock: true,
    initialStock: "0",
  });

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL;

  const clientValidationError = useMemo(() => {
    if (!form.name.trim()) return "Name is required.";

    const price = Number(form.price);
    if (!Number.isFinite(price) || price <= 0) return "Price must be greater than 0.";

    if (form.trackStock) {
      const stock = Number(form.initialStock);
      if (!Number.isInteger(stock) || stock < 0) return "Initial stock must be a non-negative integer.";
    }

    return null;
  }, [form]);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);

    if (!apiBaseUrl) {
      setError("Missing NEXT_PUBLIC_API_BASE_URL. Check frontend/.env.local and restart npm run dev.");
      return;
    }

    if (clientValidationError) {
      setError(clientValidationError);
      return;
    }

    setSubmitting(true);
    try {
      const payload = {
        name: form.name.trim(),
        description: form.description.trim() ? form.description.trim() : null,
        price: Number(form.price),
        initialStock: form.trackStock ? Number(form.initialStock) : null,
      };

      const res = await fetch(`${apiBaseUrl}/products`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });

      if (!res.ok) {
        const text = await res.text().catch(() => "");
        throw new Error(text || `Request failed: ${res.status}`);
      }

      router.push("/products");
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
            placeholder="SAAMA Therapy"
            required
          />
        </label>

        <label className="grid gap-1">
          <span className="text-sm font-medium text-neutral-800">Price</span>
          <input
            value={form.price}
            onChange={(e) => setForm((s) => ({ ...s, price: e.target.value }))}
            className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
            placeholder="100"
            inputMode="decimal"
          />
        </label>

        <label className="grid gap-1 md:col-span-2">
          <span className="text-sm font-medium text-neutral-800">Description</span>
          <textarea
            value={form.description}
            onChange={(e) => setForm((s) => ({ ...s, description: e.target.value }))}
            className="min-h-[96px] rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
            placeholder="Optional"
          />
        </label>

        <label className="grid gap-1">
          <span className="text-sm font-medium text-neutral-800">Initial stock</span>
          <input
            value={form.initialStock}
            onChange={(e) => setForm((s) => ({ ...s, initialStock: e.target.value }))}
            className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5 disabled:bg-white/40 disabled:text-neutral-500"
            placeholder="0"
            inputMode="numeric"
            disabled={!form.trackStock}
          />
          <label className="flex items-center gap-2">
          <input
            type="checkbox"
            checked={form.trackStock}
            onChange={(e) =>
              setForm((s) => ({
                ...s,
                trackStock: e.target.checked,
                initialStock: e.target.checked ? s.initialStock : "0",
              }))
            }
            className="h-4 w-4 rounded border-black/20 bg-white/70"
          />
          <span className="text-sm text-neutral-800">Track stock</span>
        </label>
          <span className="text-xs text-neutral-600">Uncheck for services or products without inventory.</span>
        </label>
      </div>

      <div className="flex items-center gap-3">
        <button
          type="submit"
          disabled={submitting}
          className="rounded-xl bg-black px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-black/90 disabled:opacity-60"
        >
          {submitting ? "Saving..." : "Save"}
        </button>
        <a href="/products" className="text-sm font-medium text-neutral-800 hover:underline">
          Cancel
        </a>
      </div>
    </form>
  );
}
