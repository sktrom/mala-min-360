import Link from "next/link";
import { BrandHeader } from "@/components/BrandHeader";
import { LogoutButton } from "@/components/LogoutButton";
import { RequireAuth } from "@/components/RequireAuth";
import { StatCard } from "@/components/StatCard";
import { UserSummary } from "@/components/UserSummary";

const stats = [
  { label: "عدد العقارات", value: "12" },
  { label: "مشاهدات العقارات", value: "1,284" },
  { label: "مشاهدات الجولة", value: "438" },
  { label: "ضغطات واتساب", value: "76" }
];

export default function DashboardPage() {
  return (
    <RequireAuth>
      <main className="app-shell">
        <aside className="sidebar">
          <BrandHeader compact />
          <nav className="side-nav" aria-label="لوحة التحكم">
            <Link className="active" href="/dashboard">
              الرئيسية
            </Link>
            <Link href="/properties">العقارات</Link>
          </nav>
        </aside>
        <section className="main-content">
          <div className="toolbar">
            <div>
              <p className="eyebrow">لوحة التحكم</p>
              <h1>نظرة عامة</h1>
              <UserSummary />
            </div>
            <LogoutButton />
          </div>
          <div className="grid dashboard-grid">
            {stats.map((stat) => (
              <StatCard key={stat.label} {...stat} />
            ))}
          </div>
        </section>
      </main>
    </RequireAuth>
  );
}
