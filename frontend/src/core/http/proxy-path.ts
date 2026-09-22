/** Fail closed before forwarding to a backend which may decode paths differently. */
export function proxyPath(path: string): string | null {
  try {
    const decoded = decodeURIComponent(path);
    // Reject ambiguous separators, nested encodings and normalization changes.
    if (
      /[\\\\%\x00-\x20]/.test(decoded) ||
      /%2f/i.test(path) ||
      decoded.includes("//") ||
      decoded.split("/").some((part) => part === "." || part === "..")
    )
      return null;
    if (
      !/^\/api\/(admin|generations|departments|club-roles|roadmaps|roadmap-categories|roadmap-resources|profile|sharing|v1|interview-questions)(\/|$)/.test(
        decoded
      )
    )
      return null;
    return path.replace(
      /^\/api\/v1\/interview-question(?=\/|$)/,
      "/api/v1/interview-questions"
    );
  } catch {
    return null;
  }
}
