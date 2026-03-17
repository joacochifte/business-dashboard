"use client";

import { useTransition } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";

import type { PerformanceMetric } from "./PerformanceSeriesChart";

type Props = {
  performanceMetric: PerformanceMetric;
  compareYears: number[];
  includeForecast: boolean;
  comparisonRangeEnabled: boolean;
};

const METRIC_OPTIONS: Array<{ value: PerformanceMetric; label: string }> = [
  { value: "revenue", label: "Revenue" },
  { value: "marginPct", label: "Margin %" },
  { value: "gains", label: "Gains" },
  { value: "avgTicket", label: "Avg ticket" },
];

const YEAR_OPTIONS = [1, 2, 3];

export default function PerformanceSeriesControls({
  performanceMetric,
  compareYears,
  includeForecast,
  comparisonRangeEnabled,
}: Props) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [, startTransition] = useTransition();

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

  function toggleForecast(nextValue: boolean) {
    replaceParams((params) => {
      if (nextValue) {
        params.set("includeForecast", "1");
      } else {
        params.delete("includeForecast");
      }
    });
  }

  return (
    <div className="mt-5 rounded-[26px] border border-black/10 bg-white/82 p-4 shadow-sm">
      <div className="space-y-4">
        <div className="space-y-2">
          <div className="text-[11px] font-semibold uppercase tracking-[0.18em] text-neutral-500">Metric</div>
          <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
            {METRIC_OPTIONS.map((option) => {
              const active = option.value === performanceMetric;
              return (
                <button
                  key={option.value}
                  type="button"
                  onClick={() => updateMetric(option.value)}
                  aria-pressed={active}
                  className={`inline-flex w-full items-center justify-center rounded-[18px] px-3.5 py-2 text-sm font-medium transition ${
                    active
                      ? "bg-neutral-950 text-white shadow-[0_10px_24px_rgba(15,23,42,0.18)]"
                      : "border border-black/10 bg-white/80 text-neutral-800 hover:bg-white"
                  }`}
                >
                  {option.label}
                </button>
              );
            })}
          </div>
        </div>

        <div className="grid gap-4 sm:grid-cols-[minmax(0,1fr)_auto] sm:items-end">
          <div className="space-y-2">
            <div className="text-[11px] font-semibold uppercase tracking-[0.18em] text-neutral-500">Compare years</div>
            <div className="grid grid-cols-3 gap-2">
              {YEAR_OPTIONS.map((yearOffset) => {
                const active = compareYears.includes(yearOffset);
                return (
                  <button
                    key={yearOffset}
                    type="button"
                    onClick={() => toggleCompareYear(yearOffset)}
                    disabled={!comparisonRangeEnabled}
                    aria-pressed={active}
                    className={`inline-flex w-full items-center justify-center rounded-[18px] px-3.5 py-2 text-sm font-medium transition ${
                      active
                        ? "bg-gradient-to-r from-blue-600 to-cyan-600 text-white shadow-[0_10px_24px_rgba(37,99,235,0.22)]"
                        : "border border-black/10 bg-white/80 text-neutral-800 hover:bg-white"
                    } disabled:cursor-not-allowed disabled:border-black/5 disabled:bg-white/35 disabled:text-neutral-400`}
                  >
                    {yearOffset}y ago
                  </button>
                );
              })}
            </div>
          </div>

          <div className="flex sm:justify-end">
            <label
              className={`inline-flex min-w-[150px] items-center justify-between rounded-full border px-4 py-2.5 shadow-sm transition ${
                performanceMetric === "revenue" && comparisonRangeEnabled
                  ? "cursor-pointer border-black/10 bg-white/90"
                  : "cursor-not-allowed border-black/5 bg-white/40"
              }`}
            >
              <span
                className="text-sm font-medium text-neutral-800"
              >
                Forecast
              </span>
              <span
                className={`relative inline-flex h-6 w-11 rounded-full transition ${
                  includeForecast && performanceMetric === "revenue" ? "bg-neutral-950" : "bg-neutral-300"
                }`}
              >
                <span
                  className={`absolute top-0.5 h-5 w-5 rounded-full bg-white shadow-sm transition-transform ${
                    includeForecast && performanceMetric === "revenue" ? "translate-x-[22px]" : "translate-x-0.5"
                  }`}
                />
              </span>
              <input
                type="checkbox"
                checked={includeForecast}
                onChange={(event) => toggleForecast(event.target.checked)}
                disabled={performanceMetric !== "revenue" || !comparisonRangeEnabled}
                className="sr-only"
              />
            </label>
          </div>
        </div>
      </div>
    </div>
  );
}
