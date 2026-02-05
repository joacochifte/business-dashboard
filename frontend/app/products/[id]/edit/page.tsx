import Link from "next/link";

import EditProductForm from "./ui/EditProductForm";

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
      <main className="p-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-semibold">Edit product</h1>
          <Link href="/products" className="text-sm text-neutral-700 hover:underline">
            Back
          </Link>
        </div>
      </main>
    );
  }

  const res = await fetch(`${apiBaseUrl}/products/${id}`, { cache: "no-store" });

  if (res.status === 404) {
    return (
      <main className="p-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-semibold">Edit product</h1>
          <Link href="/products" className="text-sm text-neutral-700 hover:underline">
            Back
          </Link>
        </div>

        <div className="mt-4 rounded-lg border border-neutral-200 bg-white p-4 text-sm text-neutral-700">
          Product not found: <span className="font-mono">{id}</span>
        </div>
      </main>
    );
  }

  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(text || `Failed to load product: ${res.status}`);
  }

  const product = (await res.json()) as ProductDto;

  return (
    <main className="p-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Edit product</h1>
        <Link href="/products" className="text-sm text-neutral-700 hover:underline">
          Back
        </Link>
      </div>

      <div className="mt-4 rounded-lg border border-neutral-200 bg-white p-4">
        <EditProductForm product={product} />
      </div>
    </main>
  );
}
