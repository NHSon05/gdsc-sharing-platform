export const memberManagementKeys = {
  all: ["member-management"] as const,
  generations: (includeInactive = false) =>
    [...memberManagementKeys.all, "generations", { includeInactive }] as const,
  departments: (includeInactive = false) =>
    [...memberManagementKeys.all, "departments", { includeInactive }] as const,
  clubRoles: (includeInactive = false) =>
    [...memberManagementKeys.all, "clubRoles", { includeInactive }] as const,
  memberProfile: (userId: string) =>
    [...memberManagementKeys.all, "memberProfile", userId] as const,
};
