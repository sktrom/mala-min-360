"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import { login } from "@/lib/api";
import { setAccessToken, setStoredUser } from "@/lib/auth-storage";

export function LoginForm() {
  const router = useRouter();
  const [email, setEmail] = useState("owner@demo.local");
  const [password, setPassword] = useState("Demo12345!");
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      const auth = await login(email, password);
      setAccessToken(auth.accessToken);
      setStoredUser(auth.user);
      router.push("/dashboard");
      router.refresh();
    } catch (loginError) {
      setError(loginError instanceof Error ? loginError.message : "فشل تسجيل الدخول.");
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <label className="field">
        البريد الإلكتروني
        <input
          type="email"
          name="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          autoComplete="email"
          required
        />
      </label>
      <label className="field">
        كلمة المرور
        <input
          type="password"
          name="password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          autoComplete="current-password"
          required
        />
      </label>
      {error && <p className="form-error">{error}</p>}
      <button className="button primary" type="submit" disabled={isLoading}>
        {isLoading ? "جاري الدخول..." : "دخول"}
      </button>
    </form>
  );
}
