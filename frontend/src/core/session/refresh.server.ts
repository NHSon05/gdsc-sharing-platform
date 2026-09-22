import "server-only";
import { createHash } from "node:crypto";
import { backendFetch, parseTokenPair, type TokenPair } from "./bff.server";

// Single Next.js process only. Shared atomic coordination is required for replicas.
// Brief result retention makes overlapping requests with the same old cookie idempotent.
const flights = new Map<
  string,
  { expires: number; promise: Promise<TokenPair | null> }
>();
export function refreshSession(token: string): Promise<TokenPair | null> {
  const key = createHash("sha256").update(token).digest("hex");
  for (const [k, entry] of flights)
    if (entry.expires <= Date.now()) flights.delete(k);
  const existing = flights.get(key);
  if (existing) return existing.promise;
  if (flights.size >= 10000)
    return Promise.reject(new Error("Refresh capacity reached"));
  const promise = (async () => {
    const response = await backendFetch("/api/auth/refresh", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken: token }),
    });
    if (response.status === 401 || response.status === 403) return null;
    if (!response.ok) throw new Error("Refresh unavailable");
    return parseTokenPair(await response.json());
  })();
  // Timeout exceeds upstream deadline, so pending rotations cannot be duplicated.
  flights.set(key, { expires: Date.now() + 30000, promise });
  void promise.then(
    () => {
      const entry = flights.get(key);
      if (entry?.promise === promise) entry.expires = Date.now() + 5000;
    },
    () => {
      flights.delete(key);
    }
  );
  return promise;
}
