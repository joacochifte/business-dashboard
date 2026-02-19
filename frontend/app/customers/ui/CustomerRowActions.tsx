"use client";

import { apiFetchClient } from "@/lib/api";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";

type Props = {
  customerId: string;
};

export default function CustomerRowActions({ customerId }: Props) {
  const router = useRouter();
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function onDelete() {
    setError(null);
    if (deleting) return;

    const ok = window.confirm("Delete this customer?");
    if (!ok) return;

    setDeleting(true);
    try {
      await apiFetchClient(`/customers/${customerId}`, { method: "DELETE" });
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
        href={`/customers/${customerId}/edit`}
        className="rounded-xl border border-black/10 bg-white/60 px-3 py-2 text-xs font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
      >
        Edit
      </Link>
      <button
        type="button"
        onClick={onDelete}
        disabled={deleting}
        className="rounded-xl border border-rose-200 bg-rose-50/70 px-3 py-2 text-xs font-semibold text-rose-900 shadow-sm backdrop-blur transition hover:bg-rose-100/80 disabled:opacity-60"
      >
        {deleting ? "Deleting..." : "Delete"}
      </button>
      {error ? <span className="ml-2 text-xs text-red-700">{error}</span> : null}
    </div>
  );
}
