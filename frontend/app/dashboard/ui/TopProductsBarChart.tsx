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
};

function formatMoney(v: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(v);
}

export default function TopProductsBarChart({ data }: Props) {
  const chartData = data.map((p) => ({
    name: p.productName?.trim() ? p.productName : p.productId,
    revenue: p.revenue,
    quantity: p.quantity,
  }));

  if (chartData.length === 0) {
    return (
      <div className="rounded-lg border border-neutral-200 bg-neutral-50 px-3 py-8 text-center text-sm text-neutral-600">
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
            dataKey="name"
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
            formatter={(value, name, props) => {
              if (name === "revenue") return [formatMoney(Number(value)), "Revenue"];
              return [String(value), String(name)];
            }}
            labelFormatter={(label) => String(label)}
            contentStyle={{
              borderRadius: 12,
              borderColor: "#e5e7eb",
              boxShadow: "0 10px 25px rgba(0,0,0,0.08)",
            }}
          />
          <Bar dataKey="revenue" radius={[10, 10, 6, 6]}>
            {chartData.map((_, idx) => (
              <Cell key={idx} fill="rgba(0,0,0,0.82)" />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}

