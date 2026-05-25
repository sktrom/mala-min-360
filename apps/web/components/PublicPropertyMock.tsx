import Link from "next/link";

export function PublicPropertyMock() {
  return (
    <main className="public-property">
      <div className="container">
        <section className="public-hero">
          <div className="gallery" aria-label="معرض العقار">
            <div className="gallery-main" />
            <div className="gallery-side">
              <span />
              <span />
            </div>
          </div>

          <aside className="panel public-info">
            <span className="badge published">منشور</span>
            <h1>شقة عائلية في المالكي</h1>
            <div className="price">75,000 USD</div>
            <p className="meta-row">دمشق · المالكي · 120 م² · 3 غرف</p>
            <p>
              صفحة عرض أولية لعقار تجريبي مع مساحة للصور، السعر، الموقع، وأزرار التواصل والجولة.
            </p>
            <div className="actions">
              <Link className="button primary" href="#">
                ابدأ جولة 360
              </Link>
              <Link className="button secondary" href="#">
                تواصل واتساب
              </Link>
            </div>
            <div className="qr-box">QR</div>
          </aside>
        </section>
      </div>
    </main>
  );
}
