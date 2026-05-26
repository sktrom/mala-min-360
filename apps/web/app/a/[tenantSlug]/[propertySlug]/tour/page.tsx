import { BrandHeader } from "@/components/BrandHeader";
import { PublicTourViewer } from "@/components/PublicTourViewer";

type PublicTourPageProps = {
  params: Promise<{
    tenantSlug: string;
    propertySlug: string;
  }>;
};

export default async function PublicTourPage({ params }: PublicTourPageProps) {
  const { tenantSlug, propertySlug } = await params;

  return (
    <main className="page">
      <div className="container">
        <BrandHeader />
      </div>
      <PublicTourViewer tenantSlug={tenantSlug} propertySlug={propertySlug} />
    </main>
  );
}
