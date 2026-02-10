"use client";

import { updateProduct } from "@/lib/products.api";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useMemo, useState } from "react";

type ProductDto = {
  id: string;
  name: string;
  description?: string | null;
  price: number;
  stock: number | null;
  isActive: boolean;
};

type Props = {
  product: ProductDto;
};

type FormState = {
  name: string;
  description: string;
  price: string;
  stock: string;
  trackStock: boolean;
  isActive: boolean;
};

export default function EditProductForm({ product }: Props) {
  const router = useRouter();

  const [form, setForm] = useState<FormState>({
    name: product.name ?? "",
    description: product.description ?? "",
    price: String(product.price ?? ""),
    stock: product.stock === null || product.stock === undefined ? "" : String(product.stock),
    trackStock: product.stock !== null && product.stock !== undefined,
    isActive: Boolean(product.isActive),
  });

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const clientValidationError = useMemo(() => {
    if (!form.name.trim()) return "Name is required.";

    const price = Number(form.price);
    if (!Number.isFinite(price) || price <= 0) return "Price must be greater than 0.";

    if (form.trackStock) {
      if (!form.stock.trim()) return "Stock is required.";
      const stock = Number(form.stock);
      if (!Number.isInteger(stock) || stock < 0) return "Stock must be a non-negative integer.";
    }

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
      await updateProduct(product.id, {
        id: product.id,
        name: form.name.trim(),
        description: form.description.trim() ? form.description.trim() : null,
        price: Number(form.price),
        stock: form.trackStock ? Number(form.stock) : null,
        isActive: form.isActive,
      });

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
            required
          />
        </label>

        <label className="grid gap-1">
          <span className="text-sm font-medium text-neutral-800">Price</span>
          <input
            value={form.price}
            onChange={(e) => setForm((s) => ({ ...s, price: e.target.value }))}
            className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
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
          <span className="text-sm font-medium text-neutral-800">Stock</span>
          <input
            value={form.stock}
            onChange={(e) => setForm((s) => ({ ...s, stock: e.target.value }))}
            className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5 disabled:bg-white/40 disabled:text-neutral-500"
            inputMode="numeric"
            disabled={!form.trackStock}
            placeholder={form.trackStock ? "0" : "Untracked"}
          />
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={form.isActive}
              onChange={(e) => setForm((s) => ({ ...s, isActive: e.target.checked }))}
              className="h-4 w-4 rounded border-black/20 bg-white/70"
            />
            <span className="text-sm text-neutral-800">Active</span>
          </label>
          <label className="mt-2 flex items-center gap-2">
            <input
              type="checkbox"
              checked={form.trackStock}
              onChange={(e) =>
                setForm((s) => ({
                  ...s,
                  trackStock: e.target.checked,
                  stock: e.target.checked ? (s.stock.trim() ? s.stock : "0") : "",
                }))
              }
              className="h-4 w-4 rounded border-black/20 bg-white/70"
            />
            <span className="text-sm text-neutral-800">Track stock</span>
          </label>
          <span className="text-xs text-neutral-600">
            {form.trackStock ? "Enter the current stock." : "Untracked (service / no inventory)."}
          </span>

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
        <Link href="/products" className="text-sm font-medium text-neutral-800 hover:underline">
          Cancel
        </Link>
      </div>
    </form>
  );
}
