import { apiFetchClient, apiJsonServer } from "@/lib/api";
import type { IsoDateTime } from "@/lib/api";

export type SaleDto = {
  id: string;
  items: SaleItemDto[];
  customerId?: string | null;
  customerName?: string | null;
  paymentMethod?: string | null;
  isDebt: boolean;
  total: number;
  createdAt: IsoDateTime;
};

export type SaleItemDto = {
  productId: string;
  unitPrice: number;
  quantity: number;
  specialPrice?: number | null;
};

export type SaleCreationDto = {
  items: SaleItemDto[];
  total: number;
  customerId?: string | null;
  paymentMethod?: string | null;
  isDebt: boolean;
};

export type SaleUpdateDto = {
  id: string;
  items: SaleItemDto[];
  total: number;
  customerId?: string | null;
  paymentMethod?: string | null;
  isDebt: boolean;
};

// Server (pages / server components)
export async function getSales(): Promise<SaleDto[]> {
  return apiJsonServer<SaleDto[]>("/sales", { cache: "no-store" });
}

export async function getSalesByDebt(isDebt: boolean): Promise<SaleDto[]> {
  return apiJsonServer<SaleDto[]>(`/sales?isDebt=${isDebt}`, { cache: "no-store" });
}

export async function getSaleById(id: string): Promise<SaleDto> {
  return apiJsonServer<SaleDto>(`/sales/${id}`, { cache: "no-store" });
}

// Client (forms / buttons)
export async function createSale(payload: SaleCreationDto): Promise<void> {
  await apiFetchClient("/sales", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}

export async function updateSale(id: string, payload: SaleUpdateDto): Promise<void> {
  await apiFetchClient(`/sales/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}

export async function deleteSale(id: string): Promise<void> {
  await apiFetchClient(`/sales/${id}`, { method: "DELETE" });
}
