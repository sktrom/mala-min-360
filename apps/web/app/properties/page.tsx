import Link from "next/link";
import { BrandHeader } from "@/components/BrandHeader";
import { LogoutButton } from "@/components/LogoutButton";
import { PropertiesManager } from "@/components/PropertiesManager";
import { RequireAuth } from "@/components/RequireAuth";

export default function PropertiesPage() {
  return (
    <RequireAuth>
      <main className="app-shell">
        <aside className="sidebar">
          <BrandHeader compact />
          <nav className="side-nav" aria-label="لوحة التحكم">
            <Link href="/dashboard">الرئيسية</Link>
            <Link className="active" href="/properties">
              العقارات
            </Link>
          </nav>
        </aside>
        <section className="main-content">
          <div className="toolbar">
            <div>
              <p className="eyebrow">إدارة العقارات</p>
              <h1>العقارات</h1>
            </div>
            <LogoutButton />
          </div>
          <PropertiesManager />
        </section>
      </main>
    </RequireAuth>
  );
}
