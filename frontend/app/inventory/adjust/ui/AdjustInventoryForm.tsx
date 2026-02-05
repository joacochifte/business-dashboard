"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";

import { adjustInventory } from "@/lib/inventory.api";
import { fetchJson } from "@/lib/api";
import type { ProductDto } from "@/lib/products.api";

type FormState = {
  productId: string;
  delta: string;
};

function isInt(v: number) {
  return Number.isFinite(v) && Math.floor(v) === v;
}

export default function AdjustInventoryForm() {
  const router = useRouter();

  const [products, setProducts] = useState<ProductDto[]>([]);
  const [loading, setLoading] = useState(true);

  const [form, setForm] = useState<FormState>({
    productId: "",
    delta: "",
  });

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let alive = true;

    async function load() {
      setLoading(true);
      try {
        const data = await fetchJson<ProductDto[]>("/products");
        if (!alive) return;
        setProducts(data);
      } catch (err) {
        if (!alive) return;
        const msg = err instanceof Error ? err.message : "Failed to load products.";
        setError(msg);
      } finally {
        if (alive) setLoading(false);
      }
    }

    load();
    return () => {
      alive = false;
    };
  }, []);

  const selected = useMemo(() => products.find((p) => p.id === form.productId) ?? null, [products, form.productId]);

  const clientValidationError = useMemo(() => {
    if (!form.productId) return "Product is required.";
    const delta = Number(form.delta);
    if (!isInt(delta) || delta === 0) return "Delta must be a non-zero integer (e.g. -1, 5).";
    return null;
  }, [form]);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);

    if (clientValidationError) {
      setError(clientValidationError);
      return;
    }

    setSubmitting(true);
    try {
      await adjustInventory({ productId: form.productId, delta: Number(form.delta) });
      router.push(`/inventory?productId=${encodeURIComponent(form.productId)}`);
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
        <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-800">{error}</div>
      ) : null}

      <div className="grid gap-4 md:grid-cols-2">
        <label className="grid gap-1 md:col-span-2">
          <span className="text-sm font-medium text-neutral-800">Product</span>
          <select
            value={form.productId}
            onChange={(e) => setForm((s) => ({ ...s, productId: e.target.value }))}
            disabled={loading}
            className="rounded-md border border-neutral-300 bg-white px-3 py-2 text-sm outline-none focus:border-neutral-500 disabled:bg-neutral-100 disabled:text-neutral-500"
          >
            <option value="">{loading ? "Loading products..." : "Select a product"}</option>
            {products.map((p) => (
              <option key={p.id} value={p.id}>
                {p.name}
              </option>
            ))}
          </select>
          {selected ? (
            <span className="text-xs text-neutral-600">
              Current stock:{" "}
              <span className="font-medium text-neutral-800">{selected.stock === null ? "Untracked" : selected.stock}</span>
              {!selected.isActive ? <span className="ml-2 text-xs text-red-700">(Inactive)</span> : null}
            </span>
          ) : (
            <span className="text-xs text-neutral-600">Choose a product to adjust its stock.</span>
          )}
        </label>

        <label className="grid gap-1">
          <span className="text-sm font-medium text-neutral-800">Delta</span>
          <input
            value={form.delta}
            onChange={(e) => setForm((s) => ({ ...s, delta: e.target.value }))}
            className="rounded-md border border-neutral-300 px-3 py-2 text-sm outline-none focus:border-neutral-500"
            inputMode="numeric"
            placeholder="-1 or 5"
          />
          <span className="text-xs text-neutral-600">Use a negative number to remove stock, positive to add.</span>
        </label>

        <div className="rounded-lg border border-neutral-200 bg-neutral-50 p-3 text-xs text-neutral-700">
          <div className="font-medium text-neutral-800">Notes</div>
          <div className="mt-1">
            - Products marked as <span className="font-medium">Untracked</span> shouldn&apos;t be adjusted (backend will reject).
          </div>
          <div className="mt-1">- This creates an inventory movement for history.</div>
        </div>
      </div>

      <div className="flex items-center gap-3">
        <button
          type="submit"
          disabled={submitting}
          className="rounded-md bg-black px-3 py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          {submitting ? "Saving..." : "Apply adjustment"}
        </button>
        <a href="/inventory" className="text-sm text-neutral-700 hover:underline">
          Cancel
        </a>
      </div>
    </form>
  );
}

