import { NextResponse } from "next/server";
import { parseUserProfile } from "@/core/session/user-profile";
import {
  backendFetch,
  clearSession,
  noStore,
  parseTokenPair,
  trustedMutation,
  unavailable,
  writeSession,
} from "@/core/session/bff.server";

export async function POST(request: Request) {
  if (!trustedMutation(request)) return new NextResponse(null, { status: 403 });
  let body: unknown;
  try {
    body = await request.json();
  } catch {
    return new NextResponse(null, { status: 400 });
  }
  try {
    const result = await backendFetch("/api/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });
    if (!result.ok) {
      const body: unknown = await result.json().catch(() => null);
      const traceId =
        body &&
        typeof body === "object" &&
        "traceId" in body &&
        typeof body.traceId === "string"
          ? body.traceId
          : undefined;
      const rawErrors =
        body && typeof body === "object" && "errors" in body
          ? body.errors
          : null;
      const errors: Record<string, string[]> = {};
      if (rawErrors && typeof rawErrors === "object") {
        for (const [key, messages] of Object.entries(rawErrors)) {
          if (
            ["email", "password"].includes(key.toLowerCase()) &&
            Array.isArray(messages) &&
            messages.every((message: unknown) => typeof message === "string")
          )
            errors[key] = messages;
        }
      }
      const response = noStore(
        NextResponse.json(
          { title: "Login failed", traceId, errors },
          { status: result.status >= 400 ? result.status : 502 }
        )
      );
      return result.status === 401 || result.status === 403
        ? clearSession(response)
        : response;
    }
    const data: unknown = await result.json();
    const pair = parseTokenPair(data);
    // Browser receives profile only; tokens never cross the server/client boundary.
    const user =
      data && typeof data === "object" && "user" in data ? data.user : null;
    return writeSession(
      NextResponse.json({ user: parseUserProfile(user) }),
      pair
    );
  } catch {
    return unavailable();
  }
}
