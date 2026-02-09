import { apiJsonServer } from "@/lib/api";
import type { IsoDateTime } from "@/lib/api";

export type DashboardSummaryDto = {
  revenueTotal: number;
  gains: number;
  salesCount: number;
  avgTicket: number;
};

export type SalesByPeriodPointDto = {
  periodStart: IsoDateTime;
  revenue: number;
  salesCount: number;
};

export type SalesByPeriodDto = {
  groupBy: "day" | "week" | "month" | string;
  points: SalesByPeriodPointDto[];
};

export type TopProductDto = {
  productId: string;
  productName: string;
  quantity: number;
  revenue: number;
};

function toQuery(params: Record<string, string | number | boolean | null | undefined>) {
  const sp = new URLSearchParams();
  for (const [k, v] of Object.entries(params)) {
    if (v === null || v === undefined || v === "") continue;
    sp.set(k, String(v));
  }
  const qs = sp.toString();
  return qs ? `?${qs}` : "";
}

// Server (pages / server components)
export async function getDashboardSummary(opts?: { from?: IsoDateTime; to?: IsoDateTime }): Promise<DashboardSummaryDto> {
  const qs = toQuery({ from: opts?.from, to: opts?.to });
  return apiJsonServer<DashboardSummaryDto>(`/dashboard/summary${qs}`, { cache: "no-store" });
}

export async function getSalesByPeriod(opts?: {
  groupBy?: "day" | "week" | "month";
  from?: IsoDateTime;
  to?: IsoDateTime;
}): Promise<SalesByPeriodDto> {
  const qs = toQuery({ groupBy: opts?.groupBy ?? "day", from: opts?.from, to: opts?.to });
  return apiJsonServer<SalesByPeriodDto>(`/dashboard/sales-by-period${qs}`, { cache: "no-store" });
}

export async function getTopProducts(opts?: {
  limit?: number;
  from?: IsoDateTime;
  to?: IsoDateTime;
}): Promise<TopProductDto[]> {
  const qs = toQuery({ limit: opts?.limit ?? 10, from: opts?.from, to: opts?.to });
  return apiJsonServer<TopProductDto[]>(`/dashboard/top-products${qs}`, { cache: "no-store" });
}
