import { apiFetchClient, apiJsonServer } from "@/lib/api";
import type { IsoDateTime } from "@/lib/api";

export type CostSummaryDto = {
  id: string;
  name: string;
  amount: number;
  dateIncurred: IsoDateTime;
};

export type CostDto = {
  id: string;
  name: string;
  amount: number;
  dateIncurred: IsoDateTime;
  description?: string | null;
};

export type CostCreationDto = {
  name: string;
  amount: number;
  dateIncurred: IsoDateTime;
  description?: string | null;
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
export async function getCosts(opts?: {
  startDate?: IsoDateTime;
  endDate?: IsoDateTime;
}): Promise<CostSummaryDto[]> {
  const qs = toQuery({ startDate: opts?.startDate, endDate: opts?.endDate });
  return apiJsonServer<CostSummaryDto[]>(`/costs${qs}`, { cache: "no-store" });
}

export async function getCostById(id: string): Promise<CostDto> {
  return apiJsonServer<CostDto>(`/costs/${id}`, { cache: "no-store" });
}

// Client (forms / buttons)
export async function createCost(payload: CostCreationDto): Promise<void> {
  await apiFetchClient("/costs", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}

export async function updateCost(id: string, payload: CostCreationDto): Promise<void> {
  await apiFetchClient(`/costs/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}

export async function deleteCost(id: string): Promise<void> {
  await apiFetchClient(`/costs/${id}`, { method: "DELETE" });
}

