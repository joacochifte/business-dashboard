import { apiFetchClient, apiJsonServer } from "@/lib/api";

export type ProductDto = {
  id: string;
  name: string;
  description?: string | null;
  price: number;
  stock: number | null;
  isActive: boolean;
};

export type ProductCreationDto = {
  name: string;
  price: number;
  initialStock: number | null;
  description: string | null;
};

export type ProductUpdateDto = {
  id: string;
  name: string;
  description: string | null;
  price: number;
  stock: number | null;
  isActive: boolean;
};

// Server (pages / server components)
export async function getProducts(): Promise<ProductDto[]> {
  return apiJsonServer<ProductDto[]>("/products", { cache: "no-store" });
}

export async function getProductById(id: string): Promise<ProductDto> {
  return apiJsonServer<ProductDto>(`/products/${id}`, { cache: "no-store" });
}

// Client (forms / buttons)
export async function createProduct(payload: ProductCreationDto): Promise<void> {
  await apiFetchClient("/products", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}

export async function updateProduct(id: string, payload: ProductUpdateDto): Promise<void> {
  await apiFetchClient(`/products/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
}

export async function deleteProduct(id: string): Promise<void> {
  await apiFetchClient(`/products/${id}`, { method: "DELETE" });
}

