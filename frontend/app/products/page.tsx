import Link from "next/link";
import { getProducts } from "@/lib/products.api";
import PageShell from "../ui/PageShell";
import AppNav from "../ui/AppNav";
import ProductTable from "./ui/ProductTable";

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

      <ProductTable products={products} />
    </PageShell>
  );
}
