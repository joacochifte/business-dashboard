import Link from "next/link";
import { apiJsonServer } from "@/lib/api";
import ProductRowActions from "./ui/ProductRowActions";

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
    <main className="p-6">
      <header className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Products</h1>
        <Link
          href="/products/new"
          className="rounded-md bg-black px-3 py-2 text-sm font-medium text-white hover:bg-black/90"
        >
          Add product
        </Link>
      </header>

      <div className="mt-4 overflow-x-auto rounded-lg border border-neutral-200">
        <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="bg-neutral-50 text-left">
            <th className="px-3 py-2 font-medium text-neutral-700">Name</th>
            <th className="px-3 py-2 text-right font-medium text-neutral-700">Price</th>
            <th className="px-3 py-2 text-right font-medium text-neutral-700">Stock</th>
            <th className="px-3 py-2 font-medium text-neutral-700">Active</th>
            <th className="px-3 py-2 text-right font-medium text-neutral-700">Actions</th>
          </tr>
        </thead>
        <tbody>
          {products.map((p) => (
            <tr key={p.id} className="border-t border-neutral-200">
              <td className="px-3 py-2">{p.name}</td>
              <td className="px-3 py-2 text-right tabular-nums">{p.price}</td>
              <td className="px-3 py-2 text-right tabular-nums">
                {p.stock === null ? (
                  <span className="text-xs font-medium text-neutral-600">Untracked</span>
                ) : (
                  p.stock
                )}
              </td>
              <td className="px-3 py-2">
                <span
                  className={
                    p.isActive
                      ? "inline-flex rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-700"
                      : "inline-flex rounded-full bg-neutral-100 px-2 py-0.5 text-xs font-medium text-neutral-700"
                  }
                >
                  {p.isActive ? "Yes" : "No"}
                </span>
              </td>
              <td className="px-3 py-2">
                <ProductRowActions productId={p.id} />
              </td>
            </tr>
          ))}
        </tbody>
        </table>
      </div>
    </main>
  );
}
