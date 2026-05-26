"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { getPublicProperties } from "@/lib/api";
import type { PublicPropertyCard } from "@/lib/types";
import { getMediaUrl } from "@/lib/url";

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

export function VisitorGallery() {
  const [properties, setProperties] = useState<PublicPropertyCard[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    async function loadProperties() {
      setIsLoading(true);
      setError(null);

      try {
        const loadedProperties = await getPublicProperties();

        if (isMounted) {
          setProperties(loadedProperties);
        }
      } catch {
        if (isMounted) {
          setError("تعذر تحميل المعرض.");
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    void loadProperties();

    return () => {
      isMounted = false;
    };
  }, []);

  if (isLoading) {
    return <p className="note">جاري تحميل المعرض...</p>;
  }

  if (error) {
    return <p className="form-error">{error}</p>;
  }

  if (properties.length === 0) {
    return (
      <div className="visitor-empty panel">
        <p className="eyebrow">المعرض</p>
        <h2>لا توجد عقارات منشورة حالياً.</h2>
        <p>سيظهر هنا العقارات المنشورة من المكاتب عند توفرها.</p>
      </div>
    );
  }

  return (
    <div className="visitor-gallery-grid">
      {properties.map((property) => (
        <article className="visitor-property-card" key={property.id}>
          <div className="visitor-card-image">
            {property.coverImageUrl ? (
              // eslint-disable-next-line @next/next/no-img-element
              <img src={getMediaUrl(property.coverImageUrl)} alt={property.title} />
            ) : (
              <div className="visitor-image-placeholder" />
            )}
            <span className="visitor-image-badge">360</span>
          </div>

          <div className="visitor-card-body">
            <div>
              <span className="gateway-card-kicker">{property.tenantName}</span>
              <h2>{property.title}</h2>
              <p className="meta-row">
                {property.city} · {property.areaName} · {property.areaSqm} م²
              </p>
            </div>

            <div className="visitor-card-specs">
              <span>{listingTypeLabels[property.listingType] ?? property.listingType}</span>
              <span>{propertyTypeLabels[property.propertyType] ?? property.propertyType}</span>
              {property.bedrooms !== null && <span>{property.bedrooms} غرف</span>}
              {property.bathrooms !== null && <span>{property.bathrooms} حمام</span>}
            </div>

            <strong className="visitor-card-price">
              {formatPrice(property.price)} {property.currency}
            </strong>

            <div className="actions">
              <Link
                className="button primary"
                href={`/a/${property.tenantSlug}/${property.slug}`}
              >
                عرض التفاصيل
              </Link>
              <Link
                className="button secondary"
                href={`/a/${property.tenantSlug}/${property.slug}/tour`}
              >
                جولة 360
              </Link>
            </div>
          </div>
        </article>
      ))}
    </div>
  );
}

function formatPrice(value: number): string {
  return new Intl.NumberFormat("en-US", {
    maximumFractionDigits: 2
  }).format(value);
}
