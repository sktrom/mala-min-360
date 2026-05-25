import Link from "next/link";
import { BrandHeader } from "@/components/BrandHeader";
import { LogoutButton } from "@/components/LogoutButton";
import { RequireAuth } from "@/components/RequireAuth";
import { UserSummary } from "@/components/UserSummary";
import { DashboardStats } from "@/components/DashboardStats";

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
          <DashboardStats />
        </section>
      </main>
    </RequireAuth>
  );
}

