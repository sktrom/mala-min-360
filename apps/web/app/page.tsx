import Link from "next/link";
import { BrandHeader } from "@/components/BrandHeader";
import { FeatureCard } from "@/components/FeatureCard";

const features = [
  {
    icon: "01",
    title: "صفحات عقارات احترافية",
    description: "اعرض عقارك بصفحة أنيقة وسريعة تبرز السعر والموقع والصور وتفاصيل التواصل."
  },
  {
    icon: "02",
    title: "جولات 360",
    description: "جولات تفاعلية تساعد العميل على فهم المساحة واتخاذ قرار أسرع قبل الزيارة."
  },
  {
    icon: "03",
    title: "مشاركة واتساب و QR",
    description: "شارك الرابط عبر واتساب وتابع اهتمام العملاء من صفحة واحدة سهلة الوصول."
  },
  {
    icon: "04",
    title: "إحصائيات بسيطة",
    description: "مؤشرات يومية مختصرة للمشاهدات والجولات وضغطات التواصل بدون تعقيد."
  }
];

export default function Home() {
  return (
    <main className="page">
      <div className="container">
        <BrandHeader />

        <section className="hero">
          <div className="hero-copy">
            <p className="eyebrow">منصة عقارية ذكية للمكاتب الحديثة</p>
            <h1>بيتك من كل زاوية</h1>
            <p className="lead">
              اعرض عقارك بجولة 360 وصفحة احترافية خلال دقائق، وشارك الرابط عبر واتساب
              لتقديم تجربة موثوقة وحديثة للعملاء.
            </p>
            <div className="actions">
              <Link className="button primary" href="/login">
                ابدأ الآن
              </Link>
              <Link className="button secondary" href="/a/demo-agency/demo-property">
                شاهد مثال
              </Link>
            </div>
          </div>

          <div className="hero-visual" aria-label="معاينة صفحة عقار">
            <div className="property-preview-image" />
            <div className="property-preview-body">
              <div className="badge-row">
                <span className="badge published">منشور</span>
                <span className="badge">جولة 360</span>
              </div>
              <h3>اعرض عقارك من كل زاوية</h3>
              <p className="meta-row">المالكي · 120 م² · 3 غرف</p>
            </div>
          </div>
        </section>
      </div>

      <section className="section">
        <div className="container">
          <div className="section-heading">
            <h2>جولات 360 وصفحات عقارية احترافية</h2>
            <p>كل ما يحتاجه المكتب لعرض العقار بثقة، من الرابط العام حتى متابعة الاهتمام.</p>
          </div>
          <div className="grid feature-grid">
            {features.map((feature) => (
              <FeatureCard key={feature.title} {...feature} />
            ))}
          </div>
        </div>
      </section>

      <section className="section">
        <div className="container">
          <div className="agency-band">
            <div>
              <h2>مصمم للمكاتب العقارية التي تريد حضورا رقميا أرقى</h2>
              <p>
                واجهات عربية، صفحات عامة قابلة للمشاركة، وجولات 360 تساعد العميل على رؤية
                التفاصيل قبل الزيارة.
              </p>
            </div>
            <div className="mosaic" aria-hidden="true">
              {Array.from({ length: 15 }).map((_, index) => (
                <span key={index} />
              ))}
            </div>
          </div>
        </div>
      </section>

      <footer className="footer">
        <div className="container">Mala Min 360 · مالا من 360</div>
      </footer>
    </main>
  );
}
