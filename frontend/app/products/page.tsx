import Link from "next/link";
import { apiJsonServer } from "@/lib/api";
import ProductRowActions from "./ui/ProductRowActions";
import PageShell from "../ui/PageShell";
import AppNav from "../ui/AppNav";

type ProductDto = {
  id: string;
  name: string;
  description?: string | null;
  price: number;
  stock: number | null;
  isActive: boolean;
};

async function getProducts(): Promise<ProductDto[]> {
  return apiJsonServer<ProductDto[]>("/products", { cache: "no-store" });
}

export default async function ProductsPage() {
  const products = await getProducts();

  return (
    <PageShell>
      <header className="flex items-center justify-between">
        <div className="space-y-1">
          <h1 className="text-3xl font-semibold tracking-tight text-neutral-950">Products</h1>
        </div>
        <div className="flex items-center gap-2">
          <AppNav className="hidden md:flex" />
          <Link
            href="/products/new"
            className="rounded-xl bg-black px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-black/90"
          >
            Add product
          </Link>
        </div>
      </header>

      <div className="mt-6 overflow-x-auto rounded-2xl border border-black/10 bg-white/60 shadow-sm backdrop-blur">
        <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="bg-white/40 text-left">
            <th className="px-4 py-3 font-medium text-neutral-700">Name</th>
            <th className="px-4 py-3 text-right font-medium text-neutral-700">Price</th>
            <th className="px-4 py-3 text-right font-medium text-neutral-700">Stock</th>
            <th className="px-4 py-3 font-medium text-neutral-700">Active</th>
            <th className="px-4 py-3 text-right font-medium text-neutral-700">Actions</th>
          </tr>
        </thead>
        <tbody>
          {products.map((p) => (
            <tr key={p.id} className="border-t border-black/10">
              <td className="px-4 py-3 font-medium text-neutral-900">{p.name}</td>
              <td className="px-4 py-3 text-right tabular-nums text-neutral-900">{p.price}</td>
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
        </tbody>
        </table>
      </div>
    </PageShell>
  );
}
