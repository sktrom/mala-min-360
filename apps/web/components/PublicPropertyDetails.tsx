import Link from "next/link";
import type { PublicProperty, PublicPropertyImage } from "@/lib/types";
import { getMediaUrl } from "@/lib/url";

type PublicPropertyDetailsProps = {
  property: PublicProperty;
};

const listingTypeLabels: Record<string, string> = {
  Sale: "بيع",
  Rent: "إيجار"
};

const propertyTypeLabels: Record<string, string> = {
  Apartment: "شقة",
  Villa: "فيلا",
  House: "منزل",
  Shop: "محل",
  Office: "مكتب",
  Land: "أرض"
};

export function PublicPropertyDetails({ property }: PublicPropertyDetailsProps) {
  const galleryImages = getGalleryImages(property);
  const whatsAppUrl = getWhatsAppUrl(property);
  const tourUrl = `/a/${property.tenant.slug}/${property.slug}/tour`;

  return (
    <main className="public-property">
      <div className="container">
        <section className="public-hero">
          <PublicGallery images={galleryImages} title={property.title} />

          <aside className="panel public-info">
            <span className="badge published">منشور</span>
            <h1>{property.title}</h1>
            <div className="price">
              {formatPrice(property.price)} {property.currency}
            </div>
            <p className="meta-row">
              {property.city} · {property.areaName} · {property.areaSqm} م²
            </p>
            {property.addressText && <p>{property.addressText}</p>}

            <div className="public-specs">
              <Spec label="نوع الإعلان" value={listingTypeLabels[property.listingType] ?? property.listingType} />
              <Spec label="نوع العقار" value={propertyTypeLabels[property.propertyType] ?? property.propertyType} />
              <Spec label="الغرف" value={formatOptionalNumber(property.bedrooms)} />
              <Spec label="الحمامات" value={formatOptionalNumber(property.bathrooms)} />
              <Spec label="الطابق" value={formatOptionalNumber(property.floorNumber)} />
              <Spec label="المساحة" value={`${formatPrice(property.areaSqm)} م²`} />
            </div>

            {property.description && <p className="public-description">{property.description}</p>}

            <div className="tenant-summary">
              <strong>{property.tenant.name}</strong>
              {property.tenant.city && <span>{property.tenant.city}</span>}
              {property.tenant.phone && <span>{property.tenant.phone}</span>}
            </div>

            <div className="actions">
              <Link className="button primary" href={tourUrl}>
                ابدأ جولة 360
              </Link>
              {whatsAppUrl && (
                <a className="button secondary" href={whatsAppUrl} target="_blank" rel="noreferrer">
                  تواصل واتساب
                </a>
              )}
              {property.tenant.phone && (
                <a className="button secondary" href={`tel:${property.tenant.phone}`}>
                  اتصال
                </a>
              )}
            </div>
          </aside>
        </section>
      </div>
    </main>
  );
}

function PublicGallery({ images, title }: { images: PublicPropertyImage[]; title: string }) {
  if (images.length === 0) {
    return (
      <div className="gallery public-gallery-placeholder" aria-label="معرض العقار">
        <div className="gallery-main" />
        <div className="gallery-side">
          <span />
          <span />
        </div>
      </div>
    );
  }

  const [coverImage, ...secondaryImages] = images;

  return (
    <div className="gallery" aria-label="معرض العقار">
      <div className="public-gallery-image main">
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img src={getMediaUrl(coverImage.url)} alt={title} />
      </div>
      <div className="gallery-side">
        {secondaryImages.slice(0, 2).map((image, index) => (
          <span className="public-gallery-image" key={`${image.url}-${index}`}>
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={getMediaUrl(image.url)} alt={`${title} ${index + 2}`} />
          </span>
        ))}
        {secondaryImages.length === 0 && <span />}
        {secondaryImages.length <= 1 && <span />}
      </div>
    </div>
  );
}

function Spec({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  );
}

function getGalleryImages(property: PublicProperty): PublicPropertyImage[] {
  const images = property.images ?? [];

  if (images.length > 0) {
    return [...images].sort((first, second) => {
      if (first.isCover !== second.isCover) {
        return first.isCover ? -1 : 1;
      }

      return first.sortOrder - second.sortOrder;
    });
  }

  if (property.coverImageUrl) {
    return [{ url: property.coverImageUrl, sortOrder: 0, isCover: true }];
  }

  return [];
}

function getWhatsAppUrl(property: PublicProperty): string | null {
  const rawNumber = property.tenant.whatsAppNumber;

  if (!rawNumber) {
    return null;
  }

  const number = rawNumber.replace(/[^\d]/g, "");
  const message = encodeURIComponent(`مرحباً، أريد الاستفسار عن العقار: ${property.title}`);

  return `https://wa.me/${number}?text=${message}`;
}

function formatOptionalNumber(value: number | null): string {
  return value === null ? "غير محدد" : formatPrice(value);
}

function formatPrice(value: number): string {
  return new Intl.NumberFormat("en-US", {
    maximumFractionDigits: 2
  }).format(value);
}
