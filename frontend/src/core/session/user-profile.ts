import type { CurrentUserDto } from "./session.types";

/** Validate and select the public profile fields at the backend boundary. */
export function parseUserProfile(value: unknown): CurrentUserDto {
  if (
    !value ||
    typeof value !== "object" ||
    !("id" in value) ||
    typeof value.id !== "string" ||
    !("email" in value) ||
    typeof value.email !== "string" ||
    !("displayName" in value) ||
    typeof value.displayName !== "string" ||
    !("status" in value) ||
    typeof value.status !== "string" ||
    !("roles" in value) ||
    !Array.isArray(value.roles) ||
    !value.roles.every((role: unknown) => typeof role === "string")
  )
    throw new Error("Invalid user profile");
  const department = "department" in value ? value.department : null;
  return {
    id: value.id,
    email: value.email,
    displayName: value.displayName,
    status: value.status,
    roles: value.roles,
    studentCode:
      "studentCode" in value && typeof value.studentCode === "string"
        ? value.studentCode
        : undefined,
    generation:
      "generation" in value && typeof value.generation === "string"
        ? value.generation
        : undefined,
    avatarUrl:
      "avatarUrl" in value && typeof value.avatarUrl === "string"
        ? value.avatarUrl
        : undefined,
    department:
      department &&
      typeof department === "object" &&
      "id" in department &&
      typeof department.id === "string" &&
      "name" in department &&
      typeof department.name === "string"
        ? { id: department.id, name: department.name }
        : undefined,
  };
}
