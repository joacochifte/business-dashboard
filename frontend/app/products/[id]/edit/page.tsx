import Link from "next/link";
import { getProductById } from "@/lib/products.api";
import type { ApiError } from "@/lib/api";

import EditProductForm from "./ui/EditProductForm";
import PageShell from "../../../ui/PageShell";

type Props = {
  params: Promise<{ id: string }>;
};

type ProductDto = {
  id: string;
  name: string;
  description?: string | null;
  price: number;
  stock: number | null;
  isActive: boolean;
};

export default async function EditProductPage({ params }: Props) {
  const { id } = await params;
  let product: ProductDto;
  try {
    product = await getProductById(id);
  } catch (error) {
    const apiError = error as ApiError;
    if (apiError?.status === 404) {
      const backLink = (
        <Link
          href="/products"
          className="rounded-xl border border-black/10 bg-white/60 px-4 py-2.5 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
        >
          Back
        </Link>
      );
      return (
        <PageShell actions={backLink}>
          <div className="rounded-2xl border border-black/10 bg-white/60 p-5 text-sm text-neutral-700 shadow-sm backdrop-blur">
            Product not found: <span className="font-mono">{id}</span>
          </div>
        </PageShell>
      );
    }

    throw error;
  }

  const backLink = (
    <Link
      href="/products"
      className="rounded-xl border border-black/10 bg-white/60 px-4 py-2.5 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
    >
      Back
    </Link>
  );

  return (
    <PageShell actions={backLink}>
      <div className="rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
        <EditProductForm product={product} />
      </div>
    </PageShell>
  );
}
