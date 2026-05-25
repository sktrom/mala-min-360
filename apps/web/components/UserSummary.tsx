"use client";

import { useEffect, useState } from "react";
import type { CurrentUser } from "@/lib/api";
import { getStoredUser } from "@/lib/auth-storage";

export function UserSummary() {
  const [user, setUser] = useState<CurrentUser | null>(null);

  useEffect(() => {
    setUser(getStoredUser());
  }, []);

  if (!user) {
    return null;
  }

  return (
    <p className="user-summary">
      مرحبا {user.fullName} · {user.tenantName}
    </p>
  );
}
