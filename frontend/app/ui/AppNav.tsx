"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useEffect, useState } from "react";
import NotificationBell from "./NotificationBell";

type NavLink = {
  href: string;
  label: string;
};

type Props = {
  className?: string;
  links?: NavLink[];
};

const defaultLinks: NavLink[] = [
  { href: "/", label: "Home" },
  { href: "/dashboard", label: "Dashboard" },
  { href: "/products", label: "Products" },
  { href: "/sales", label: "Sales" },
  { href: "/debts", label: "Debts" },
  { href: "/customers", label: "Customers" },
  { href: "/costs", label: "Costs" },
  { href: "/inventory", label: "Inventory" },
];

function isActive(pathname: string, href: string) {
  if (href === "/") return pathname === "/";
  return pathname === href || pathname.startsWith(`${href}/`);
}

export default function AppNav({ className, links = defaultLinks }: Props) {
  const pathname = usePathname();
  const [mobileOpen, setMobileOpen] = useState(false);

  useEffect(() => {
    setMobileOpen(false);
  }, [pathname]);

  const baseLinkClass =
    "inline-flex items-center justify-center rounded-xl border px-3.5 py-2 text-xs font-semibold uppercase tracking-wide transition";
  const desktopPalette = (active: boolean) =>
    active
      ? "border-neutral-900 bg-neutral-900 text-white"
      : "border-black/10 bg-white/80 text-neutral-900 hover:bg-white";
  const mobilePalette = (active: boolean) =>
    active
      ? "border-neutral-900 bg-neutral-900 text-white"
      : "border-black/5 bg-white text-neutral-900 hover:bg-neutral-50";

  const renderDesktopLink = (link: NavLink) => {
    const active = isActive(pathname, link.href);
    return (
      <Link
        key={link.href}
        href={link.href}
        className={`${baseLinkClass} text-sm normal-case tracking-normal ${desktopPalette(active)}`}
        aria-current={active ? "page" : undefined}
      >
        {link.label}
      </Link>
    );
  };

  const renderMobileLink = (link: NavLink) => {
    const active = isActive(pathname, link.href);
    return (
      <Link
        key={link.href}
        href={link.href}
        className={`${baseLinkClass} w-full justify-start px-4 ${mobilePalette(active)}`}
        aria-current={active ? "page" : undefined}
      >
        {link.label}
      </Link>
    );
  };

  return (
    <>
      <div className="sticky top-0 z-40 border-b border-black/10 bg-white/90 backdrop-blur md:hidden">
        <div className="flex items-center justify-between gap-2 px-4 py-3">
          <div className="flex items-center gap-2">
            <div className="grid h-9 w-9 place-items-center rounded-xl border border-black/10 bg-white text-sm font-semibold text-neutral-900">
              BD
            </div>
            <div className="text-sm font-semibold text-neutral-900">Business Dashboard</div>
          </div>
          <div className="flex items-center gap-2">
            <NotificationBell />
            <button
              type="button"
              onClick={() => setMobileOpen((open) => !open)}
              className="inline-flex h-10 w-10 items-center justify-center rounded-xl border border-black/10 bg-white/80 text-neutral-800 shadow-sm transition hover:bg-white"
              aria-label="Abrir navegación"
              aria-expanded={mobileOpen}
            >
              {mobileOpen ? (
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <line x1="18" x2="6" y1="6" y2="18" />
                  <line x1="6" x2="18" y1="6" y2="18" />
                </svg>
              ) : (
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <line x1="3" x2="21" y1="6" y2="6" />
                  <line x1="3" x2="21" y1="12" y2="12" />
                  <line x1="3" x2="21" y1="18" y2="18" />
                </svg>
              )}
            </button>
          </div>
        </div>
        {mobileOpen && (
          <div className="border-t border-black/10 px-4 py-3">
            <nav className="flex flex-col gap-1.5">{links.map((link) => renderMobileLink(link))}</nav>
          </div>
        )}
      </div>

      <div className={`hidden md:block ${className ?? ""}`}>
        <div className="fixed left-1/2 top-6 z-40 w-[min(960px,calc(100%_-_3rem))] -translate-x-1/2 rounded-2xl border border-black/10 bg-white/70 px-4 py-2 shadow-lg shadow-black/10 backdrop-blur">
          <div className="flex items-center justify-between gap-3">
            <nav className="flex flex-wrap items-center gap-1.5">{links.map((link) => renderDesktopLink(link))}</nav>
            <NotificationBell />
          </div>
        </div>
      </div>
    </>
  );
}
