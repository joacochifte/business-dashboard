import { apiFetchClient, apiJsonServer } from "@/lib/api";
import type { IsoDateTime } from "@/lib/api";

export type InventoryMovementItemDto = {
  id: string;
  productId: string;
  type: string; // "In" | "Out" | "Adjust" (from backend enum name)
  reason: string; // "Sale" | "Purchase" | "Adjustment" (from backend enum name)
  quantity: number;
  createdAt: IsoDateTime;
};

export type InventoryMovementsPageDto = {
  page: number;
  pageSize: number;
  total: number;
  items: InventoryMovementItemDto[];
};

export type InventoryAdjustDto = {
  productId: string;
  delta: number;
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
export async function getInventoryMovements(opts?: {
  productId?: string;
  from?: IsoDateTime;
  to?: IsoDateTime;
  page?: number;
  pageSize?: number;
}): Promise<InventoryMovementsPageDto> {
  const qs = toQuery({
    productId: opts?.productId,
    from: opts?.from,
    to: opts?.to,
    page: opts?.page ?? 1,
    pageSize: opts?.pageSize ?? 50,
  });

  return apiJsonServer<InventoryMovementsPageDto>(`/inventory/movements${qs}`, { cache: "no-store" });
}

// Client (forms / buttons)
export async function adjustInventory(payload: InventoryAdjustDto): Promise<void> {
  await apiFetchClient("/inventory/adjust", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}

