import Link from "next/link";
import { getProducts } from "@/lib/products.api";
import PageShell from "../ui/PageShell";
import ProductTable from "./ui/ProductTable";

export default async function ProductsPage() {
  const products = await getProducts();

  return (
    <PageShell
      actions={
        <Link
          href="/products/new"
          className="rounded-xl bg-black px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-black/90"
        >
          Add product
        </Link>
      }
    >
      <ProductTable products={products} />
    </PageShell>
  );
}
