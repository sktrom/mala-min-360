import Link from "next/link";
import { BrandHeader } from "@/components/BrandHeader";
import { LogoutButton } from "@/components/LogoutButton";
import { RequireAuth } from "@/components/RequireAuth";
import { SubscriptionSummary } from "@/components/SubscriptionSummary";

export default function SubscriptionPage() {
  return (
    <RequireAuth>
      <main className="app-shell">
        <aside className="sidebar">
          <BrandHeader compact />
          <nav className="side-nav" aria-label="لوحة التحكم">
            <Link href="/dashboard">الرئيسية</Link>
            <Link href="/properties">العقارات</Link>
            <Link className="active" href="/subscription">
              الاشتراك
            </Link>
          </nav>
        </aside>
        <section className="main-content">
          <div className="toolbar">
            <div>
              <p className="eyebrow">الباقة والحدود</p>
              <h1>الاشتراك والحدود</h1>
            </div>
            <LogoutButton />
          </div>
          <SubscriptionSummary />
        </section>
      </main>
    </RequireAuth>
  );
}
