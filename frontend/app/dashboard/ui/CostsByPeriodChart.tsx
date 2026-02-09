"use client";

import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

type CostPoint = {
  periodStart: string;
  amount: number;
};

type Props = {
  points: CostPoint[];
  groupBy: "day" | "month";
};

function formatMoney(v: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(v);
}

function toLabel(periodStart: string, groupBy: "day" | "month") {
  if (groupBy === "month") return periodStart;

  const d = new Date(periodStart);
  if (Number.isNaN(d.getTime())) return periodStart;
  return new Intl.DateTimeFormat(undefined, { month: "short", day: "2-digit" }).format(d);
}

export default function CostsByPeriodChart({ points, groupBy }: Props) {
  const chartData = points.map((p) => ({
    label: toLabel(p.periodStart, groupBy),
    amount: p.amount,
  }));

  if (chartData.length === 0) {
    return (
      <div className="rounded-2xl border border-black/10 bg-white/50 px-3 py-8 text-center text-sm text-neutral-600 shadow-sm backdrop-blur">
        No data yet.
      </div>
    );
  }

  return (
    <div className="h-72 w-full">
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={chartData} margin={{ top: 8, right: 16, left: 0, bottom: 8 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
          <XAxis
            dataKey="label"
            tickLine={false}
            axisLine={false}
            interval={0}
            angle={-18}
            textAnchor="end"
            height={56}
            tick={{ fill: "#404040", fontSize: 12 }}
          />
          <YAxis
            tickLine={false}
            axisLine={false}
            tick={{ fill: "#404040", fontSize: 12 }}
            tickFormatter={(v) => (typeof v === "number" ? formatMoney(v) : String(v))}
            width={86}
          />
          <Tooltip
            cursor={{ fill: "rgba(0,0,0,0.04)" }}
            formatter={(value) => [formatMoney(Number(value)), "Costs"]}
            labelFormatter={(label) => String(label)}
            contentStyle={{
              borderRadius: 12,
              borderColor: "#e5e7eb",
              boxShadow: "0 10px 25px rgba(0,0,0,0.08)",
            }}
          />
          <Bar dataKey="amount" radius={[10, 10, 6, 6]}>
            {chartData.map((_, idx) => (
              <Cell key={idx} fill="rgba(20,20,20,0.72)" />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}

