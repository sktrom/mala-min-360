"use client";

import { useRouter } from "next/navigation";
import { type ReactNode, useEffect, useState } from "react";
import { getCurrentUser } from "@/lib/api";
import { clearAccessToken, getAccessToken, setStoredUser } from "@/lib/auth-storage";

type RequireAuthProps = {
  children: ReactNode;
};

export function RequireAuth({ children }: RequireAuthProps) {
  const router = useRouter();
  const [isVerified, setIsVerified] = useState(false);

  useEffect(() => {
    let isMounted = true;

    async function verifyToken() {
      const token = getAccessToken();

      if (!token) {
        router.replace("/login");
        return;
      }

      try {
        const user = await getCurrentUser(token);

        if (!isMounted) {
          return;
        }

        setStoredUser(user);
        setIsVerified(true);
      } catch {
        clearAccessToken();
        router.replace("/login");
      }
    }

    void verifyToken();

    return () => {
      isMounted = false;
    };
  }, [router]);

  if (!isVerified) {
    return (
      <main className="auth-shell">
        <section className="card login-card">
          <p className="note">جاري التحقق من تسجيل الدخول...</p>
        </section>
      </main>
    );
  }

  return <>{children}</>;
}
