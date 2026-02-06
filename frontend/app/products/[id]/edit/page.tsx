import Link from "next/link";

import EditProductForm from "./ui/EditProductForm";
import PageShell from "../../../ui/PageShell";
import AppNav from "../../../ui/AppNav";

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

  const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL;
  if (!apiBaseUrl) {
    return (
      <PageShell>
        <div className="flex items-end justify-between gap-4">
          <h1 className="text-3xl font-semibold tracking-tight text-neutral-950">Edit product</h1>
          <div className="flex items-center gap-2">
            <AppNav className="hidden md:flex" />
            <Link
              href="/products"
              className="rounded-xl border border-black/10 bg-white/60 px-4 py-2.5 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
            >
              Back
            </Link>
          </div>
        </div>
      </PageShell>
    );
  }

  const res = await fetch(`${apiBaseUrl}/products/${id}`, { cache: "no-store" });

  if (res.status === 404) {
    return (
      <PageShell>
        <div className="flex items-end justify-between gap-4">
          <h1 className="text-3xl font-semibold tracking-tight text-neutral-950">Edit product</h1>
          <div className="flex items-center gap-2">
            <AppNav className="hidden md:flex" />
            <Link
              href="/products"
              className="rounded-xl border border-black/10 bg-white/60 px-4 py-2.5 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
            >
              Back
            </Link>
          </div>
        </div>

        <div className="mt-6 rounded-2xl border border-black/10 bg-white/60 p-5 text-sm text-neutral-700 shadow-sm backdrop-blur">
          Product not found: <span className="font-mono">{id}</span>
        </div>
      </PageShell>
    );
  }

  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(text || `Failed to load product: ${res.status}`);
  }

  const product = (await res.json()) as ProductDto;

  return (
    <PageShell>
      <div className="flex items-end justify-between gap-4">
        <h1 className="text-3xl font-semibold tracking-tight text-neutral-950">Edit product</h1>
        <div className="flex items-center gap-2">
          <AppNav className="hidden md:flex" />
          <Link
            href="/products"
            className="rounded-xl border border-black/10 bg-white/60 px-4 py-2.5 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
          >
            Back
          </Link>
        </div>
      </div>

      <div className="mt-6 rounded-2xl border border-black/10 bg-white/60 p-5 shadow-sm backdrop-blur">
        <EditProductForm product={product} />
      </div>
    </PageShell>
  );
}
