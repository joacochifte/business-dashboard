"use client";

import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

import type {
  DashboardPerformancePointDto,
  DashboardPerformanceSeriesDto,
  DashboardPerformanceSeriesLineDto,
} from "@/lib/dashboard.api";

export type PerformanceMetric = "revenue" | "marginPct" | "gains" | "avgTicket";

type Props = {
  series: DashboardPerformanceSeriesDto;
  metric: PerformanceMetric;
};

type ChartRow = {
  axisIndex: number;
  axisLabel: string;
  [key: string]: number | string | undefined;
};

const COMPARISON_COLORS = ["#2563eb", "#d97706", "#059669", "#dc2626", "#7c3aed", "#0891b2"];

function formatMoney(value: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(value);
}

function formatMoneyCompact(value: number) {
  const sign = value < 0 ? "-" : "";
  const absValue = Math.abs(value);

  if (absValue >= 1_000_000) {
    return `${sign}$${(absValue / 1_000_000).toFixed(absValue >= 10_000_000 ? 0 : 1)}M`;
  }

  if (absValue >= 1_000) {
    return `${sign}$${(absValue / 1_000).toFixed(absValue >= 10_000 ? 0 : 1)}K`;
  }

  return `${sign}$${absValue.toFixed(0)}`;
}

function formatPercent(value: number) {
  return `${value.toFixed(1)}%`;
}

function metricLabel(metric: PerformanceMetric) {
  switch (metric) {
    case "marginPct":
      return "Margin %";
    case "gains":
      return "Gains";
    case "avgTicket":
      return "Average ticket";
    default:
      return "Revenue";
  }
}

function getMetricValue(point: DashboardPerformancePointDto, metric: PerformanceMetric) {
  switch (metric) {
    case "marginPct":
      return point.marginPct;
    case "gains":
      return point.gains;
    case "avgTicket":
      return point.avgTicket;
    default:
      return point.revenue;
  }
}

function formatMetricValue(value: number, metric: PerformanceMetric) {
  return metric === "marginPct" ? formatPercent(value) : formatMoney(value);
}

function formatAxisTick(value: number, metric: PerformanceMetric) {
  return metric === "marginPct" ? `${value.toFixed(0)}%` : formatMoneyCompact(value);
}

function axisLabelPrefix(axisMode: string) {
  return axisMode === "month_of_period" ? "Month" : "Day";
}

function buildVisibleLines(series: DashboardPerformanceSeriesDto, metric: PerformanceMetric) {
  const lines: DashboardPerformanceSeriesLineDto[] = [series.currentSeries, ...series.comparisonSeries];

  if (metric === "revenue" && series.forecastSeries && series.currentSeries.points.length > 0) {
    const lastCurrentPoint = series.currentSeries.points[series.currentSeries.points.length - 1];
    lines.push({
      ...series.forecastSeries,
      points: [lastCurrentPoint, ...series.forecastSeries.points],
    });
  }

  return lines;
}

function buildChartRows(lines: DashboardPerformanceSeriesLineDto[], metric: PerformanceMetric) {
  const rows = new Map<number, ChartRow>();

  for (const line of lines) {
    for (const point of line.points) {
      const existing = rows.get(point.axisIndex) ?? {
        axisIndex: point.axisIndex,
        axisLabel: point.axisLabel,
      };

      existing.axisLabel = point.axisLabel;
      existing[line.id] = getMetricValue(point, metric);
      rows.set(point.axisIndex, existing);
    }
  }

  return Array.from(rows.values()).sort((a, b) => a.axisIndex - b.axisIndex);
}

function lineStroke(line: DashboardPerformanceSeriesLineDto, comparisonIndex: number) {
  if (line.kind === "current") return "#111827";
  if (line.kind === "forecast") return "rgba(17,24,39,0.4)";
  return COMPARISON_COLORS[comparisonIndex % COMPARISON_COLORS.length];
}

export default function PerformanceSeriesChart({ series, metric }: Props) {
  const visibleLines = buildVisibleLines(series, metric);
  const chartData = buildChartRows(visibleLines, metric);

  if (series.currentSeries.points.length === 0) {
    return (
      <div className="rounded-2xl border border-black/10 bg-white/50 px-3 py-10 text-center text-sm text-neutral-600 shadow-sm backdrop-blur">
        No data yet.
      </div>
    );
  }

  return (
    <div className="h-[24rem] w-full">
      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={chartData} margin={{ top: 12, right: 16, left: 8, bottom: 8 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
          <XAxis
            dataKey="axisLabel"
            tickLine={false}
            axisLine={false}
            minTickGap={18}
            tick={{ fill: "#404040", fontSize: 12 }}
          />
          <YAxis
            tickLine={false}
            axisLine={false}
            width={72}
            tick={{ fill: "#404040", fontSize: 12 }}
            tickFormatter={(value) => (typeof value === "number" ? formatAxisTick(value, metric) : String(value))}
          />
          <Tooltip
            formatter={(value, _name, item) => [formatMetricValue(Number(value), metric), String(item.name ?? metricLabel(metric))]}
            labelFormatter={(label) => `${axisLabelPrefix(series.axisMode)} ${String(label)}`}
            contentStyle={{
              borderRadius: 12,
              borderColor: "#e5e7eb",
              boxShadow: "0 10px 25px rgba(0,0,0,0.08)",
            }}
          />
          <Legend wrapperStyle={{ fontSize: 12 }} />
          {visibleLines.map((line, index) => {
            const comparisonIndex = series.comparisonSeries.findIndex((candidate) => candidate.id === line.id);
            const stroke = lineStroke(line, comparisonIndex < 0 ? index : comparisonIndex);

            return (
              <Line
                key={line.id}
                type="monotone"
                dataKey={line.id}
                name={line.label}
                stroke={stroke}
                strokeWidth={line.kind === "current" ? 3 : 2.25}
                strokeDasharray={line.kind === "forecast" ? "7 5" : undefined}
                dot={false}
                activeDot={{ r: line.kind === "current" ? 5 : 4 }}
                connectNulls={false}
              />
            );
          })}
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
