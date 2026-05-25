import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Mala Min 360 — مالا من 360",
  description: "منصة عقارية لجولات 360 وصفحات عقارات احترافية"
};

export default function RootLayout({
  children
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="ar" dir="rtl">
      <body>{children}</body>
    </html>
  );
}
