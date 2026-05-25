import { BrandHeader } from "@/components/BrandHeader";
import { PublicPropertyMock } from "@/components/PublicPropertyMock";

export default function DemoPropertyPage() {
  return (
    <main className="page">
      <div className="container">
        <BrandHeader />
      </div>
      <PublicPropertyMock />
    </main>
  );
}
