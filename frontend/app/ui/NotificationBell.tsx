"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { fetchUnseenCount, markAllAsSeen, markAsSeen, type NotificationDto } from "@/lib/notifications.api";

export default function NotificationBell() {
  const [count, setCount] = useState(0);
  const [open, setOpen] = useState(false);
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    fetchUnseenCount().then(setCount).catch(() => setCount(0));

    const interval = setInterval(() => {
      fetchUnseenCount().then(setCount).catch(() => null);
    }, 60_000);

    return () => clearInterval(interval);
  }, []);

  async function onOpen() {
    if (open) { setOpen(false); return; }
    setOpen(true);
    setLoading(true);
    try {
      const res = await fetch(`${process.env.NEXT_PUBLIC_API_BASE_URL}/notifications/unseen`, { cache: "no-store" });
      const data: NotificationDto[] = res.ok ? await res.json() : [];
      setNotifications(data);
      setCount(data.length);
    } catch {
      setNotifications([]);
    } finally {
      setLoading(false);
    }
  }

  async function onDismiss(id: string) {
    await markAsSeen(id).catch(() => null);
    setNotifications((prev) => prev.filter((n) => n.id !== id));
    setCount((c) => Math.max(0, c - 1));
  }

  async function onMarkAllSeen() {
    await markAllAsSeen().catch(() => null);
    setNotifications([]);
    setCount(0);
    setOpen(false);
  }

  return (
    <div className="relative">
      <button
        type="button"
        onClick={onOpen}
        className="relative rounded-xl border border-black/10 bg-white/60 p-2.5 shadow-sm backdrop-blur transition hover:bg-white/80"
        aria-label="Notifications"
      >
        <svg
          xmlns="http://www.w3.org/2000/svg"
          className="h-4 w-4 text-neutral-800"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
          <path d="M13.73 21a2 2 0 0 1-3.46 0" />
        </svg>
        {count > 0 && (
          <span className="absolute -right-1 -top-1 flex h-4 w-4 items-center justify-center rounded-full bg-rose-500 text-[10px] font-bold text-white">
            {count > 9 ? "9+" : count}
          </span>
        )}
      </button>

      {open && (
        <div className="absolute right-0 top-full z-50 mt-2 w-80 rounded-2xl border border-black/10 bg-white/90 shadow-lg backdrop-blur">
          <div className="flex items-center justify-between border-b border-black/10 px-4 py-3">
            <span className="text-sm font-semibold text-neutral-900">Notifications</span>
            {notifications.length > 0 && (
              <button type="button" onClick={onMarkAllSeen} className="text-xs text-neutral-600 hover:underline">
                Mark all as seen
              </button>
            )}
          </div>

          <div className="max-h-72 overflow-y-auto">
            {loading ? (
              <p className="px-4 py-6 text-center text-xs text-neutral-500">Loading...</p>
            ) : notifications.length === 0 ? (
              <p className="px-4 py-6 text-center text-xs text-neutral-500">No new notifications</p>
            ) : (
              notifications.map((n) => (
                <div key={n.id} className="flex items-start gap-2 border-b border-black/5 px-4 py-3 last:border-0">
                  <div className="flex-1 min-w-0">
                    {n.customerId ? (
                      <Link
                        href={`/customers/${n.customerId}/edit`}
                        onClick={() => setOpen(false)}
                        className="text-sm text-neutral-900 hover:underline"
                      >
                        {n.title}
                      </Link>
                    ) : (
                      <p className="text-sm text-neutral-900">{n.title}</p>
                    )}
                    <p className="mt-0.5 text-xs text-neutral-500">
                      {new Date(n.date).toLocaleDateString()}
                    </p>
                  </div>
                  <button
                    type="button"
                    onClick={() => onDismiss(n.id)}
                    className="mt-0.5 shrink-0 rounded-lg p-1 text-neutral-400 transition hover:bg-black/5 hover:text-neutral-700"
                    title="Mark as seen"
                  >
                    ✕
                  </button>
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
}
