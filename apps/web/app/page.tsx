import Link from "next/link";
import { BrandHeader } from "@/components/BrandHeader";

export default function Home() {
  return (
    <main className="page gateway-page">
      <div className="container">
        <BrandHeader />

        <section className="gateway-hero">
          <div className="gateway-copy">
            <p className="eyebrow">بيتك من كل زاوية</p>
            <h1>منصة عقارية ذكية للمكاتب الحديثة</h1>
            <p className="lead">
              أنشئ صفحات عقارية احترافية، اعرض جولات 360، وشارك عقاراتك بثقة مع العملاء.
            </p>
          </div>

          <div className="gateway-preview" aria-hidden="true">
            <div className="gateway-preview-main" />
            <div className="gateway-preview-card">
              <span className="badge published">جولة 360</span>
              <strong>صفحة عقارية احترافية</strong>
              <span>دمشق · المالكي · 120 م²</span>
            </div>
          </div>
        </section>

        <section className="gateway-actions-grid" aria-label="خيارات الدخول">
          <article className="gateway-action-card office">
            <span className="gateway-card-kicker">للمكاتب العقارية</span>
            <h2>دخول المكاتب العقارية</h2>
            <p>لإدارة العقارات، الصور، جولات 360، الإحصائيات والاشتراك.</p>
            <Link className="button primary" href="/login">
              تسجيل دخول المكتب
            </Link>
          </article>

          <article className="gateway-action-card visitor">
            <span className="gateway-card-kicker">للزوار والعملاء</span>
            <h2>الدخول كزائر</h2>
            <p>تصفح تجربة عرض عقاري احترافية وشاهد العقارات والجولات المتاحة.</p>
            <Link className="button gold" href="/visitor">
              دخول المعرض
            </Link>
          </article>
        </section>
      </div>
    </main>
  );
}
