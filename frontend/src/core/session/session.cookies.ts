export const AUTH_COOKIE_NAMES = {
  ACCESS_TOKEN: "accessToken",
  REFRESH_TOKEN: "refreshToken",
} as const;

/**
 * Safely reads a cookie by name from the browser document.
 */
export function getAuthCookie(name: string): string | null {
  if (typeof document === "undefined") return null;

  const cookies = document.cookie ? document.cookie.split(/;\s*/) : [];
  for (const cookie of cookies) {
    const [cookieKey, ...cookieValParts] = cookie.split("=");
    if (cookieKey === name) {
      return decodeURIComponent(cookieValParts.join("="));
    }
  }
  return null;
}

/**
 * Sets an authentication cookie with secure defaults (SameSite=Lax, path=/).
 */
export function setAuthCookie(
  name: string,
  value: string,
  maxAgeSeconds = 7 * 24 * 60 * 60
): void {
  if (typeof document === "undefined") return;

  const isSecure =
    typeof window !== "undefined" && window.location.protocol === "https:";
  const secureFlag = isSecure ? "; Secure" : "";

  document.cookie = `${name}=${encodeURIComponent(
    value
  )}; path=/; max-age=${maxAgeSeconds}; SameSite=Lax${secureFlag}`;
}

/**
 * Removes an authentication cookie by expiring it immediately.
 */
export function removeAuthCookie(name: string): void {
  if (typeof document === "undefined") return;
  document.cookie = `${name}=; path=/; max-age=0; SameSite=Lax`;
}
