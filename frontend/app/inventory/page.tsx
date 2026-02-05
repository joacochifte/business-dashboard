import Link from "next/link";

import { getProducts, type ProductDto } from "@/lib/products.api";
import { getInventoryMovements, type InventoryMovementItemDto } from "@/lib/inventory.api";
import { toIsoUtcEndOfDay, toIsoUtcStartOfDay } from "@/lib/api";

type Props = {
  searchParams?: Promise<Record<string, string | string[] | undefined>>;
};

function pickFirst(v: string | string[] | undefined): string | undefined {
  if (Array.isArray(v)) return v[0];
  return v;
}

function clampInt(v: string | undefined, fallback: number, min: number, max: number) {
  const n = v ? Number(v) : NaN;
  if (!Number.isInteger(n)) return fallback;
  return Math.max(min, Math.min(max, n));
}

function toIsoRange(fromDate?: string, toDate?: string) {
  // from/to inputs come as "YYYY-MM-DD". We convert to UTC boundaries to avoid timezone shifts.
  const from =
    fromDate && /^\d{4}-\d{2}-\d{2}$/.test(fromDate) ? toIsoUtcStartOfDay(new Date(`${fromDate}T00:00:00Z`)) : undefined;
  const to =
    toDate && /^\d{4}-\d{2}-\d{2}$/.test(toDate) ? toIsoUtcEndOfDay(new Date(`${toDate}T00:00:00Z`)) : undefined;
  return { from, to };
}

function formatDateTimeUtc(iso: string) {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  return new Intl.DateTimeFormat("en-US", { dateStyle: "medium", timeStyle: "short", timeZone: "UTC" }).format(d);
}

function signedQuantity(m: InventoryMovementItemDto) {
  // Backend stores quantity as positive; "Out" is a negative movement.
  if (m.type.toLowerCase() === "out") return -Math.abs(m.quantity);
  if (m.type.toLowerCase() === "in") return Math.abs(m.quantity);
  return m.quantity;
}

export default async function InventoryPage({ searchParams }: Props) {
  const sp = searchParams ? await searchParams : {};

  const productId = pickFirst(sp.productId);
  const fromDate = pickFirst(sp.from);
  const toDate = pickFirst(sp.to);

  const page = clampInt(pickFirst(sp.page), 1, 1, 10_000);
  const pageSize = clampInt(pickFirst(sp.pageSize), 50, 1, 200);

  const products = await getProducts();
  const productMap = new Map<string, ProductDto>(products.map((p) => [p.id, p]));

  const range = toIsoRange(fromDate, toDate);
  const movementsPage = await getInventoryMovements({
    productId: productId?.trim() ? productId : undefined,
    from: range.from,
    to: range.to,
    page,
    pageSize,
  });

  const totalPages = Math.max(1, Math.ceil(movementsPage.total / movementsPage.pageSize));
  const hasPrev = movementsPage.page > 1;
  const hasNext = movementsPage.page < totalPages;

  const queryBase = new URLSearchParams();
  if (productId) queryBase.set("productId", productId);
  if (fromDate) queryBase.set("from", fromDate);
  if (toDate) queryBase.set("to", toDate);
  if (pageSize !== 50) queryBase.set("pageSize", String(pageSize));

  function pageHref(p: number) {
    const qs = new URLSearchParams(queryBase);
    qs.set("page", String(p));
    return `/inventory?${qs.toString()}`;
  }

  return (
    <main className="p-6 space-y-4">
      <header className="flex items-center justify-between">
        <div className="space-y-1">
          <h1 className="text-2xl font-semibold">Inventory</h1>
          <p className="text-sm text-neutral-600">Movements history (IN / OUT / ADJUST).</p>
        </div>
        <Link
          href="/inventory/adjust"
          className="rounded-md bg-black px-3 py-2 text-sm font-medium text-white hover:bg-black/90"
        >
          Adjust stock
        </Link>
      </header>

      <section className="rounded-xl border border-neutral-200 bg-white p-4">
        <form className="grid gap-3 md:grid-cols-12" method="GET">
          <div className="md:col-span-4">
            <label className="grid gap-1">
              <span className="text-xs font-medium text-neutral-700">Product</span>
              <select
                name="productId"
                defaultValue={productId ?? ""}
                className="rounded-md border border-neutral-300 bg-white px-3 py-2 text-sm outline-none focus:border-neutral-500"
              >
                <option value="">All products</option>
                {products.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.name}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <div className="md:col-span-3">
            <label className="grid gap-1">
              <span className="text-xs font-medium text-neutral-700">From</span>
              <input
                type="date"
                name="from"
                defaultValue={fromDate ?? ""}
                className="rounded-md border border-neutral-300 bg-white px-3 py-2 text-sm outline-none focus:border-neutral-500"
              />
            </label>
          </div>

          <div className="md:col-span-3">
            <label className="grid gap-1">
              <span className="text-xs font-medium text-neutral-700">To</span>
              <input
                type="date"
                name="to"
                defaultValue={toDate ?? ""}
                className="rounded-md border border-neutral-300 bg-white px-3 py-2 text-sm outline-none focus:border-neutral-500"
              />
            </label>
          </div>

          <div className="md:col-span-2">
            <label className="grid gap-1">
              <span className="text-xs font-medium text-neutral-700">Page size</span>
              <select
                name="pageSize"
                defaultValue={String(pageSize)}
                className="rounded-md border border-neutral-300 bg-white px-3 py-2 text-sm outline-none focus:border-neutral-500"
              >
                <option value="25">25</option>
                <option value="50">50</option>
                <option value="100">100</option>
                <option value="200">200</option>
              </select>
            </label>
          </div>

          <div className="flex items-end md:col-span-12 md:justify-end">
            <button type="submit" className="rounded-md bg-black px-3 py-2 text-sm font-medium text-white hover:bg-black/90">
              Apply
            </button>
          </div>
        </form>
      </section>

      <section className="overflow-x-auto rounded-lg border border-neutral-200 bg-white">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="bg-neutral-50 text-left">
              <th className="px-3 py-2 font-medium text-neutral-700">Date (UTC)</th>
              <th className="px-3 py-2 font-medium text-neutral-700">Product</th>
              <th className="px-3 py-2 font-medium text-neutral-700">Type</th>
              <th className="px-3 py-2 font-medium text-neutral-700">Reason</th>
              <th className="px-3 py-2 text-right font-medium text-neutral-700">Qty</th>
            </tr>
          </thead>
          <tbody>
            {movementsPage.items.map((m) => {
              const productName = productMap.get(m.productId)?.name ?? m.productId;
              const qty = signedQuantity(m);
              return (
                <tr key={m.id} className="border-t border-neutral-200">
                  <td className="px-3 py-2 whitespace-nowrap">{formatDateTimeUtc(m.createdAt)}</td>
                  <td className="px-3 py-2">{productName}</td>
                  <td className="px-3 py-2">{m.type}</td>
                  <td className="px-3 py-2">{m.reason}</td>
                  <td className="px-3 py-2 text-right tabular-nums">
                    <span className={qty < 0 ? "text-red-700" : "text-green-700"}>{qty}</span>
                  </td>
                </tr>
              );
            })}
            {movementsPage.items.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-3 py-10 text-center text-sm text-neutral-600">
                  No movements found.
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </section>

      <footer className="flex items-center justify-between">
        <div className="text-xs text-neutral-600">
          Page <span className="font-medium text-neutral-800">{movementsPage.page}</span> of{" "}
          <span className="font-medium text-neutral-800">{totalPages}</span> • Total{" "}
          <span className="font-medium text-neutral-800">{movementsPage.total}</span>
        </div>
        <div className="flex items-center gap-2">
          <Link
            href={hasPrev ? pageHref(movementsPage.page - 1) : "#"}
            aria-disabled={!hasPrev}
            className={
              hasPrev
                ? "rounded-md border border-neutral-300 bg-white px-3 py-1.5 text-sm font-medium text-neutral-800 shadow-sm hover:bg-neutral-50"
                : "cursor-not-allowed rounded-md border border-neutral-200 bg-neutral-100 px-3 py-1.5 text-sm font-medium text-neutral-500"
            }
          >
            Prev
          </Link>
          <Link
            href={hasNext ? pageHref(movementsPage.page + 1) : "#"}
            aria-disabled={!hasNext}
            className={
              hasNext
                ? "rounded-md border border-neutral-300 bg-white px-3 py-1.5 text-sm font-medium text-neutral-800 shadow-sm hover:bg-neutral-50"
                : "cursor-not-allowed rounded-md border border-neutral-200 bg-neutral-100 px-3 py-1.5 text-sm font-medium text-neutral-500"
            }
          >
            Next
          </Link>
        </div>
      </footer>
    </main>
  );
}

