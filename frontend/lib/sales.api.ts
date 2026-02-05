import { apiFetchClient, apiJsonServer } from "@/lib/api";
import type { IsoDateTime } from "@/lib/api";

export type SaleItemDto = {
  productId: string;
  unitPrice: number;
  quantity: number;
};

export type SaleDto = {
  id: string;
  items: SaleItemDto[];
  customerName?: string | null;
  paymentMethod?: string | null;
  total: number;
  createdAt: IsoDateTime;
};

export type SaleCreationDto = {
  id?: string;
  items: SaleItemDto[];
  total: number;
  customerName?: string | null;
  paymentMethod?: string | null;
};

export type SaleUpdateDto = {
  id?: string;
  items: SaleItemDto[];
  total: number;
  customerName?: string | null;
  paymentMethod?: string | null;
};

// Server (pages / server components)
export async function getSales(): Promise<SaleDto[]> {
  return apiJsonServer<SaleDto[]>("/sales", { cache: "no-store" });
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
