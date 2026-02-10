import Link from "next/link";
import AppNav from "./ui/AppNav";

function FeatureCard(props: {
  href: string;
  title: string;
  description: string;
  kicker: string;
  icon: React.ReactNode;
}) {
  return (
    <Link
      href={props.href}
      className="group relative overflow-hidden rounded-2xl border border-black/10 bg-white/70 p-5 shadow-sm backdrop-blur transition hover:-translate-y-0.5 hover:shadow-md"
    >
      <div className="absolute inset-0 opacity-0 transition group-hover:opacity-100">
        <div className="absolute -left-24 top-8 h-44 w-44 rounded-full bg-amber-200/50 blur-3xl" />
        <div className="absolute -right-16 bottom-0 h-44 w-44 rounded-full bg-rose-200/50 blur-3xl" />
      </div>

      <div className="relative flex items-start gap-3">
        <div className="flex h-10 w-10 items-center justify-center rounded-xl border border-black/10 bg-white shadow-sm">
          {props.icon}
        </div>
        <div className="min-w-0">
          <div className="text-[11px] font-medium tracking-wide text-neutral-500">{props.kicker}</div>
          <div className="mt-1 text-base font-semibold text-neutral-900">{props.title}</div>
          <p className="mt-1 text-sm text-neutral-600">{props.description}</p>
        </div>
      </div>

      <div className="relative mt-4 flex items-center gap-2 text-sm font-medium text-neutral-900">
        Open
        <span className="inline-block transition group-hover:translate-x-0.5" aria-hidden>
          →
        </span>
      </div>
    </Link>
  );
}

export default function HomePage() {
  return (
    <main className="relative min-h-screen overflow-hidden bg-[radial-gradient(1200px_600px_at_20%_-10%,rgba(245,240,235,1),transparent),radial-gradient(900px_500px_at_90%_10%,rgba(244,216,208,0.9),transparent),linear-gradient(to_bottom,#fffbf4,#f4eed7)]">
      <div className="pointer-events-none absolute inset-0">
        <div className="absolute left-[-160px] top-[-160px] h-[380px] w-[380px] rounded-full bg-amber-200/40 blur-3xl" />
        <div className="absolute right-[-140px] top-[10%] h-[360px] w-[360px] rounded-full bg-rose-200/40 blur-3xl" />
        <div className="absolute bottom-[-200px] left-[30%] h-[460px] w-[460px] rounded-full bg-neutral-200/40 blur-3xl" />
      </div>

      <div className="relative mx-auto max-w-6xl px-6 py-10">
        <header className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="grid h-9 w-9 place-items-center rounded-xl border border-black/10 bg-white/70 shadow-sm backdrop-blur">
              <span className="text-sm font-semibold tracking-tight text-neutral-900">BD</span>
            </div>
            <div className="leading-tight">
              <div className="text-sm font-semibold text-neutral-900">Business Dashboard</div>
              <div className="text-xs text-neutral-600">Portfolio project • .NET + Next.js</div>
            </div>
          </div>

          <AppNav className="hidden md:flex" />
        </header>

        <section className="mt-10 grid gap-8 lg:grid-cols-12 lg:items-end">
          <div className="lg:col-span-7">
            <div className="inline-flex items-center gap-2 rounded-full border border-black/10 bg-white/60 px-3 py-1 text-xs font-medium text-neutral-800 shadow-sm backdrop-blur">
              <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" />
              Live demo UI
              <span className="text-neutral-500">•</span>
              Clean Architecture backend
            </div>

            <h1 className="mt-4 text-balance text-4xl font-semibold tracking-tight text-neutral-950 sm:text-5xl">
              Track products, inventory movements, and sales. Turn it into insights.
            </h1>
            <p className="mt-4 max-w-xl text-pretty text-base leading-7 text-neutral-700">
              A recruiter-friendly business dashboard built for a real family business workflow:
              products, stock, sales, and clear metrics.
            </p>

            <div className="mt-6 flex flex-col gap-3 sm:flex-row">
              <Link
                href="/dashboard"
                className="inline-flex items-center justify-center rounded-xl bg-black px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-black/90"
              >
                Open dashboard
              </Link>
              <Link
                href="/products/new"
                className="inline-flex items-center justify-center rounded-xl border border-black/10 bg-white/60 px-4 py-2.5 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
              >
                Add a product
              </Link>
            </div>

            <div className="mt-8 grid grid-cols-3 gap-3">
              <div className="rounded-2xl border border-black/10 bg-white/60 p-4 shadow-sm backdrop-blur">
                <div className="text-[11px] font-medium tracking-wide text-neutral-500">STACK</div>
                <div className="mt-1 text-sm font-semibold text-neutral-900">.NET 8 • Postgres</div>
              </div>
              <div className="rounded-2xl border border-black/10 bg-white/60 p-4 shadow-sm backdrop-blur">
                <div className="text-[11px] font-medium tracking-wide text-neutral-500">FRONTEND</div>
                <div className="mt-1 text-sm font-semibold text-neutral-900">Next.js • Tailwind</div>
              </div>
              <div className="rounded-2xl border border-black/10 bg-white/60 p-4 shadow-sm backdrop-blur">
                <div className="text-[11px] font-medium tracking-wide text-neutral-500">QUALITY</div>
                <div className="mt-1 text-sm font-semibold text-neutral-900">Tests • DTOs • </div>
              </div>
            </div>
          </div>

          <div className="lg:col-span-5">
            <div className="rounded-3xl border border-black/10 bg-white/60 p-6 shadow-sm backdrop-blur">
              <div className="flex items-center justify-between">
                <div>
                  <div className="text-xs font-medium text-neutral-600">Quick navigation</div>
                  <div className="mt-1 text-lg font-semibold text-neutral-950">Core modules</div>
                </div>
                <div className="hidden rounded-full border border-black/10 bg-white/70 px-3 py-1 text-xs text-neutral-700 shadow-sm md:block">
                  Start here
                </div>
              </div>

              <div className="mt-5 grid gap-3">
                <FeatureCard
                  href="/products"
                  kicker="Catalog"
                  title="Products"
                  description="Create, edit, activate/deactivate, and optionally track stock."
                  icon={
                    <svg viewBox="0 0 24 24" className="h-5 w-5 text-neutral-900" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M7 7h10v10H7z" />
                      <path d="M7 7l5-3 5 3" />
                    </svg>
                  }
                />
                <FeatureCard
                  href="/sales"
                  kicker="Transactions"
                  title="Sales"
                  description="Create sales with items. Stock is validated and discounted."
                  icon={
                    <svg viewBox="0 0 24 24" className="h-5 w-5 text-neutral-900" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M6 7h12l-1 12H7L6 7z" />
                      <path d="M9 7a3 3 0 0 1 6 0" />
                    </svg>
                  }
                />
                <FeatureCard
                  href="/inventory"
                  kicker="History"
                  title="Inventory"
                  description="See movements (IN/OUT/ADJUST) and apply manual adjustments."
                  icon={
                    <svg viewBox="0 0 24 24" className="h-5 w-5 text-neutral-900" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M4 7h16v13H4z" />
                      <path d="M8 7V4h8v3" />
                    </svg>
                  }
                />
                <FeatureCard
                  href="/dashboard"
                  kicker="Insights"
                  title="Dashboard"
                  description="Revenue, sales count, average ticket, trends, and top products."
                  icon={
                    <svg viewBox="0 0 24 24" className="h-5 w-5 text-neutral-900" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M4 19V5" />
                      <path d="M4 19h16" />
                      <path d="M7 15l3-4 3 2 4-6" />
                    </svg>
                  }
                />
                <FeatureCard
                  href="/costs"
                  kicker="Catalog"
                  title="Costs"
                  description="Create, edit and delete costs."
                  icon={
                    <svg viewBox="0 0 24 24" className="h-5 w-5 text-neutral-900" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M7 7h10v10H7z" />
                      <path d="M7 7l5-3 5 3" />
                    </svg>
                  }
                />
              </div>
            </div>
          </div>
        </section>

        <footer className="mt-12 flex flex-col gap-2 border-t border-black/10 pt-6 text-xs text-neutral-600 md:flex-row md:items-center md:justify-between">
          <div>
            Tip: run the API and set <span className="font-mono">NEXT_PUBLIC_API_BASE_URL</span> in{" "}
            <span className="font-mono">frontend/.env.local</span>.
          </div>
          <div className="text-neutral-500">Built for learning: clean commits, clear architecture, tests-first mindset.</div>
        </footer>
      </div>
    </main>
  );
}
