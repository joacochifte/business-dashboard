"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useMemo, useRef, useState } from "react";
import { updateCustomer, type CustomerDto } from "@/lib/customers.api";

type Props = {
  customer: CustomerDto;
};

type FormState = {
  name: string;
  email: string;
  phone: string;
  birthDateIso: string;
  birthDateText: string;
};

function toDateInputValue(iso: string | null | undefined): string {
  if (!iso) return "";
  // ISO might be "1990-05-20T00:00:00Z" → extract "1990-05-20"
  return iso.slice(0, 10);
}

function formatIsoToDisplay(iso: string): string {
  if (!iso) return "";
  const parts = iso.split("-");
  if (parts.length !== 3) return "";
  const [year, month, day] = parts;
  return `${day}/${month}/${year}`;
}

function normalizeDisplayInput(raw: string): string {
  const digits = raw.replace(/\D/g, "").slice(0, 8);
  if (digits.length <= 2) return digits;
  if (digits.length <= 4) return `${digits.slice(0, 2)}/${digits.slice(2)}`;
  return `${digits.slice(0, 2)}/${digits.slice(2, 4)}/${digits.slice(4)}`;
}

function parseDisplayToIso(display: string): string | null {
  const m = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(display);
  if (!m) return null;

  const day = Number(m[1]);
  const month = Number(m[2]);
  const year = Number(m[3]);

  if (month < 1 || month > 12 || day < 1 || day > 31) return null;

  const probe = new Date(Date.UTC(year, month - 1, day));
  if (
    probe.getUTCFullYear() !== year ||
    probe.getUTCMonth() + 1 !== month ||
    probe.getUTCDate() !== day
  ) {
    return null;
  }

  return `${year.toString().padStart(4, "0")}-${month.toString().padStart(2, "0")}-${day
    .toString()
    .padStart(2, "0")}`;
}

export default function EditCustomerForm({ customer }: Props) {
  const router = useRouter();
  const datePickerRef = useRef<HTMLInputElement>(null);
  const initialBirthDateIso = toDateInputValue(customer.birthDate);

  const [form, setForm] = useState<FormState>({
    name: customer.name,
    email: customer.email ?? "",
    phone: customer.phone ?? "",
    birthDateIso: initialBirthDateIso,
    birthDateText: formatIsoToDisplay(initialBirthDateIso),
  });

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const clientValidationError = useMemo(() => {
    if (!form.name.trim()) return "Name is required.";
    if (form.birthDateText && !parseDisplayToIso(form.birthDateText)) {
      return "Birthday must use DD/MM/YYYY (for example 10/06/1995).";
    }
    return null;
  }, [form]);

  function openDatePicker() {
    const input = datePickerRef.current;
    if (!input) return;

    if (typeof input.showPicker === "function") {
      input.showPicker();
      return;
    }

    input.focus();
    input.click();
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);

    if (clientValidationError) {
      setError(clientValidationError);
      return;
    }

    setSubmitting(true);
    try {
      await updateCustomer(customer.id, {
        id: customer.id,
        name: form.name.trim(),
        email: form.email.trim() || null,
        phone: form.phone.trim() || null,
        birthDate: form.birthDateIso || null,
        isActive: true,
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
          <div className="flex items-center gap-2">
            <input
              type="text"
              inputMode="numeric"
              value={form.birthDateText}
              onChange={(e) => {
                const birthDateText = normalizeDisplayInput(e.target.value);
                const birthDateIso = parseDisplayToIso(birthDateText) ?? "";
                setForm((s) => ({ ...s, birthDateText, birthDateIso }));
              }}
              placeholder="DD/MM/YYYY"
              className="w-full rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none focus:border-black/20 focus:ring-2 focus:ring-black/5"
            />
            <button
              type="button"
              onClick={openDatePicker}
              className="rounded-xl border border-black/10 bg-white/60 px-3 py-2 text-xs font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
            >
              Pick
            </button>
            <input
              ref={datePickerRef}
              type="date"
              value={form.birthDateIso}
              onChange={(e) => {
                const birthDateIso = e.target.value;
                setForm((s) => ({
                  ...s,
                  birthDateIso,
                  birthDateText: formatIsoToDisplay(birthDateIso),
                }));
              }}
              className="h-0 w-0 opacity-0"
              tabIndex={-1}
              aria-hidden="true"
            />
          </div>
        </label>
      </div>

      <div className="flex items-center gap-3 pt-2">
        <button
          type="submit"
          disabled={submitting}
          className="rounded-xl bg-black px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-black/90 disabled:opacity-60"
        >
          {submitting ? "Saving..." : "Save changes"}
        </button>
        <Link href="/customers" className="text-sm font-medium text-neutral-800 hover:underline">
          Cancel
        </Link>
      </div>
    </form>
  );
}
