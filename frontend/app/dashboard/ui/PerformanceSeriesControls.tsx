"use client";

import { useEffect, useState, useTransition } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";

import type { PerformanceMetric } from "./PerformanceSeriesChart";

type Props = {
  performanceMetric: PerformanceMetric;
  compareYears: number[];
  compareMonthsRaw: string;
  includeForecast: boolean;
  forecastPeriods: number;
  comparisonRangeEnabled: boolean;
  allowSpecificMonths: boolean;
};

const METRIC_OPTIONS: Array<{ value: PerformanceMetric; label: string }> = [
  { value: "revenue", label: "Revenue" },
  { value: "marginPct", label: "Margin %" },
  { value: "gains", label: "Gains" },
  { value: "avgTicket", label: "Avg ticket" },
];

const YEAR_OPTIONS = [1, 2, 3];

function normalizeMonthsInput(value: string) {
  return value
    .split(",")
    .map((item) => item.trim())
    .filter((item, index, items) => /^\d{4}-\d{2}$/.test(item) && items.indexOf(item) === index)
    .join(", ");
}

export default function PerformanceSeriesControls({
  performanceMetric,
  compareYears,
  compareMonthsRaw,
  includeForecast,
  forecastPeriods,
  comparisonRangeEnabled,
  allowSpecificMonths,
}: Props) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [isPending, startTransition] = useTransition();
  const [monthsInput, setMonthsInput] = useState(compareMonthsRaw);

  useEffect(() => {
    setMonthsInput(compareMonthsRaw);
  }, [compareMonthsRaw]);

  function replaceParams(mutator: (params: URLSearchParams) => void) {
    const params = new URLSearchParams(searchParams.toString());
    mutator(params);
    const query = params.toString();

    startTransition(() => {
      router.replace(query ? `${pathname}?${query}` : pathname, { scroll: false });
    });
  }

  function updateMetric(nextMetric: PerformanceMetric) {
    replaceParams((params) => {
      params.set("performanceMetric", nextMetric);
    });
  }

  function toggleCompareYear(yearOffset: number) {
    if (!comparisonRangeEnabled) return;

    replaceParams((params) => {
      const current = params
        .getAll("compareYear")
        .map((value) => Number(value))
        .filter((value) => Number.isInteger(value) && value > 0);

      const next = current.includes(yearOffset) ? current.filter((value) => value !== yearOffset) : [...current, yearOffset];
      params.delete("compareYear");
      next
        .sort((a, b) => a - b)
        .forEach((value) => params.append("compareYear", String(value)));
    });
  }

  function commitMonthsInput(rawValue: string) {
    const normalized = normalizeMonthsInput(rawValue);

    replaceParams((params) => {
      if (!comparisonRangeEnabled || !allowSpecificMonths || !normalized) {
        params.delete("compareMonths");
        return;
      }

      params.set("compareMonths", normalized);
    });
  }

  function toggleForecast(nextValue: boolean) {
    replaceParams((params) => {
      if (nextValue) {
        params.set("includeForecast", "1");
      } else {
        params.delete("includeForecast");
      }
    });
  }

  function updateForecastPeriods(nextValue: number) {
    replaceParams((params) => {
      params.set("forecastPeriods", String(nextValue));
    });
  }

  return (
    <div className="mt-4 rounded-2xl border border-black/10 bg-white/45 p-4">
      <div className="grid gap-4 xl:grid-cols-[1.3fr_1fr_1.2fr_0.9fr]">
        <div className="space-y-2">
          <div className="text-xs font-medium text-neutral-700">Metric</div>
          <div className="flex flex-wrap gap-2">
            {METRIC_OPTIONS.map((option) => {
              const active = option.value === performanceMetric;
              return (
                <button
                  key={option.value}
                  type="button"
                  onClick={() => updateMetric(option.value)}
                  aria-pressed={active}
                  className={`rounded-xl px-3 py-2 text-sm font-medium transition ${
                    active
                      ? "bg-black text-white shadow-sm"
                      : "border border-black/10 bg-white/70 text-neutral-800 hover:bg-white"
                  }`}
                >
                  {option.label}
                </button>
              );
            })}
          </div>
        </div>

        <div className="space-y-2">
          <div className="text-xs font-medium text-neutral-700">Compare years</div>
          <div className="flex flex-wrap gap-2">
            {YEAR_OPTIONS.map((yearOffset) => {
              const active = compareYears.includes(yearOffset);
              return (
                <button
                  key={yearOffset}
                  type="button"
                  onClick={() => toggleCompareYear(yearOffset)}
                  disabled={!comparisonRangeEnabled}
                  aria-pressed={active}
                  className={`rounded-xl px-3 py-2 text-sm font-medium transition ${
                    active
                      ? "bg-blue-600 text-white shadow-sm"
                      : "border border-black/10 bg-white/70 text-neutral-800 hover:bg-white"
                  } disabled:cursor-not-allowed disabled:border-black/5 disabled:bg-white/35 disabled:text-neutral-400`}
                >
                  {yearOffset}y ago
                </button>
              );
            })}
          </div>
        </div>

        <label className="grid gap-2">
          <span className="text-xs font-medium text-neutral-700">Specific months</span>
          <input
            type="text"
            value={monthsInput}
            onChange={(event) => setMonthsInput(event.target.value)}
            onBlur={(event) => commitMonthsInput(event.target.value)}
            onKeyDown={(event) => {
              if (event.key === "Enter") {
                event.preventDefault();
                commitMonthsInput(monthsInput);
              }
            }}
            placeholder="2025-11, 2024-08"
            disabled={!comparisonRangeEnabled || !allowSpecificMonths}
            className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none transition focus:border-black/20 focus:ring-2 focus:ring-black/5 disabled:cursor-not-allowed disabled:bg-white/35 disabled:text-neutral-400"
          />
          <span className="text-[11px] text-neutral-500">Day-based views only. Applies on blur or Enter.</span>
        </label>

        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-1">
          <label className="flex items-center justify-between rounded-xl border border-black/10 bg-white/70 px-3 py-2">
            <span className="text-sm font-medium text-neutral-800">Forecast</span>
            <input
              type="checkbox"
              checked={includeForecast}
              onChange={(event) => toggleForecast(event.target.checked)}
              disabled={performanceMetric !== "revenue"}
              className="h-4 w-4 rounded border-black/20 text-black focus:ring-black/20 disabled:cursor-not-allowed"
            />
          </label>

          <label className="grid gap-1">
            <span className="text-xs font-medium text-neutral-700">Forecast periods</span>
            <select
              value={String(forecastPeriods)}
              onChange={(event) => updateForecastPeriods(Number(event.target.value))}
              className="rounded-xl border border-black/10 bg-white/70 px-3 py-2 text-sm outline-none transition focus:border-black/20 focus:ring-2 focus:ring-black/5"
            >
              <option value="3">3</option>
              <option value="6">6</option>
              <option value="12">12</option>
            </select>
          </label>
        </div>
      </div>

      <div className="mt-3 flex flex-wrap gap-x-6 gap-y-2 text-xs text-neutral-500">
        <span>{isPending ? "Updating chart..." : "Updates automatically"}</span>
        <span>Comparisons: {compareYears.length + (normalizeMonthsInput(compareMonthsRaw) ? normalizeMonthsInput(compareMonthsRaw).split(",").length : 0)}</span>
        <span>{performanceMetric === "revenue" ? "Forecast uses previous years when available." : "Forecast is available on Revenue only."}</span>
      </div>
    </div>
  );
}
