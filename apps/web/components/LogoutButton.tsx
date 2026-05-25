"use client";

import { useRouter } from "next/navigation";
import { clearAccessToken } from "@/lib/auth-storage";

export function LogoutButton() {
  const router = useRouter();

  function handleLogout() {
    clearAccessToken();
    router.replace("/login");
  }

  return (
    <button className="button secondary" type="button" onClick={handleLogout}>
      تسجيل الخروج
    </button>
  );
}
