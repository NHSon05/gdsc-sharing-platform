import { getRoleBadgeStyle } from "../components/role-badge-list";
import { hasRole, isAdmin, isMember } from "@/features/auth/utils/rbac";
import type { CurrentUserDto } from "@/features/auth/types/auth.types";
import type { ClubMembershipDto } from "../types/profile.types";

describe("Profile Feature Logic & RBAC Tests", () => {
  describe("getRoleBadgeStyle", () => {
    it("should return amber styles for LEAD role", () => {
      const style = getRoleBadgeStyle("LEAD");
      expect(style).toContain("amber");
    });

    it("should return teal styles for SUBLEAD role", () => {
      const style = getRoleBadgeStyle("SUBLEAD");
      expect(style).toContain("teal");
    });

    it("should return brand styles for CORE TEAM role", () => {
      const style = getRoleBadgeStyle("CORE TEAM");
      expect(style).toContain("brand");
    });

    it("should return neutral styles for generic roles", () => {
      const style = getRoleBadgeStyle("SPEAKER");
      expect(style).toContain("neutral");
    });
  });

  describe("RBAC Multiple Roles", () => {
    const multiRoleUser: CurrentUserDto = {
      id: "u-1",
      displayName: "Admin Member",
      email: "admin@gdsc.dev",
      status: "Active",
      roles: ["Admin", "Member"],
    };

    it("should recognize user as both Admin and Member", () => {
      expect(isAdmin(multiRoleUser)).toBe(true);
      expect(isMember(multiRoleUser)).toBe(true);
      expect(hasRole(multiRoleUser, "Admin")).toBe(true);
      expect(hasRole(multiRoleUser, "Member")).toBe(true);
    });

    it("should handle case-insensitive role matching", () => {
      expect(hasRole(multiRoleUser, "admin")).toBe(true);
      expect(hasRole(multiRoleUser, "ADMIN")).toBe(true);
      expect(hasRole(multiRoleUser, "member")).toBe(true);
    });

    it("should return false for non-existent role", () => {
      expect(hasRole(multiRoleUser, "Guest")).toBe(false);
    });
  });

  describe("Membership Chronological Sorting", () => {
    const memberships: ClubMembershipDto[] = [
      {
        id: "m-1",
        generation: { id: "g-1", number: 1, name: "Gen 1" },
        isActive: false,
        departments: [],
      },
      {
        id: "m-3",
        generation: { id: "g-3", number: 3, name: "Gen 3" },
        isActive: true,
        departments: [],
      },
      {
        id: "m-2",
        generation: { id: "g-2", number: 2, name: "Gen 2" },
        isActive: false,
        departments: [],
      },
    ];

    it("should sort generations in descending order (latest Gen 3 first)", () => {
      const sorted = [...memberships].sort(
        (a, b) => (b.generation?.number || 0) - (a.generation?.number || 0)
      );
      expect(sorted[0].generation.number).toBe(3);
      expect(sorted[1].generation.number).toBe(2);
      expect(sorted[2].generation.number).toBe(1);
    });
  });

  describe("Zustand Session Store & Selectors", () => {
    const mockUser: CurrentUserDto = {
      id: "u-99",
      email: "zustand@gdsc.dev",
      displayName: "Zustand Tester",
      status: "Active",
      roles: ["Admin", "Member"],
    };

    it("should correctly store user in Zustand store", async () => {
      const { useSessionStore } = await import("@/core/session/session.store");
      const { selectCurrentUser, selectRoles } =
        await import("@/core/session/session.selectors");

      useSessionStore.getState().clearSession();
      expect(useSessionStore.getState().user).toBeNull();

      useSessionStore.getState().setUser(mockUser);
      expect(selectCurrentUser(useSessionStore.getState())).toEqual(mockUser);
      expect(selectRoles(useSessionStore.getState())).toEqual([
        "Admin",
        "Member",
      ]);

      useSessionStore.getState().clearSession();
      expect(selectCurrentUser(useSessionStore.getState())).toBeNull();
      expect(selectRoles(useSessionStore.getState())).toEqual([]);
    });
  });
});
