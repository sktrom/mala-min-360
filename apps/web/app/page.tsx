import Link from "next/link";
import { BrandHeader } from "@/components/BrandHeader";
import { FeatureCard } from "@/components/FeatureCard";

const features = [
  {
    icon: "01",
    title: "صفحات عقارات احترافية",
    description: "صفحات خفيفة ومنظمة تعرض تفاصيل العقار بوضوح على الهاتف والحاسوب."
  },
  {
    icon: "02",
    title: "جولات 360",
    description: "أساس جاهز لعرض جولات تفاعلية تساعد العميل على فهم المساحة قبل الزيارة."
  },
  {
    icon: "03",
    title: "مشاركة واتساب و QR",
    description: "روابط جاهزة للمشاركة السريعة مع العملاء ومكاتب التسويق العقاري."
  },
  {
    icon: "04",
    title: "إحصائيات بسيطة",
    description: "مؤشرات يومية مختصرة لمتابعة مشاهدات العقارات والجولات والتواصل."
  }
];

export default function Home() {
  return (
    <main className="page">
      <div className="container">
        <BrandHeader />

        <section className="hero">
          <div className="hero-copy">
            <p className="eyebrow">منصة عقارية سورية الهوية</p>
            <h1>مالا من 360 لصفحات عقارية وجولات افتراضية أوضح</h1>
            <p className="lead">
              واجهة عربية خفيفة تساعد المكاتب العقارية على تقديم العقارات بصورة مهنية، مع
              صفحات عامة وجولات 360 ومشاركة سهلة عبر واتساب و QR.
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
              <h3>شقة عائلية في دمشق</h3>
              <p className="meta-row">المالكي · 120 م² · 3 غرف</p>
            </div>
          </div>
        </section>
      </div>

      <section className="section">
        <div className="container">
          <div className="section-heading">
            <h2>أساس واضح لتسويق العقارات</h2>
            <p>المرحلة الأولى تركز على تجربة نظيفة ومباشرة يمكن البناء عليها لاحقا.</p>
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
              <h2>مصمم للمكاتب العقارية التي تحتاج عرضا أسرع وأكثر ثقة</h2>
              <p>
                واجهات عربية، صفحات عامة قابلة للمشاركة، ومساحة منظمة لإدارة العقارات
                والجولات بدون تعقيد.
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
