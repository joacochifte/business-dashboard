"use client";

import { useEffect, useState } from "react";

export default function TzOffsetField() {
  const [offset, setOffset] = useState(0);

  useEffect(() => {
    setOffset(new Date().getTimezoneOffset());
  }, []);

  // Minutes to add to local time to get UTC (JS getTimezoneOffset contract).
  return <input type="hidden" name="tzOffset" value={String(offset)} />;
}

