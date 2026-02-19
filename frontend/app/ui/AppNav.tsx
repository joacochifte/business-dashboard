import Link from "next/link";
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

export default function AppNav({ className, links = defaultLinks }: Props) {
  const linkClassName =
    "rounded-xl border border-black/10 bg-white/60 px-4 py-2.5 text-sm font-semibold text-neutral-900 shadow-sm backdrop-blur transition hover:bg-white/80";

  return (
    <div className={`flex items-center gap-2 ${className ?? ""}`}>
      <nav className="flex items-center gap-2">
        {links.map((l) => (
          <Link key={l.href} href={l.href} className={linkClassName}>
            {l.label}
          </Link>
        ))}
      </nav>
      <NotificationBell />
    </div>
  );
}
