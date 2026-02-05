"use client";

import { apiFetchClient } from "@/lib/api";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";

type Props = {
  productId: string;
};

export default function ProductRowActions({ productId }: Props) {
  const router = useRouter();
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function onDelete() {
    setError(null);
    if (deleting) return;

    const ok = window.confirm("Delete this product?");
    if (!ok) return;

    setDeleting(true);
    try {
      await apiFetchClient(`/products/${productId}`, { method: "DELETE" });

      router.refresh();
    } catch (err) {
      const msg = err instanceof Error ? err.message : "Unknown error";
      setError(msg);
    } finally {
      setDeleting(false);
    }
  }

  return (
    <div className="flex items-center justify-end gap-2">
      <Link
        href={`/products/${productId}/edit`}
        className="rounded-md border border-neutral-300 bg-white px-2.5 py-1.5 text-xs font-medium text-neutral-800 shadow-sm hover:bg-neutral-50"
      >
        Edit
      </Link>
      <button
        type="button"
        onClick={onDelete}
        disabled={deleting}
        className="rounded-md border border-red-200 bg-red-50 px-2.5 py-1.5 text-xs font-medium text-red-800 shadow-sm hover:bg-red-100 disabled:opacity-60"
      >
        {deleting ? "Deleting..." : "Delete"}
      </button>
      {error ? <span className="ml-2 text-xs text-red-700">{error}</span> : null}
    </div>
  );
}
