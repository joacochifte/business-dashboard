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

const COMPARISON_COLORS = ["#2563eb", "#d97706", "#0f766e", "#dc2626", "#7c3aed", "#0891b2"];

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

function isAbsoluteAxisLabel(label: string) {
  return /^\d{4}-\d{2}(-\d{2})?$/.test(label);
}

function formatTooltipLabel(label: string | number, axisMode: string) {
  const normalized = String(label);
  return isAbsoluteAxisLabel(normalized) ? normalized : `${axisLabelPrefix(axisMode)} ${normalized}`;
}

function getForecastStartAxisIndex(series: DashboardPerformanceSeriesDto) {
  return series.forecastSeries?.points[0]?.axisIndex;
}

function getCurrentPeriodCutoffAxisIndex(series: DashboardPerformanceSeriesDto) {
  const now = new Date();
  const points = series.currentSeries.points;

  if (points.length === 0) {
    return undefined;
  }

  const hasFuturePoints = points.some((point) => point.periodStart && new Date(point.periodStart) > now);
  if (!hasFuturePoints) {
    return undefined;
  }

  const currentPoint = [...points].reverse().find((point) => point.periodStart && new Date(point.periodStart) <= now);
  return currentPoint?.axisIndex ?? points[0]?.axisIndex;
}

function trimCurrentPoints(series: DashboardPerformanceSeriesDto, metric: PerformanceMetric) {
  const forecastStartAxisIndex = metric === "revenue" ? getForecastStartAxisIndex(series) : undefined;
  const currentPeriodCutoffAxisIndex = getCurrentPeriodCutoffAxisIndex(series);

  if (forecastStartAxisIndex === undefined) {
    if (currentPeriodCutoffAxisIndex === undefined) {
      return series.currentSeries.points;
    }

    return series.currentSeries.points.filter((point) => point.axisIndex < currentPeriodCutoffAxisIndex);
  }

  return series.currentSeries.points.filter((point) => point.axisIndex < forecastStartAxisIndex);
}

function getForecastAnchorPoint(series: DashboardPerformanceSeriesDto) {
  const forecastStartAxisIndex = getForecastStartAxisIndex(series);

  if (forecastStartAxisIndex === undefined) {
    return null;
  }

  const currentPoints = trimCurrentPoints(series, "revenue");
  return currentPoints[currentPoints.length - 1] ?? null;
}

function buildVisibleLines(series: DashboardPerformanceSeriesDto, metric: PerformanceMetric) {
  const currentSeries = {
    ...series.currentSeries,
    points: trimCurrentPoints(series, metric),
  };

  const lines: DashboardPerformanceSeriesLineDto[] = [currentSeries, ...series.comparisonSeries];

  if (metric === "revenue" && series.forecastSeries && series.forecastSeries.points.length > 0) {
    const forecastAnchorPoint = getForecastAnchorPoint(series);
    lines.push({
      ...series.forecastSeries,
      points: forecastAnchorPoint ? [forecastAnchorPoint, ...series.forecastSeries.points] : series.forecastSeries.points,
    });
  }

  return lines;
}

function buildChartRows(
  basePoints: DashboardPerformancePointDto[],
  lines: DashboardPerformanceSeriesLineDto[],
  metric: PerformanceMetric,
) {
  const rows = new Map<number, ChartRow>();

  for (const point of basePoints) {
    rows.set(point.axisIndex, {
      axisIndex: point.axisIndex,
      axisLabel: point.axisLabel,
    });
  }

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
  if (line.kind === "forecast") return "#b45309";
  return COMPARISON_COLORS[comparisonIndex % COMPARISON_COLORS.length];
}

export default function PerformanceSeriesChart({ series, metric }: Props) {
  const visibleLines = buildVisibleLines(series, metric);
  const chartData = buildChartRows(series.currentSeries.points, visibleLines, metric);

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
          <CartesianGrid strokeDasharray="4 6" stroke="#e7dfd3" />
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
            labelFormatter={(label) => formatTooltipLabel(label, series.axisMode)}
            contentStyle={{
              borderRadius: 18,
              borderColor: "rgba(15,23,42,0.08)",
              backgroundColor: "rgba(255,251,244,0.96)",
              boxShadow: "0 18px 40px rgba(41,24,9,0.12)",
            }}
          />
          <Legend wrapperStyle={{ fontSize: 12, paddingTop: 8 }} />
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
                strokeWidth={line.kind === "current" ? 3.25 : 2.4}
                strokeDasharray={line.kind === "forecast" ? "8 6" : undefined}
                dot={false}
                activeDot={{ r: line.kind === "current" ? 5 : 4, strokeWidth: 0, fill: stroke }}
                connectNulls={false}
                strokeLinecap="round"
                strokeLinejoin="round"
              />
            );
          })}
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
