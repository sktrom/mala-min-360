import Link from "next/link";

type BrandHeaderProps = {
  compact?: boolean;
};

export function BrandHeader({ compact = false }: BrandHeaderProps) {
  return (
    <header className="brand-header">
      <Link href="/" className="brand-lockup" aria-label="Mala Min 360">
        <span className="brand-mark" aria-hidden="true">
          <span className="brand-house" />
          <span className="brand-orbit" />
          <span className="brand-dot" />
        </span>
        <span className="brand-name">
          <span>مالا من 360</span>
          <span>Mala Min 360</span>
          {!compact && <span className="brand-tagline">بيتك من كل زاوية</span>}
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
