export const AUTH_COOKIE_NAMES = {
  ACCESS_TOKEN: "accessToken",
  REFRESH_TOKEN: "refreshToken",
} as const;
export const AUTH_COOKIE_MAX_AGE = {
  ACCESS_TOKEN: 15 * 60,
  REFRESH_TOKEN: 7 * 24 * 60 * 60,
} as const;
