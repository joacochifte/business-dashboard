import { apiJsonServer } from "@/lib/api";
import type { IsoDateTime } from "@/lib/api";

export type DashboardSummaryDto = {
  revenueTotal: number;
  gains: number;
  salesCount: number;
  avgTicket: number;
};

export type DashboardComparisonDto = {
  revenueDeltaPct: number | null;
  costsDeltaPct: number | null;
  gainsDeltaPct: number | null;
  salesCountDeltaPct: number | null;
  unitsSoldDeltaPct: number | null;
};

export type DashboardAlertDto = {
  kind: string;
  title: string;
  detail: string;
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

export type CustomerSalesDto = {
  customerId: string;
  customerName: string;
  salesCount: number;
};

export type CustomerSpendingDto = {
  customerId: string;
  customerName: string;
  totalSpent: number;
};

export type DashboardPerformancePointDto = {
  axisIndex: number;
  axisLabel: string;
  periodStart: IsoDateTime | null;
  revenue: number;
  costs: number;
  gains: number;
  marginPct: number;
  avgTicket: number;
  salesCount: number;
};

export type DashboardPerformanceSeriesLineDto = {
  id: string;
  label: string;
  kind: string;
  points: DashboardPerformancePointDto[];
};

export type ForecastModelKey = "historical_average" | "year_regression";

export type DashboardPerformanceSeriesDto = {
  groupBy: string;
  axisMode: string;
  currentSeries: DashboardPerformanceSeriesLineDto;
  comparisonSeries: DashboardPerformanceSeriesLineDto[];
  forecastSeries: DashboardPerformanceSeriesLineDto | null;
};

export type DashboardOverviewDto = {
  revenueTotal: number;
  costsTotal: number;
  gains: number;
  marginPct: number;
  salesCount: number;
  unitsSold: number;
  avgTicket: number;
  debtsTotal: number;
  debtRatioPct: number;
  lowStockCount: number;
  outOfStockCount: number;
  topCustomer: CustomerSpendingDto | null;
  topProductByQuantity: TopProductDto | null;
  comparison: DashboardComparisonDto;
  alerts: DashboardAlertDto[];
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

export async function getDashboardOverview(opts?: { from?: IsoDateTime; to?: IsoDateTime }): Promise<DashboardOverviewDto> {
  const qs = toQuery({ from: opts?.from, to: opts?.to });
  return apiJsonServer<DashboardOverviewDto>(`/dashboard/overview${qs}`, { cache: "no-store" });
}

export async function getPerformanceSeries(opts?: {
  groupBy?: "day" | "month";
  from?: IsoDateTime;
  to?: IsoDateTime;
  tzOffsetMinutes?: number;
  compareYears?: number[];
  forecastModel?: ForecastModelKey;
  includeForecast?: boolean;
}): Promise<DashboardPerformanceSeriesDto> {
  const params: Record<string, string | number | boolean | null | undefined> = {
    groupBy: opts?.groupBy ?? "day",
    from: opts?.from,
    to: opts?.to,
    tzOffsetMinutes: opts?.tzOffsetMinutes ?? 0,
    compareYears: opts?.compareYears?.length ? opts.compareYears.join(",") : undefined,
    forecastModel: opts?.forecastModel,
    includeForecast: opts?.includeForecast ?? false,
  };

  const qs = toQuery(params);
  return apiJsonServer<DashboardPerformanceSeriesDto>(`/dashboard/performance-series${qs}`, { cache: "no-store" });
}

export async function getSalesByPeriod(opts?: {
  groupBy?: "day" | "week" | "month";
  from?: IsoDateTime;
  to?: IsoDateTime;
  tzOffsetMinutes?: number;
}): Promise<SalesByPeriodDto> {
  const qs = toQuery({ groupBy: opts?.groupBy ?? "day", from: opts?.from, to: opts?.to, tzOffsetMinutes: opts?.tzOffsetMinutes ?? 0 });
  return apiJsonServer<SalesByPeriodDto>(`/dashboard/sales-by-period${qs}`, { cache: "no-store" });
}

export async function getTopProducts(opts?: {
  limit?: number;
  from?: IsoDateTime;
  to?: IsoDateTime;
  sortBy?: "revenue" | "quantity";
}): Promise<TopProductDto[]> {
  const qs = toQuery({ limit: opts?.limit ?? 10, from: opts?.from, to: opts?.to, sortBy: opts?.sortBy ?? "revenue" });
  return apiJsonServer<TopProductDto[]>(`/dashboard/top-products${qs}`, { cache: "no-store" });
}

export async function getSalesByCustomer(opts?: {
  limit?: number;
  from?: IsoDateTime;
  to?: IsoDateTime;
  excludeDebts?: boolean;
}): Promise<CustomerSalesDto[]> {
  const qs = toQuery({ limit: opts?.limit ?? 10, from: opts?.from, to: opts?.to, excludeDebts: opts?.excludeDebts ?? true });
  return apiJsonServer<CustomerSalesDto[]>(`/dashboard/sales-by-customer${qs}`, { cache: "no-store" });
}

export async function getSpendingByCustomer(opts?: {
  limit?: number;
  from?: IsoDateTime;
  to?: IsoDateTime;
  excludeDebts?: boolean;
}): Promise<CustomerSpendingDto[]> {
  const qs = toQuery({ limit: opts?.limit ?? 10, from: opts?.from, to: opts?.to, excludeDebts: opts?.excludeDebts ?? true });
  return apiJsonServer<CustomerSpendingDto[]>(`/dashboard/spending-by-customer${qs}`, { cache: "no-store" });
}
