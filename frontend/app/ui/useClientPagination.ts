"use client";

import { useMemo, useState } from "react";

export type ClientPaginationState<T> = {
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  startItem: number;
  endItem: number;
  items: T[];
  canPrev: boolean;
  canNext: boolean;
  setPage: (page: number) => void;
  resetPage: () => void;
};

export default function useClientPagination<T>(allItems: T[], pageSize = 20): ClientPaginationState<T> {
  const [requestedPage, setRequestedPage] = useState(1);

  const totalItems = allItems.length;
  const totalPages = Math.max(1, Math.ceil(totalItems / pageSize));
  const page = Math.min(requestedPage, totalPages);
  const startIndex = (page - 1) * pageSize;

  const items = useMemo(() => allItems.slice(startIndex, startIndex + pageSize), [allItems, pageSize, startIndex]);

  return {
    page,
    pageSize,
    totalItems,
    totalPages,
    startItem: totalItems === 0 ? 0 : startIndex + 1,
    endItem: Math.min(startIndex + pageSize, totalItems),
    items,
    canPrev: page > 1,
    canNext: page < totalPages,
    setPage: (nextPage) => setRequestedPage(Math.max(1, nextPage)),
    resetPage: () => setRequestedPage(1),
  };
}
