import type { CurrentUser } from "./api";

const ACCESS_TOKEN_KEY = "malamin360.accessToken";
const USER_KEY = "malamin360.currentUser";

// MVP development storage only. Replace with a stronger refresh/session strategy before production.
export function getAccessToken(): string | null {
  if (!isBrowser()) {
    return null;
  }

  return window.localStorage.getItem(ACCESS_TOKEN_KEY);
}

export function setAccessToken(token: string): void {
  if (isBrowser()) {
    window.localStorage.setItem(ACCESS_TOKEN_KEY, token);
  }
}

export function clearAccessToken(): void {
  if (isBrowser()) {
    window.localStorage.removeItem(ACCESS_TOKEN_KEY);
    window.localStorage.removeItem(USER_KEY);
  }
}

export function getStoredUser(): CurrentUser | null {
  if (!isBrowser()) {
    return null;
  }

  const value = window.localStorage.getItem(USER_KEY);

  if (!value) {
    return null;
  }

  try {
    return JSON.parse(value) as CurrentUser;
  } catch {
    window.localStorage.removeItem(USER_KEY);
    return null;
  }
}

export function setStoredUser(user: CurrentUser): void {
  if (isBrowser()) {
    window.localStorage.setItem(USER_KEY, JSON.stringify(user));
  }
}

function isBrowser(): boolean {
  return typeof window !== "undefined";
}
