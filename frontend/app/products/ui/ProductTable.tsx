"use client";

import { useMemo, useState } from "react";

import type { ProductDto } from "@/lib/products.api";
import ProductRowActions from "./ProductRowActions";

function formatMoney(v: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(v);
}

export default function ProductTable({ products }: { products: ProductDto[] }) {
  const [search, setSearch] = useState("");

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return products;

    return products.filter((product) => {
      const haystack = [product.name, product.description ?? "", product.stock === null ? "untracked" : String(product.stock)]
        .join(" ")
        .toLowerCase();
      return haystack.includes(q);
    });
  }, [products, search]);

  return (
    <>
      <div className="mt-6">
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search by name, description, or stock..."
          className="w-full rounded-xl border border-black/10 bg-white/70 px-4 py-2.5 text-sm text-neutral-900 shadow-sm outline-none backdrop-blur placeholder:text-neutral-400 focus:border-black/20 focus:ring-2 focus:ring-black/5"
        />
      </div>

      <div className="mt-4 overflow-x-auto rounded-2xl border border-black/10 bg-white/60 shadow-sm backdrop-blur">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="bg-white/40 text-left">
              <th className="px-4 py-3 font-medium text-neutral-700">Name</th>
              <th className="px-4 py-3 font-medium text-neutral-700">Description</th>
              <th className="px-4 py-3 text-right font-medium text-neutral-700">Price</th>
              <th className="px-4 py-3 text-right font-medium text-neutral-700">Stock</th>
              <th className="px-4 py-3 font-medium text-neutral-700">Active</th>
              <th className="px-4 py-3 text-right font-medium text-neutral-700">Actions</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((p) => (
              <tr key={p.id} className="border-t border-black/10">
                <td className="px-4 py-3 font-medium text-neutral-900">{p.name}</td>
                <td className="px-4 py-3 text-neutral-700">{p.description?.trim() ? p.description : "-"}</td>
                <td className="px-4 py-3 text-right tabular-nums text-neutral-900">{formatMoney(p.price)}</td>
                <td className="px-4 py-3 text-right tabular-nums">
                  {p.stock === null ? (
                    <span className="inline-flex rounded-full border border-black/10 bg-white/70 px-2 py-0.5 text-xs font-medium text-neutral-700">
                      Untracked
                    </span>
                  ) : (
                    <span className="text-neutral-900">{p.stock}</span>
                  )}
                </td>
                <td className="px-4 py-3">
                  <span
                    className={
                      p.isActive
                        ? "inline-flex rounded-full border border-emerald-200 bg-emerald-50 px-2 py-0.5 text-xs font-medium text-emerald-700"
                        : "inline-flex rounded-full border border-black/10 bg-white/60 px-2 py-0.5 text-xs font-medium text-neutral-700"
                    }
                  >
                    {p.isActive ? "Yes" : "No"}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <ProductRowActions productId={p.id} />
                </td>
              </tr>
            ))}
            {filtered.length === 0 && products.length > 0 ? (
              <tr>
                <td colSpan={6} className="px-4 py-12 text-center text-sm text-neutral-600">
                  No products match your search.
                </td>
              </tr>
            ) : null}
            {products.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-4 py-12 text-center text-sm text-neutral-600">
                  No products yet.
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </div>
    </>
  );
}
