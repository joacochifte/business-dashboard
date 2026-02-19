import { apiFetchClient, apiJsonServer } from "@/lib/api";

export type CustomerDto = {
  id: string;
  name: string;
  email?: string | null;
  phone?: string | null;
  birthDate?: string | null;
  isActive: boolean;
  lastPurchaseDate: string;
  totalPurchases: number;
  totalLifetimeValue: number;
};

export type CustomerCreationDto = {
  name: string;
  email?: string | null;
  phone?: string | null;
  birthDate?: string | null;
};

export type CustomerUpdateDto = {
  id: string;
  name: string;
  email?: string | null;
  phone?: string | null;
  birthDate?: string | null;
  isActive: boolean;
};

// Server (pages / server components)
export async function getCustomers(): Promise<CustomerDto[]> {
  return apiJsonServer<CustomerDto[]>("/customers", { cache: "no-store" });
}

export async function getCustomerById(id: string): Promise<CustomerDto> {
  return apiJsonServer<CustomerDto>(`/customers/${id}`, { cache: "no-store" });
}

// Client (forms / buttons)
export async function createCustomer(payload: CustomerCreationDto): Promise<void> {
  await apiFetchClient("/customers", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}

export async function updateCustomer(id: string, payload: CustomerUpdateDto): Promise<void> {
  await apiFetchClient(`/customers/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}

export async function deleteCustomer(id: string): Promise<void> {
  await apiFetchClient(`/customers/${id}`, { method: "DELETE" });
}
