/** UX navigation only; backend remains responsible for authorization. */
export function safeReturnPath(value: string | null): string {
  if (
    !value ||
    !value.startsWith("/") ||
    value.startsWith("//") ||
    /[\\\x00-\x20]/.test(value)
  )
    return "/";
  try {
    const base = "https://app.invalid";
    const url = new URL(value, base);
    const decodedPath = decodeURIComponent(url.pathname);
    if (
      url.origin !== base ||
      decodedPath.includes("%") ||
      decodedPath.includes("\\") ||
      decodedPath.startsWith("//") ||
      /[\x00-\x1f]/.test(decodedPath) ||
      /^\/(api|login|register)(\/|$)/i.test(decodedPath)
    )
      return "/";
    return url.pathname + url.search + url.hash;
  } catch {
    return "/";
  }
}
