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
  "#9a3412",
  "#2563eb",
  "#0f766e",
  "#7c2d12",
  "#7c3aed",
  "#0ea5e9",
  "#ca8a04",
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
          <CartesianGrid strokeDasharray="4 6" stroke="#e7dfd3" />
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
            cursor={{ fill: "rgba(154,52,18,0.06)" }}
            formatter={(value, name) => {
              if (name === "revenue") return [formatMoney(Number(value)), "Revenue"];
              if (name === "quantity") return [String(value), "Quantity"];
              return [String(value), String(name)];
            }}
            labelFormatter={(_, payload) => String(payload?.[0]?.payload?.name ?? "")}
            contentStyle={{
              borderRadius: 18,
              borderColor: "rgba(15,23,42,0.08)",
              backgroundColor: "rgba(255,251,244,0.96)",
              boxShadow: "0 18px 40px rgba(41,24,9,0.12)",
            }}
          />
          <Bar dataKey={sortBy} radius={[0, 12, 12, 0]} barSize={24}>
            {chartData.map((_, idx) => (
              <Cell key={idx} fill={BAR_COLORS[idx % BAR_COLORS.length]} />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
