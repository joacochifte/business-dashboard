import { apiFetchClient, apiJsonServer } from "@/lib/api";
import type { IsoDateTime } from "@/lib/api";

export type NotificationDto = {
  id: string;
  title: string;
  date: IsoDateTime;
  isSeen: boolean;
  customerId?: string | null;
};

// Server
export async function getUnseenNotifications(): Promise<NotificationDto[]> {
  return apiJsonServer<NotificationDto[]>("/notifications/unseen", { cache: "no-store" });
}

// Client
export async function fetchUnseenCount(): Promise<number> {
  const res = await fetch(
    `${process.env.NEXT_PUBLIC_API_BASE_URL}/notifications/unseen`,
    { cache: "no-store" }
  );
  if (!res.ok) return 0;
  const data: NotificationDto[] = await res.json();
  return data.length;
}

export async function markAllAsSeen(): Promise<void> {
  await apiFetchClient("/notifications/seen-all", { method: "PATCH" });
}

export async function markAsSeen(id: string): Promise<void> {
  await apiFetchClient(`/notifications/${id}/seen`, { method: "PATCH" });
}
