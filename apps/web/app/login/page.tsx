import { BrandHeader } from "@/components/BrandHeader";

export default function LoginPage() {
  return (
    <main className="auth-shell">
      <section className="card login-card">
        <BrandHeader compact />
        <h1>تسجيل الدخول</h1>
        <form>
          <label className="field">
            البريد الإلكتروني
            <input type="email" name="email" placeholder="owner@demo.local" />
          </label>
          <label className="field">
            كلمة المرور
            <input type="password" name="password" placeholder="Demo12345!" />
          </label>
          <button className="button primary" type="button">
            دخول
          </button>
        </form>
        <p className="note">بيانات التجربة: owner@demo.local / Demo12345!</p>
      </section>
    </main>
  );
}
