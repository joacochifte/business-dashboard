"use client";

import { createCustomer } from "@/lib/customers.api";
import { useRouter } from "next/navigation";
import { useMemo, useState } from "react";

type FormState = {
  name: string;
  email: string;
  phone: string;
  birthDate: string;
};

export default function NewCustomerForm() {
  const router = useRouter();

  const [form, setForm] = useState<FormState>({
    name: "",
    email: "",
    phone: "",
    birthDate: "",
  });

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const clientValidationError = useMemo(() => {
    if (!form.name.trim()) return "Name is required.";
    return null;
  }, [form]);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);

    if (clientValidationError) {
      setError(clientValidationError);
      return;
    }

    setSubmitting(true);
    try {
      await createCustomer({
        name: form.name.trim(),
        email: form.email.trim() || null,
        phone: form.phone.trim() || null,
        birthDate: form.birthDate || null,
      });
      router.push("/customers");
      router.refresh();
    } catch (err) {
      const msg = err instanceof Error ? err.message : "Unknown error";
      setError(msg);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form onSubmit={onSubmit} className="space-y-4">
      {error ? (
        <div className="rounded-xl border border-rose-200 bg-rose-50/70 px-4 py-3 text-sm text-rose-900 shadow-sm backdrop-blur">
          {error}
        </div>
      ) : null}

      <div className="grid gap-4 md:grid-cols-2">
        <label className="grid gap-1">
          <span className="text-sm font-medium text-neutral-800">Name <span className="text-rose-500">*</span></span>
          <input
            value={form.name}
            onChange={(e) => setForm((s) => ({ ...s, name: e.target.value }))}
            className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
            placeholder="Jane Doe"
            required
          />
        </label>

        <label className="grid gap-1">
          <span className="text-sm font-medium text-neutral-800">Email</span>
          <input
            type="email"
            value={form.email}
            onChange={(e) => setForm((s) => ({ ...s, email: e.target.value }))}
            className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
            placeholder="jane@example.com"
          />
        </label>

        <label className="grid gap-1">
          <span className="text-sm font-medium text-neutral-800">Phone</span>
          <input
            value={form.phone}
            onChange={(e) => setForm((s) => ({ ...s, phone: e.target.value }))}
            className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
            placeholder="+1 555 000 0000"
          />
        </label>

        <label className="grid gap-1">
          <span className="text-sm font-medium text-neutral-800">Birthday</span>
          <input
            type="date"
            lang="es"
            value={form.birthDate}
            onChange={(e) => setForm((s) => ({ ...s, birthDate: e.target.value }))}
            className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
          />
        </label>
      </div>

      <div className="flex items-center gap-3 pt-2">
        <button
          type="submit"
          disabled={submitting}
          className="rounded-xl bg-black px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-black/90 disabled:opacity-60"
        >
          {submitting ? "Saving..." : "Create customer"}
        </button>
      </div>
    </form>
  );
}
