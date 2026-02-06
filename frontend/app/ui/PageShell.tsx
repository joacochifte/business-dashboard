import type { ReactNode } from "react";

type Props = {
  children: ReactNode;
};

export default function PageShell({ children }: Props) {
  return (
    <main className="relative min-h-screen overflow-hidden bg-[radial-gradient(1200px_600px_at_20%_-10%,rgba(245,240,235,1),transparent),radial-gradient(900px_500px_at_90%_10%,rgba(244,216,208,0.9),transparent),linear-gradient(to_bottom,#fffbf4,#f4eed7)]">
      <div className="pointer-events-none absolute inset-0">
        <div className="absolute left-[-160px] top-[-160px] h-[380px] w-[380px] rounded-full bg-amber-200/40 blur-3xl" />
        <div className="absolute right-[-140px] top-[10%] h-[360px] w-[360px] rounded-full bg-rose-200/40 blur-3xl" />
        <div className="absolute bottom-[-200px] left-[30%] h-[460px] w-[460px] rounded-full bg-neutral-200/40 blur-3xl" />
      </div>

      <div className="relative mx-auto max-w-6xl px-6 py-10">{children}</div>
    </main>
  );
}

