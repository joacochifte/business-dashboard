"use client";

import { useEffect, useRef, useState } from "react";

type Props = {
  label: string;
  value: string;
  onChange: (value: string) => void;
  className?: string;
};

function formatIsoToDisplay(iso: string) {
  if (!iso) return "";
  const parts = iso.split("-");
  if (parts.length !== 3) return "";
  const [year, month, day] = parts;
  return `${day}/${month}/${year}`;
}

function normalizeDisplayInput(raw: string) {
  const digits = raw.replace(/\D/g, "").slice(0, 8);
  if (digits.length <= 2) return digits;
  if (digits.length <= 4) return `${digits.slice(0, 2)}/${digits.slice(2)}`;
  return `${digits.slice(0, 2)}/${digits.slice(2, 4)}/${digits.slice(4)}`;
}

function parseDisplayToIso(display: string) {
  if (!display.trim()) return "";

  const match = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(display);
  if (!match) return "";

  const day = Number(match[1]);
  const month = Number(match[2]);
  const year = Number(match[3]);

  if (month < 1 || month > 12 || day < 1 || day > 31) return "";

  const probe = new Date(Date.UTC(year, month - 1, day));
  if (
    probe.getUTCFullYear() !== year ||
    probe.getUTCMonth() + 1 !== month ||
    probe.getUTCDate() !== day
  ) {
    return "";
  }

  return `${year.toString().padStart(4, "0")}-${month.toString().padStart(2, "0")}-${day.toString().padStart(2, "0")}`;
}

export default function DateFilterInput({ label, value, onChange, className }: Props) {
  const datePickerRef = useRef<HTMLInputElement>(null);
  const [textValue, setTextValue] = useState(formatIsoToDisplay(value));

  useEffect(() => {
    const formatted = formatIsoToDisplay(value);
    const parsedCurrent = parseDisplayToIso(textValue);

    if (formatted) {
      if (formatted !== textValue) setTextValue(formatted);
      return;
    }

    if (parsedCurrent) {
      setTextValue("");
    }
  }, [textValue, value]);

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

  return (
    <label className={`grid gap-1 ${className ?? ""}`}>
      <span className="text-xs font-medium text-neutral-700">{label}</span>
      <div className="flex items-center gap-2">
        <input
          type="text"
          inputMode="numeric"
          value={textValue}
          onChange={(e) => {
            const normalized = normalizeDisplayInput(e.target.value);
            setTextValue(normalized);
            onChange(parseDisplayToIso(normalized));
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
          value={value}
          onChange={(e) => onChange(e.target.value)}
          className="h-0 w-0 opacity-0"
          tabIndex={-1}
          aria-hidden="true"
        />
      </div>
    </label>
  );
}
