export type CurrentUser = {
  id: string;
  tenantId: string;
  fullName: string;
  email: string;
  role: string;
  tenantName: string;
  tenantSlug: string;
};

export type AuthResponse = {
  accessToken: string;
  expiresAt: string;
  user: CurrentUser;
};

type ApiEnvelope<T> = {
  success: boolean;
  data?: T;
  error?: {
    code: string;
    message: string;
  };
};

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ?? "http://localhost:5000";

export async function login(email: string, password: string): Promise<AuthResponse> {
  return request<AuthResponse>("/api/auth/login", {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({ email, password })
  });
}

export async function getCurrentUser(token: string): Promise<CurrentUser> {
  return request<CurrentUser>("/api/auth/me", {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });
}

async function request<T>(path: string, init: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      Accept: "application/json",
      ...init.headers
    }
  });

  const payload = (await response.json().catch(() => null)) as ApiEnvelope<T> | null;

  if (!response.ok || !payload?.success || !payload.data) {
    throw new Error(payload?.error?.message ?? "تعذر الاتصال بالخادم. حاول مرة أخرى.");
  }

  return payload.data;
}
