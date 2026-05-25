import Link from "next/link";

type BrandHeaderProps = {
  compact?: boolean;
};

export function BrandHeader({ compact = false }: BrandHeaderProps) {
  return (
    <header className="brand-header">
      <Link href="/" className="brand-lockup" aria-label="Mala Min 360">
        <span className="brand-mark">360</span>
        <span className="brand-name">
          <span>Mala Min 360</span>
          <span>مالا من 360</span>
        </span>
      </Link>

      {!compact && (
        <nav className="nav-links" aria-label="التنقل الرئيسي">
          <Link href="/a/demo-agency/demo-property">مثال عقار</Link>
          <Link href="/dashboard">لوحة التحكم</Link>
          <Link href="/login">دخول</Link>
        </nav>
      )}
    </header>
  );
}
