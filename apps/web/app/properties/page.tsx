import Link from "next/link";
import { BrandHeader } from "@/components/BrandHeader";
import { PropertyCard } from "@/components/PropertyCard";

const properties = [
  {
    title: "شقة عائلية في المالكي",
    location: "دمشق · المالكي",
    price: "75,000 USD",
    status: "published" as const
  },
  {
    title: "مكتب تجاري في أبو رمانة",
    location: "دمشق · أبو رمانة",
    price: "1,200 USD / شهر",
    status: "draft" as const
  },
  {
    title: "أرض سكنية قرب يعفور",
    location: "ريف دمشق · يعفور",
    price: "140,000 USD",
    status: "published" as const
  }
];

export default function PropertiesPage() {
  return (
    <main className="app-shell">
      <aside className="sidebar">
        <BrandHeader compact />
        <nav className="side-nav" aria-label="لوحة التحكم">
          <Link href="/dashboard">الرئيسية</Link>
          <Link className="active" href="/properties">
            العقارات
          </Link>
          <Link href="/login">الخروج</Link>
        </nav>
      </aside>
      <section className="main-content">
        <div className="toolbar">
          <div>
            <p className="eyebrow">إدارة العقارات</p>
            <h1>العقارات</h1>
          </div>
          <button className="button primary" type="button">
            إضافة عقار
          </button>
        </div>
        <div className="grid properties-list">
          {properties.map((property) => (
            <PropertyCard key={property.title} {...property} />
          ))}
        </div>
      </section>
    </main>
  );
}
