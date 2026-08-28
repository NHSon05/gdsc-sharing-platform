export const ENV = {
  API_BASE_URL: process.env.NEXT_PUBLIC_API_URL
    ? process.env.NEXT_PUBLIC_API_URL.replace(/\/+$/, "")
    : "",
} as const;
