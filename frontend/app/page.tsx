import Link from "next/link";
import AppNav from "./ui/AppNav";

const quickLinks = [
  { href: "/dashboard", title: "Dashboard", description: "Metrics and trends" },
  { href: "/products", title: "Products", description: "Catalog and stock status" },
  { href: "/sales", title: "Sales", description: "Transactions and totals" },
  { href: "/inventory", title: "Inventory", description: "Movements and adjustments" },
  { href: "/costs", title: "Costs", description: "Operational expenses" },
];

export default function HomePage() {
  return (
    <main className="relative min-h-screen overflow-hidden bg-[radial-gradient(1000px_500px_at_20%_-10%,rgba(245,240,235,1),transparent),radial-gradient(800px_450px_at_90%_5%,rgba(244,216,208,0.85),transparent),linear-gradient(to_bottom,#fffbf4,#f4eed7)]">
      <div className="pointer-events-none absolute inset-0">
        <div className="absolute left-[-120px] top-[-120px] h-[300px] w-[300px] rounded-full bg-amber-200/35 blur-3xl" />
        <div className="absolute right-[-120px] top-[6%] h-[280px] w-[280px] rounded-full bg-rose-200/35 blur-3xl" />
      </div>

      <div className="relative mx-auto max-w-5xl px-6 py-10">
        <header className="flex items-center justify-between">
          <div className="space-y-1">
            <h1 className="text-3xl font-semibold tracking-tight text-neutral-950">Business Dashboard</h1>
          </div>
          <div className="flex items-center gap-2">
            <AppNav className="hidden md:flex" />
          </div>
        </header>

        <section className="mt-16 text-center">
          <h1 className="text-balance text-4xl font-semibold tracking-tight text-neutral-950 sm:text-5xl">
            Manage products, sales, inventory, and costs in one place.
          </h1>
          <p className="mx-auto mt-4 max-w-2xl text-pretty text-base leading-7 text-neutral-700">
            A simple business dashboard focused on day-to-day operations and clear insights.
          </p>

          <div className="mt-7 flex flex-col items-center justify-center gap-3 sm:flex-row">
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
              Add product
            </Link>
            <Link
              href="/sales/new"
              className="inline-flex items-center justify-center rounded-xl border border-black/10 bg-white/60 px-4 py-2.5 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
            >
              Add sale
            </Link>
            <Link
              href="/costs/new"
              className="inline-flex items-center justify-center rounded-xl border border-black/10 bg-white/60 px-4 py-2.5 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
            >
              Add cost
            </Link>
            <Link
              href="/products/new"
              className="inline-flex items-center justify-center rounded-xl border border-black/10 bg-white/60 px-4 py-2.5 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80"
            >
              Add product
            </Link>
          </div>
        </section>

        <section className="mt-12 rounded-3xl border border-black/10 bg-white/60 p-4 shadow-sm backdrop-blur sm:p-5">
          <div className="mb-4 text-sm font-semibold text-neutral-900">Quick links</div>
          <div className="grid gap-3 sm:grid-cols-2">
            {quickLinks.map((item) => (
              <Link
                key={item.href}
                href={item.href}
                className="rounded-2xl border border-black/10 bg-white/60 px-4 py-3 shadow-sm transition hover:bg-white/80"
              >
                <div className="text-sm font-semibold text-neutral-900">{item.title}</div>
                <div className="mt-0.5 text-xs text-neutral-600">{item.description}</div>
              </Link>
            ))}
          </div>
        </section>

        <footer className="mt-10 flex flex-col items-center gap-1 pb-4">
          <div className="flex items-center gap-3">
            <div className="grid h-9 w-9 place-items-center rounded-xl border border-black/10 bg-white/70 shadow-sm backdrop-blur">
              <span className="text-sm font-semibold tracking-tight text-neutral-900">BD</span>
            </div>
            <div className="leading-tight">
              <div className="text-sm font-semibold text-neutral-900">Business Dashboard</div>
              <div className="text-xs text-neutral-600">.NET 8 + Next.js</div>
            </div>
          </div>
          <p className="text-[11px] text-neutral-500">Developed by Joaquín Chifteian</p>
        </footer>
      </div>
    </main>
  );
}
