"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";

import { fetchJson } from "@/lib/api";
import type { ProductDto } from "@/lib/products.api";
import type { CustomerDto } from "@/lib/customers.api";
import { createSale, type SaleCreationDto, type SaleItemDto } from "@/lib/sales.api";

type FormItem = {
  productId: string;
  unitPrice: string;
  quantity: string;
  specialPrice: string;
  useSpecialPrice: boolean;
};

type FormState = {
  customerId: string;
  paymentMethod: string;
  isDebt: boolean;
  items: FormItem[];
};

function toNumber(v: string) {
  const n = Number(v);
  return Number.isFinite(n) ? n : NaN;
}

export default function NewSaleForm() {
  const router = useRouter();

  const [products, setProducts] = useState<ProductDto[]>([]);
  const [productsLoading, setProductsLoading] = useState(true);
  const [customers, setCustomers] = useState<CustomerDto[]>([]);

  const [form, setForm] = useState<FormState>({
    customerId: "",
    paymentMethod: "",
    isDebt: false,
    items: [{ productId: "", unitPrice: "", quantity: "1", specialPrice: "", useSpecialPrice: false }],
  });

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let alive = true;

    async function loadProducts() {
      setProductsLoading(true);
      try {
        const data = await fetchJson<ProductDto[]>("/products");
        if (!alive) return;
        setProducts(data);
      } catch (err) {
        if (!alive) return;
        const msg = err instanceof Error ? err.message : "Failed to load products.";
        setError(msg);
      } finally {
        if (alive) setProductsLoading(false);
      }
    }

    async function loadCustomers() {
      try {
        const data = await fetchJson<CustomerDto[]>("/customers");
        if (alive) setCustomers(data);
      } catch {
        // non-critical
      }
    }

    loadProducts();
    loadCustomers();
    return () => {
      alive = false;
    };
  }, []);

  const productsById = useMemo(() => {
    return new Map(products.map((p) => [p.id, p]));
  }, [products]);

  const computedTotal = useMemo(() => {
    return form.items.reduce((acc, it) => {
      const quantity = toNumber(it.quantity);
      if (!Number.isFinite(quantity)) return acc;
      
      const price = it.useSpecialPrice ? toNumber(it.specialPrice) : toNumber(it.unitPrice);
      if (!Number.isFinite(price) || price < 0) return acc;
      
      return acc + price * quantity;
    }, 0);
  }, [form.items]);

  const clientValidationError = useMemo(() => {
    if (!form.items.length) return "At least one item is required.";

    for (let i = 0; i < form.items.length; i++) {
      const it = form.items[i];
      if (!it.productId.trim()) return `Item #${i + 1}: productId is required.`;

      const unitPrice = toNumber(it.unitPrice);
      if (!Number.isFinite(unitPrice) || unitPrice <= 0) return `Item #${i + 1}: unitPrice must be > 0.`;

      const quantity = toNumber(it.quantity);
      if (!Number.isInteger(quantity) || quantity <= 0) return `Item #${i + 1}: quantity must be a positive integer.`;

      if (it.useSpecialPrice) {
        const specialPrice = toNumber(it.specialPrice);
        if (!Number.isFinite(specialPrice) || specialPrice < 0) return `Item #${i + 1}: special price must be >= 0.`;
      }
    }

    return null;
  }, [form.items]);

  function setItem(idx: number, patch: Partial<FormItem>) {
    setForm((s) => ({
      ...s,
      items: s.items.map((it, i) => (i === idx ? { ...it, ...patch } : it)),
    }));
  }

  function onChangeProduct(idx: number, productId: string) {
    const p = productsById.get(productId);
    setItem(idx, {
      productId,
      unitPrice: p ? String(p.price) : "",
    });
  }

  function addItem() {
    setForm((s) => ({
      ...s,
      items: [...s.items, { productId: "", unitPrice: "", quantity: "1", specialPrice: "", useSpecialPrice: false }],
    }));
  }

  function removeItem(idx: number) {
    setForm((s) => ({
      ...s,
      items: s.items.filter((_, i) => i !== idx),
    }));
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);

    if (clientValidationError) {
      setError(clientValidationError);
      return;
    }

    const items: SaleItemDto[] = form.items.map((it) => ({
      productId: it.productId.trim(),
      unitPrice: Number(it.unitPrice),
      quantity: Number(it.quantity),
      specialPrice: it.useSpecialPrice ? Number(it.specialPrice) : null,
    }));

    const payload: SaleCreationDto = {
      items,
      total: computedTotal,
      customerId: form.customerId || null,
      paymentMethod: form.paymentMethod.trim() ? form.paymentMethod.trim() : null,
      isDebt: form.isDebt,
    };

    setSubmitting(true);
    try {
      await createSale(payload);
      router.push("/sales");
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
          <span className="text-sm font-medium text-neutral-800">Customer</span>
          <select
            value={form.customerId}
            onChange={(e) => setForm((s) => ({ ...s, customerId: e.target.value }))}
            className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
          >
            <option value="">— None —</option>
            {customers.filter((c) => c.isActive).map((c) => (
              <option key={c.id} value={c.id}>{c.name}</option>
            ))}
          </select>
        </label>

        <label className="grid gap-1">
          <span className="text-sm font-medium text-neutral-800">Payment method</span>
          <select
            value={form.paymentMethod}
            onChange={(e) => setForm((s) => ({ ...s, paymentMethod: e.target.value }))}
            className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
          >
            <option value="">Select payment method</option>
            <option value="Cash">Cash</option>
            <option value="Card">Card</option>
            <option value="Transfer">Transfer</option>
          </select>
        </label>
      </div>

      <label className="flex items-center gap-3">
        <input
          type="checkbox"
          checked={form.isDebt}
          onChange={(e) => setForm((s) => ({ ...s, isDebt: e.target.checked }))}
          className="h-4 w-4 rounded border-black/20 accent-neutral-800"
        />
        <span className="text-sm font-medium text-neutral-800">Mark as debt</span>
      </label>

      <div className="rounded-2xl border border-black/10 bg-white/60 shadow-sm backdrop-blur">
        <div className="flex items-center justify-between border-b border-black/10 bg-white/40 px-4 py-3">
          <div className="text-sm font-medium text-neutral-800">Items</div>
          <button
            type="button"
            onClick={addItem}
            className="rounded-xl border border-black/10 bg-white/60 px-3 py-2 text-xs font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
          >
            Add item
          </button>
        </div>

        <div className="space-y-3 p-4">
          {form.items.map((it, idx) => (
            <div key={idx} className="space-y-2">
              <div className="grid gap-3 md:grid-cols-12">
                <label className="grid gap-1 md:col-span-6">
                  <span className="text-xs font-medium text-neutral-700">Product</span>
                  <select
                    value={it.productId}
                    onChange={(e) => onChangeProduct(idx, e.target.value)}
                    className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5 disabled:bg-white/40 disabled:text-neutral-500"
                    disabled={productsLoading}
                  >
                    <option value="">{productsLoading ? "Loading products..." : "Select a product"}</option>
                    {products.map((p) => (
                      <option key={p.id} value={p.id}>
                        {p.name}
                      </option>
                    ))}
                  </select>
                </label>

                <span className="grid gap-1 md:col-span-2">
                  <span className="text-xs font-medium text-neutral-700">Unit price</span>
                  <span className="rounded-xl border border-black/10 bg-white/40 px-3 py-2 text-sm font-medium text-neutral-800">
                    {it.unitPrice || "--"}
                  </span>
                </span>

                <span className="grid gap-1 md:col-span-2">
                  <span className="text-xs font-medium text-neutral-700">Qty</span>
                  <input
                    value={it.quantity}
                    onChange={(e) => setItem(idx, { quantity: e.target.value })}
                    className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
                    inputMode="numeric"
                    placeholder="1"
                  />
                </span>

                <div className="flex items-end md:col-span-2">
                  <button
                    type="button"
                    onClick={() => removeItem(idx)}
                    disabled={form.items.length <= 1}
                    className="w-full rounded-xl border border-rose-200 bg-rose-50/70 px-3 py-2 text-xs font-semibold text-rose-900 shadow-sm backdrop-blur transition hover:bg-rose-100/80 disabled:opacity-60"
                    title={form.items.length <= 1 ? "At least one item is required" : "Remove item"}
                  >
                    ✕
                  </button>
                </div>
              </div>

              <label className="flex items-center gap-2 pl-0">
                <input
                  type="checkbox"
                  checked={it.useSpecialPrice}
                  onChange={(e) => setItem(idx, { useSpecialPrice: e.target.checked, specialPrice: e.target.checked ? "" : "" })}
                  className="h-4 w-4 rounded border-black/20 accent-neutral-800"
                />
                <span className="text-xs font-medium text-neutral-700">Special price (per product)</span>
                {it.useSpecialPrice && (
                  <input
                    type="text"
                    inputMode="decimal"
                    value={it.specialPrice}
                    onChange={(e) => setItem(idx, { specialPrice: e.target.value })}
                    placeholder="0.00"
                    className="ml-2 w-24 rounded-xl border border-black/10 bg-white/70 px-2 py-1.5 text-xs outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
                  />
                )}
              </label>
            </div>
          ))}
        </div>
      </div>

      <div className="flex items-center justify-between rounded-2xl border border-black/10 bg-white/60 px-4 py-3 shadow-sm backdrop-blur">
        <span className="text-sm text-neutral-700">Total</span>
        <span className="text-sm font-semibold tabular-nums">{computedTotal.toFixed(2)}</span>
      </div>

      <div className="flex items-center gap-3">
        <button
          type="submit"
          disabled={submitting}
          className="rounded-xl bg-black px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-black/90 disabled:opacity-60"
        >
          {submitting ? "Saving..." : "Save"}
        </button>
        <a href="/sales" className="text-sm font-medium text-neutral-800 hover:underline">
          Cancel
        </a>
      </div>

      <p className="text-xs text-neutral-600">
        Unit price is taken from the product price. If you need discounts later, we can add them as a separate field.
      </p>
    </form>
  );
}
