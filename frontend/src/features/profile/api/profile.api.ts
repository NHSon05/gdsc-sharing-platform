import { httpClient } from "@/core/http/http-client";
import type {
  UserProfileDto,
  UpdateProfileRequest,
  UploadAvatarResponse,
  EmailChangeRequest,
  EmailConfirmRequest,
} from "../types/profile.types";

/**
 * GET /api/profile/me - Fetches current user full profile with memberships.
 */
export async function getProfileMeApi(
  signal?: AbortSignal
): Promise<UserProfileDto> {
  const response = await httpClient.get<UserProfileDto>("/api/profile/me", {
    signal,
  });
  return response.data;
}

/**
 * PUT /api/profile/me - Updates personal profile fields.
 */
export async function updateProfileMeApi(
  request: UpdateProfileRequest,
  signal?: AbortSignal
): Promise<UserProfileDto> {
  const response = await httpClient.patch<UserProfileDto>(
    "/api/profile/me",
    request,
    { signal }
  );
  return response.data;
}

/**
 * POST /api/profile/me/avatar - Uploads profile avatar (multipart/form-data).
 */
export async function uploadAvatarApi(
  file: File,
  signal?: AbortSignal
): Promise<UploadAvatarResponse> {
  const formData = new FormData();
  formData.append("avatar", file);

  const response = await httpClient.post<UploadAvatarResponse>(
    "/api/profile/me/avatar",
    formData,
    {
      headers: {
        "Content-Type": "multipart/form-data",
      },
      signal,
    }
  );
  return response.data;
}

/**
 * DELETE /api/profile/me/avatar - Deletes profile avatar.
 */
export async function deleteAvatarApi(signal?: AbortSignal): Promise<void> {
  await httpClient.delete("/api/profile/me/avatar", { signal });
}

/**
 * POST /api/profile/me/email/change-request - Requests email change.
 */
export async function requestEmailChangeApi(
  request: EmailChangeRequest,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.post("/api/profile/me/email/change-request", request, {
    signal,
  });
}

/**
 * POST /api/profile/email/confirm - Confirms email change.
 */
export async function confirmEmailChangeApi(
  request: EmailConfirmRequest,
  signal?: AbortSignal
): Promise<void> {
  await httpClient.post("/api/profile/email/confirm", request, { signal });
}
