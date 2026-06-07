import {
  LayoutDashboard, MapPin, Server, Grid3x3, BatteryFull,
  Users, Activity, ArrowLeftRight, FileBarChart, Settings,
  type LucideIcon,
} from "lucide-react";

export interface NavItem {
  title: string;
  href: string;
  icon: LucideIcon;
}

export const navItems: NavItem[] = [
  { title: "Dashboard", href: "/dashboard", icon: LayoutDashboard },
  { title: "Stations", href: "/stations", icon: MapPin },
  { title: "Cabinets", href: "/cabinets", icon: Server },
  { title: "Slots", href: "/slots", icon: Grid3x3 },
  { title: "Batteries", href: "/batteries", icon: BatteryFull },
  { title: "Users", href: "/users", icon: Users },
  { title: "Sessions", href: "/sessions", icon: Activity },
  { title: "Transactions", href: "/transactions", icon: ArrowLeftRight },
  { title: "Reports", href: "/reports", icon: FileBarChart },
  { title: "Settings", href: "/settings", icon: Settings },
];
