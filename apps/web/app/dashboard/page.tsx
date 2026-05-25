import Link from "next/link";
import { BrandHeader } from "@/components/BrandHeader";
import { StatCard } from "@/components/StatCard";

const stats = [
  { label: "عدد العقارات", value: "12" },
  { label: "مشاهدات العقارات", value: "1,284" },
  { label: "مشاهدات الجولة", value: "438" },
  { label: "ضغطات واتساب", value: "76" }
];

export default function DashboardPage() {
  return (
    <main className="app-shell">
      <aside className="sidebar">
        <BrandHeader compact />
        <nav className="side-nav" aria-label="لوحة التحكم">
          <Link className="active" href="/dashboard">
            الرئيسية
          </Link>
          <Link href="/properties">العقارات</Link>
          <Link href="/login">الخروج</Link>
        </nav>
      </aside>
      <section className="main-content">
        <div className="toolbar">
          <div>
            <p className="eyebrow">لوحة التحكم</p>
            <h1>نظرة عامة</h1>
          </div>
        </div>
        <div className="grid dashboard-grid">
          {stats.map((stat) => (
            <StatCard key={stat.label} {...stat} />
          ))}
        </div>
      </section>
    </main>
  );
}
