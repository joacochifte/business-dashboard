"use client";

type Props = {
  page: number;
  totalPages: number;
  totalItems: number;
  startItem: number;
  endItem: number;
  itemLabel: string;
  onPageChange: (page: number) => void;
};

export default function ClientPagination({
  page,
  totalPages,
  totalItems,
  startItem,
  endItem,
  itemLabel,
  onPageChange,
}: Props) {
  return (
    <footer className="mt-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
      <div className="text-xs text-neutral-600">
        Showing <span className="font-medium text-neutral-800">{startItem}</span>-
        <span className="font-medium text-neutral-800">{endItem}</span> of{" "}
        <span className="font-medium text-neutral-800">{totalItems}</span> {itemLabel}
      </div>

      <div className="flex items-center gap-2">
        <button
          type="button"
          onClick={() => onPageChange(page - 1)}
          disabled={page <= 1}
          className={
            page > 1
              ? "rounded-xl border border-black/10 bg-white/60 px-4 py-2 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
              : "cursor-not-allowed rounded-xl border border-black/5 bg-white/30 px-4 py-2 text-sm font-semibold text-neutral-500 backdrop-blur"
          }
        >
          Prev
        </button>
        <div className="text-xs font-medium text-neutral-700">
          Page {page} / {totalPages}
        </div>
        <button
          type="button"
          onClick={() => onPageChange(page + 1)}
          disabled={page >= totalPages}
          className={
            page < totalPages
              ? "rounded-xl border border-black/10 bg-white/60 px-4 py-2 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
              : "cursor-not-allowed rounded-xl border border-black/5 bg-white/30 px-4 py-2 text-sm font-semibold text-neutral-500 backdrop-blur"
          }
        >
          Next
        </button>
      </div>
    </footer>
  );
}
