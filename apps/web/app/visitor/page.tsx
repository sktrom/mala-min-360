import Link from "next/link";
import { BrandHeader } from "@/components/BrandHeader";
import { VisitorGallery } from "@/components/VisitorGallery";

export default function VisitorPage() {
  return (
    <main className="page visitor-page">
      <div className="container">
        <BrandHeader />

        <section className="visitor-hero">
          <div>
            <p className="eyebrow">الدخول كزائر</p>
            <h1>معرض العقارات</h1>
            <p className="lead">
              تجربة هادئة لاستكشاف العقارات وصفحات العرض وجولات 360.
            </p>
          </div>
          <div className="visitor-hero-actions">
            <Link className="button secondary" href="/">
              الرئيسية
            </Link>
            <Link className="button primary" href="/login">
              دخول المكتب
            </Link>
          </div>
        </section>

        <VisitorGallery />
      </div>
    </main>
  );
}
