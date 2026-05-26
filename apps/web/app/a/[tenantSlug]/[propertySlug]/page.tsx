import { BrandHeader } from "@/components/BrandHeader";
import { PublicPropertyDetails } from "@/components/PublicPropertyDetails";
import { ApiError, getPublicProperty } from "@/lib/api";

type PublicPropertyPageProps = {
  params: Promise<{
    tenantSlug: string;
    propertySlug: string;
  }>;
};

export default async function PublicPropertyPage({ params }: PublicPropertyPageProps) {
  const { tenantSlug, propertySlug } = await params;

  try {
    const property = await getPublicProperty(tenantSlug, propertySlug);

    return (
      <main className="page">
        <div className="container">
          <BrandHeader />
        </div>
        <PublicPropertyDetails property={property} />
      </main>
    );
  } catch (error) {
    return <UnavailablePropertyPage isNotFound={isNotFoundError(error)} />;
  }
}

function UnavailablePropertyPage({ isNotFound }: { isNotFound: boolean }) {
  return (
    <main className="page">
      <div className="container">
        <BrandHeader />
        <section className="public-unavailable panel">
          <p className="eyebrow">صفحة العقار</p>
          <h1>العقار غير متاح</h1>
          <p>
            {isNotFound
              ? "قد يكون الرابط غير صحيح أو العقار غير منشور حالياً."
              : "تعذر تحميل بيانات العقار حالياً. حاول مرة أخرى لاحقاً."}
          </p>
        </section>
      </div>
    </main>
  );
}

function isNotFoundError(error: unknown): boolean {
  return error instanceof ApiError && error.status === 404;
}
