"use client";

import { useEffect, useState } from "react";

type Variant = "date" | "datetime";

type Props = {
  iso: string;
  variant?: Variant;
  className?: string;
  fallback?: string;
  parseDateOnly?: boolean;
};

export default function ClientDateTime({
  iso,
  variant = "datetime",
  className,
  fallback,
  parseDateOnly,
}: Props) {
  const [text, setText] = useState(fallback ?? "...");

  useEffect(() => {
    let d = new Date(iso);
    if (parseDateOnly) {
      const [y, m, day] = iso.slice(0, 10).split("-").map(Number);
      d = new Date(y, (m ?? 1) - 1, day ?? 1, 0, 0, 0, 0);
    }
    if (Number.isNaN(d.getTime())) {
      setText(fallback ?? iso);
      return;
    }

    const opts: Intl.DateTimeFormatOptions =
      variant === "date"
        ? { dateStyle: "medium" }
        : { dateStyle: "medium", timeStyle: "short" };

    // Use browser locale + timezone so it matches the user's machine.
    setText(new Intl.DateTimeFormat(undefined, opts).format(d));
  }, [iso, variant, fallback]);

  return (
    <span className={className} suppressHydrationWarning>
      {text}
    </span>
  );
}
