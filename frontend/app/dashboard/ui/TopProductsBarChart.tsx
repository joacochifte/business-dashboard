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

import type { TopProductDto } from "@/lib/dashboard.api";

type Props = {
  data: TopProductDto[];
  sortBy?: "revenue" | "quantity";
};

function formatMoney(v: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(v);
}

const BAR_COLORS = [
  "#111827",
  "#2563eb",
  "#d97706",
  "#059669",
  "#dc2626",
  "#7c3aed",
  "#0891b2",
  "#ea580c",
  "#4f46e5",
  "#65a30d",
];

function shortenLabel(value: string, max = 18) {
  const trimmed = value.trim();
  return trimmed.length > max ? `${trimmed.slice(0, max - 1)}…` : trimmed;
}

export default function TopProductsBarChart({ data, sortBy = "revenue" }: Props) {
  const chartData = data.map((p) => ({
    name: p.productName?.trim() ? p.productName : p.productId,
    shortName: shortenLabel(p.productName?.trim() ? p.productName : p.productId),
    revenue: p.revenue,
    quantity: p.quantity,
  }));

  if (chartData.length === 0) {
    return (
      <div className="rounded-2xl border border-black/10 bg-white/50 px-3 py-8 text-center text-sm text-neutral-600 shadow-sm backdrop-blur">
        No data yet.
      </div>
    );
  }

  const chartHeight = Math.max(320, chartData.length * 44);

  return (
    <div className="w-full" style={{ height: chartHeight }}>
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={chartData} layout="vertical" margin={{ top: 8, right: 20, left: 12, bottom: 8 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
          <XAxis
            type="number"
            tickLine={false}
            axisLine={false}
            tick={{ fill: "#404040", fontSize: 12 }}
            tickFormatter={(v) => (typeof v === "number" && sortBy === "revenue" ? formatMoney(v) : String(v))}
          />
          <YAxis
            type="category"
            dataKey="shortName"
            tickLine={false}
            axisLine={false}
            interval={0}
            tick={{ fill: "#404040", fontSize: 12 }}
            width={140}
          />
          <Tooltip
            cursor={{ fill: "rgba(0,0,0,0.04)" }}
            formatter={(value, name) => {
              if (name === "revenue") return [formatMoney(Number(value)), "Revenue"];
              if (name === "quantity") return [String(value), "Quantity"];
              return [String(value), String(name)];
            }}
            labelFormatter={(_, payload) => String(payload?.[0]?.payload?.name ?? "")}
            contentStyle={{
              borderRadius: 12,
              borderColor: "#e5e7eb",
              boxShadow: "0 10px 25px rgba(0,0,0,0.08)",
            }}
          />
          <Bar dataKey={sortBy} radius={[0, 10, 10, 0]} barSize={24}>
            {chartData.map((_, idx) => (
              <Cell key={idx} fill={BAR_COLORS[idx % BAR_COLORS.length]} />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
