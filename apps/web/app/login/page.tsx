"use client";

import Link from "next/link";
import { BrandHeader } from "@/components/BrandHeader";
import { LoginForm } from "@/components/LoginForm";

export default function LoginPage() {
  return (
    <main className="auth-shell">
      <section className="card login-card">
        <BrandHeader compact />
        <h1>تسجيل الدخول</h1>
        <LoginForm />
        <p className="note">بيانات التجربة: owner@demo.local / Demo12345!</p>
        <div className="login-links">
          <Link href="/">العودة للرئيسية</Link>
          <Link href="/visitor">الدخول كزائر</Link>
        </div>
      </section>
    </main>
  );
}
