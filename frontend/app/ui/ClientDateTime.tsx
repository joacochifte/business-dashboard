"use client";

import { useEffect, useState } from "react";

type Variant = "date" | "datetime";

type Props = {
  iso: string;
  variant?: Variant;
  className?: string;
  fallback?: string;
};

export default function ClientDateTime({ iso, variant = "datetime", className, fallback }: Props) {
  const [text, setText] = useState(fallback ?? "...");

  useEffect(() => {
    const d = new Date(iso);
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
