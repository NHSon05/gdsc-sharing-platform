"use client";

import React, { useState } from "react";
import {
  Users,
  Building2,
  Layers,
  Plus,
  Edit2,
  Trash2,
  CheckCircle2,
  XCircle,
  Shield,
  Search,
  ChevronLeft,
  ChevronRight,
  UserCheck,
  UserX,
  Loader2,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { UserAvatar } from "@/components/ui/user-avatar";
import {
  useDepartmentsQuery,
  useGenerationsQuery,
  useDeactivateDepartmentMutation,
  useActivateDepartmentMutation,
  useDeactivateGenerationMutation,
  DepartmentCrudDialog,
  GenerationCrudDialog,
  MembershipEditorDialog,
  useAdminMembersQuery,
  useUpdateMemberStatusMutation,
  useUpdateMemberSystemRolesMutation,
} from "@/features/member-management";
import type {
  DepartmentDto,
  GenerationDto,
} from "@/features/profile/types/profile.types";
import { useTranslation } from "@/core/i18n/i18n.context";

export interface AdminUsersViewProps {
  showHeader?: boolean;
}

export function AdminUsersView({ showHeader = false }: AdminUsersViewProps) {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState("members");

  // Dialog states
  const [deptModalOpen, setDeptModalOpen] = useState(false);
  const [editingDept, setEditingDept] = useState<DepartmentDto | null>(null);

  const [genModalOpen, setGenModalOpen] = useState(false);
  const [editingGen, setEditingGen] = useState<GenerationDto | null>(null);

  const [membershipModalOpen, setMembershipModalOpen] = useState(false);
  const [selectedMember, setSelectedMember] = useState<{
    id: string;
    displayName: string;
  } | null>(null);

  // Queries (fetch with includeInactive = true for Admin)
  const { data: departments = [] } = useDepartmentsQuery(true);
  const { data: generations = [] } = useGenerationsQuery(true);

  // Member management state
  const [memberSearch, setMemberSearch] = useState("");
  const [selectedDeptFilter, setSelectedDeptFilter] = useState("");
  const [selectedGenFilter, setSelectedGenFilter] = useState("");
  const [selectedStatusFilter, setSelectedStatusFilter] = useState("");
  const [selectedRoleFilter, setSelectedRoleFilter] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 15;

  const { data: membersData, isLoading: isMembersLoading } =
    useAdminMembersQuery({
      search: memberSearch.trim() || undefined,
      departmentId: selectedDeptFilter || undefined,
      generationId: selectedGenFilter || undefined,
      status: selectedStatusFilter ? Number(selectedStatusFilter) : undefined,
      systemRole: selectedRoleFilter || undefined,
      page,
      pageSize,
    });

  // Mutations
  const deactivateDeptMutation = useDeactivateDepartmentMutation();
  const activateDeptMutation = useActivateDepartmentMutation();
  const deactivateGenMutation = useDeactivateGenerationMutation();
  const updateStatusMutation = useUpdateMemberStatusMutation();
  const updateRolesMutation = useUpdateMemberSystemRolesMutation();

  const handleOpenEditDept = (dept: DepartmentDto) => {
    setEditingDept(dept);
    setDeptModalOpen(true);
  };

  const handleOpenCreateDept = () => {
    setEditingDept(null);
    setDeptModalOpen(true);
  };

  const handleOpenEditGen = (gen: GenerationDto) => {
    setEditingGen(gen);
    setGenModalOpen(true);
  };

  const handleOpenCreateGen = () => {
    setEditingGen(null);
    setGenModalOpen(true);
  };

  const handleOpenMembershipEditor = (member: {
    id: string;
    displayName: string;
  }) => {
    setSelectedMember(member);
    setMembershipModalOpen(true);
  };

  const handleToggleAdminRole = async (
    userId: string,
    currentRoles: string[]
  ) => {
    const isAdmin = currentRoles.includes("Admin");
    const nextRoles = isAdmin
      ? currentRoles.filter((r) => r !== "Admin")
      : [...currentRoles, "Admin"];

    if (
      isAdmin &&
      !window.confirm(
        "Bạn có chắc muốn hủy quyền Quản trị viên (Admin) của người dùng này?"
      )
    ) {
      return;
    }

    await updateRolesMutation.mutateAsync({ userId, roles: nextRoles });
  };

  const handleToggleStatus = async (
    userId: string,
    currentStatus: number | string
  ) => {
    const isBanned = currentStatus === 3 || currentStatus === "Banned";
    const nextStatus = isBanned ? 1 : 3; // 1: Active, 3: Banned
    const message = isBanned
      ? "Bạn có chắc muốn mở khóa tài khoản này?"
      : "Bạn có chắc muốn tạm khóa tài khoản này?";

    if (!window.confirm(message)) return;

    await updateStatusMutation.mutateAsync({ userId, status: nextStatus });
  };

  return (
    <div className="space-y-6">
      {/* Page Header (optional) */}
      {showHeader && (
        <div className="flex flex-col gap-2 border-b border-neutral-200/80 pb-6 dark:border-zinc-800/80">
          <div className="text-brand flex items-center gap-2 text-xs font-semibold tracking-wider uppercase">
            <Shield className="size-4" />
            <span>Quản trị hệ thống</span>
          </div>
          <h1 className="text-2xl font-bold tracking-tight text-neutral-900 sm:text-3xl dark:text-white">
            {t("memberManagement.title")}
          </h1>
          <p className="text-sm text-neutral-500 dark:text-zinc-400">
            {t("memberManagement.subtitle")}
          </p>
        </div>
      )}

      {/* Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="mb-6">
          <TabsTrigger value="members" icon={<Users className="size-4" />}>
            {t("memberManagement.tabMembers")}
          </TabsTrigger>
          <TabsTrigger
            value="departments"
            icon={<Building2 className="size-4" />}
          >
            {t("memberManagement.tabDepartments")}
          </TabsTrigger>
          <TabsTrigger value="generations" icon={<Layers className="size-4" />}>
            {t("memberManagement.tabGenerations")}
          </TabsTrigger>
        </TabsList>

        {/* =================================================================
            TAB 1: MEMBERS & MEMBERSHIP ASSIGNMENTS
            ================================================================= */}
        <TabsContent value="members" className="space-y-6">
          {/* Filters Bar */}
          <div className="flex flex-col gap-3 rounded-2xl border border-neutral-200/80 bg-white p-4 shadow-2xs sm:flex-row sm:items-center sm:justify-between dark:border-zinc-800/80 dark:bg-zinc-900/60">
            <div className="relative max-w-sm flex-1">
              <Search className="absolute top-1/2 left-3 size-4 -translate-y-1/2 text-neutral-400 dark:text-zinc-500" />
              <input
                type="text"
                value={memberSearch}
                onChange={(e) => {
                  setMemberSearch(e.target.value);
                  setPage(1);
                }}
                placeholder="Tìm kiếm theo tên, email, MSSV..."
                className="focus:border-brand w-full rounded-xl border border-neutral-200/90 bg-neutral-50/50 py-2 pr-4 pl-9 text-xs text-neutral-900 placeholder:text-neutral-400 focus:bg-white focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900/90 dark:text-zinc-100"
              />
            </div>

            <div className="flex flex-wrap items-center gap-2">
              {/* Department Filter */}
              <select
                value={selectedDeptFilter}
                onChange={(e) => {
                  setSelectedDeptFilter(e.target.value);
                  setPage(1);
                }}
                className="rounded-xl border border-neutral-200/90 bg-white px-3 py-2 text-xs font-medium text-neutral-700 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-300"
              >
                <option value="">Tất cả phòng ban</option>
                {departments.map((dept) => (
                  <option key={dept.id} value={dept.id}>
                    {dept.name}
                  </option>
                ))}
              </select>

              {/* Generation Filter */}
              <select
                value={selectedGenFilter}
                onChange={(e) => {
                  setSelectedGenFilter(e.target.value);
                  setPage(1);
                }}
                className="rounded-xl border border-neutral-200/90 bg-white px-3 py-2 text-xs font-medium text-neutral-700 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-300"
              >
                <option value="">Tất cả nhiệm kỳ (Gen)</option>
                {generations.map((gen) => (
                  <option key={gen.id} value={gen.id}>
                    Gen {gen.number} {gen.name ? `(${gen.name})` : ""}
                  </option>
                ))}
              </select>

              {/* System Role Filter */}
              <select
                value={selectedRoleFilter}
                onChange={(e) => {
                  setSelectedRoleFilter(e.target.value);
                  setPage(1);
                }}
                className="rounded-xl border border-neutral-200/90 bg-white px-3 py-2 text-xs font-medium text-neutral-700 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-300"
              >
                <option value="">Mọi quyền hệ thống</option>
                <option value="Admin">Quản trị viên (Admin)</option>
                <option value="Member">Thành viên</option>
              </select>

              {/* Status Filter */}
              <select
                value={selectedStatusFilter}
                onChange={(e) => {
                  setSelectedStatusFilter(e.target.value);
                  setPage(1);
                }}
                className="rounded-xl border border-neutral-200/90 bg-white px-3 py-2 text-xs font-medium text-neutral-700 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-300"
              >
                <option value="">Mọi trạng thái</option>
                <option value="1">Đang hoạt động</option>
                <option value="3">Tạm khóa / Bị cấm</option>
              </select>
            </div>
          </div>

          {/* Members Table */}
          <div className="overflow-hidden rounded-3xl border border-neutral-200/80 bg-white shadow-xs dark:border-zinc-800/80 dark:bg-[#0C0C0E]">
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead className="border-b border-neutral-100 bg-neutral-50/70 text-[11px] font-semibold text-neutral-500 uppercase dark:border-zinc-800/80 dark:bg-zinc-900/40 dark:text-zinc-400">
                  <tr>
                    <th className="px-6 py-3.5">Thành viên</th>
                    <th className="px-6 py-3.5">Email & Liên hệ</th>
                    <th className="px-6 py-3.5">Nhiệm kỳ & Ban</th>
                    <th className="px-6 py-3.5">Quyền hệ thống</th>
                    <th className="px-6 py-3.5">Trạng thái</th>
                    <th className="px-6 py-3.5 text-right">Thao tác</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-neutral-100 font-medium dark:divide-zinc-800/80">
                  {isMembersLoading ? (
                    <tr>
                      <td colSpan={6} className="py-16 text-center">
                        <Loader2 className="text-brand mx-auto size-7 animate-spin" />
                        <span className="mt-2 block text-xs text-neutral-500">
                          Đang tải danh sách thành viên...
                        </span>
                      </td>
                    </tr>
                  ) : !membersData?.items || membersData.items.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="py-16 text-center">
                        <Users className="mx-auto size-10 text-neutral-300 dark:text-zinc-600" />
                        <p className="mt-2 text-sm font-semibold text-neutral-700 dark:text-zinc-300">
                          Không tìm thấy thành viên phù hợp
                        </p>
                        <p className="text-xs text-neutral-500 dark:text-zinc-500">
                          Thử thay đổi bộ lọc tìm kiếm hoặc từ khóa.
                        </p>
                      </td>
                    </tr>
                  ) : (
                    membersData.items.map((member) => {
                      const isAdmin = member.systemRoles.includes("Admin");
                      const isBanned =
                        member.status === 3 || member.status === "Banned";
                      const displayName =
                        member.displayName || member.fullName || "User";

                      return (
                        <tr
                          key={member.id}
                          className="transition-colors hover:bg-neutral-50/50 dark:hover:bg-zinc-900/30"
                        >
                          {/* Member column */}
                          <td className="px-6 py-4">
                            <div className="flex items-center gap-3">
                              <UserAvatar
                                name={displayName}
                                avatarUrl={member.avatarUrl}
                                size="sm"
                                isAdmin={isAdmin}
                                showAdminBadge={isAdmin}
                              />
                              <div>
                                <div className="flex items-center gap-1.5">
                                  <span className="font-bold text-neutral-900 dark:text-white">
                                    {displayName}
                                  </span>
                                  {member.studentCode && (
                                    <span className="rounded-md bg-neutral-100 px-1.5 py-0.5 text-[10px] font-semibold text-neutral-600 dark:bg-zinc-800 dark:text-zinc-400">
                                      {member.studentCode}
                                    </span>
                                  )}
                                </div>
                                {member.fullName &&
                                  member.fullName !== displayName && (
                                    <div className="text-[11px] text-neutral-500 dark:text-zinc-400">
                                      {member.fullName}
                                    </div>
                                  )}
                              </div>
                            </div>
                          </td>

                          {/* Email column */}
                          <td className="px-6 py-4 text-neutral-600 dark:text-zinc-400">
                            {member.email}
                          </td>

                          {/* Gen & Depts column */}
                          <td className="px-6 py-4">
                            <div className="flex flex-wrap items-center gap-1.5">
                              {member.generationNumbers.length > 0 ? (
                                member.generationNumbers.map((genNum) => (
                                  <span
                                    key={genNum}
                                    className="rounded-md border border-neutral-200 bg-neutral-50 px-1.5 py-0.5 text-[10px] font-semibold text-neutral-700 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-300"
                                  >
                                    Gen {genNum}
                                  </span>
                                ))
                              ) : (
                                <span className="text-[11px] text-neutral-400 italic">
                                  Chưa gán Gen
                                </span>
                              )}

                              {member.departmentNames.map((deptName) => (
                                <span
                                  key={deptName}
                                  className="rounded-md border border-blue-200/60 bg-blue-50/70 px-1.5 py-0.5 text-[10px] font-semibold text-blue-700 dark:border-blue-900/40 dark:bg-blue-950/40 dark:text-blue-300"
                                >
                                  {deptName}
                                </span>
                              ))}
                            </div>
                          </td>

                          {/* System role column */}
                          <td className="px-6 py-4">
                            {isAdmin ? (
                              <span className="inline-flex items-center gap-1 rounded-full border border-rose-200 bg-rose-50 px-2.5 py-0.5 text-[10px] font-bold text-rose-700 dark:border-rose-900/50 dark:bg-rose-950/50 dark:text-rose-300">
                                <Shield className="size-3" />
                                <span>Admin</span>
                              </span>
                            ) : (
                              <span className="rounded-full border border-neutral-200 bg-neutral-100 px-2.5 py-0.5 text-[10px] font-semibold text-neutral-700 dark:border-zinc-800 dark:bg-zinc-800 dark:text-zinc-300">
                                Member
                              </span>
                            )}
                          </td>

                          {/* Status column */}
                          <td className="px-6 py-4">
                            {isBanned ? (
                              <span className="inline-flex items-center gap-1 rounded-full border border-rose-200 bg-rose-50 px-2 py-0.5 text-[10px] font-semibold text-rose-700 dark:border-rose-900/50 dark:bg-rose-950/50 dark:text-rose-300">
                                <UserX className="size-3" />
                                <span>Tạm khóa</span>
                              </span>
                            ) : (
                              <span className="inline-flex items-center gap-1 rounded-full border border-emerald-200 bg-emerald-50 px-2 py-0.5 text-[10px] font-semibold text-emerald-700 dark:border-emerald-900/50 dark:bg-emerald-950/50 dark:text-emerald-300">
                                <UserCheck className="size-3" />
                                <span>Hoạt động</span>
                              </span>
                            )}
                          </td>

                          {/* Actions column */}
                          <td className="px-6 py-4 text-right">
                            <div className="flex items-center justify-end gap-1.5">
                              {/* Edit Membership */}
                              <Button
                                variant="outline"
                                size="sm"
                                onClick={() =>
                                  handleOpenMembershipEditor({
                                    id: member.id,
                                    displayName,
                                  })
                                }
                                leftIcon={<Edit2 className="size-3" />}
                                className="text-xs"
                              >
                                {t("memberManagement.editMembership")}
                              </Button>

                              {/* Toggle Admin */}
                              <button
                                type="button"
                                onClick={() =>
                                  handleToggleAdminRole(
                                    member.id,
                                    member.systemRoles
                                  )
                                }
                                title={
                                  isAdmin
                                    ? "Hủy quyền Quản trị viên"
                                    : "Cấp quyền Quản trị viên"
                                }
                                className={`flex size-8 cursor-pointer items-center justify-center rounded-lg border transition-colors ${
                                  isAdmin
                                    ? "border-rose-200 bg-rose-50 text-rose-600 hover:bg-rose-100 dark:border-rose-900/50 dark:bg-rose-950/30 dark:text-rose-400"
                                    : "border-neutral-200 bg-white text-neutral-600 hover:bg-neutral-100 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-400"
                                }`}
                              >
                                <Shield className="size-3.5" />
                              </button>

                              {/* Toggle Status */}
                              <button
                                type="button"
                                onClick={() =>
                                  handleToggleStatus(member.id, member.status)
                                }
                                title={
                                  isBanned
                                    ? "Mở khóa tài khoản"
                                    : "Tạm khóa tài khoản"
                                }
                                className={`flex size-8 cursor-pointer items-center justify-center rounded-lg border transition-colors ${
                                  isBanned
                                    ? "border-emerald-200 bg-emerald-50 text-emerald-600 hover:bg-emerald-100 dark:border-emerald-900/50 dark:bg-emerald-950/30 dark:text-emerald-400"
                                    : "border-neutral-200 bg-white text-neutral-600 hover:bg-rose-50 hover:text-rose-600 dark:border-zinc-800 dark:bg-zinc-900 dark:text-zinc-400"
                                }`}
                              >
                                <UserCheck className="size-3.5" />
                              </button>
                            </div>
                          </td>
                        </tr>
                      );
                    })
                  )}
                </tbody>
              </table>
            </div>

            {/* Pagination footer */}
            {membersData && membersData.totalCount > 0 && (
              <div className="flex flex-col items-center justify-between gap-3 border-t border-neutral-100 bg-neutral-50/50 px-6 py-3.5 sm:flex-row dark:border-zinc-800/80 dark:bg-zinc-900/30">
                <span className="text-xs text-neutral-500 dark:text-zinc-400">
                  Hiển thị{" "}
                  <span className="font-semibold text-neutral-900 dark:text-white">
                    {(membersData.page - 1) * membersData.pageSize + 1} -{" "}
                    {Math.min(
                      membersData.page * membersData.pageSize,
                      membersData.totalCount
                    )}
                  </span>{" "}
                  trên tổng số{" "}
                  <span className="font-semibold text-neutral-900 dark:text-white">
                    {membersData.totalCount}
                  </span>{" "}
                  thành viên
                </span>

                <div className="flex items-center gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={page <= 1}
                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                    leftIcon={<ChevronLeft className="size-3.5" />}
                    className="h-8 text-xs"
                  >
                    Trước
                  </Button>
                  <span className="px-2 text-xs font-semibold text-neutral-700 dark:text-zinc-300">
                    Trang {page} /{" "}
                    {Math.max(
                      1,
                      Math.ceil(membersData.totalCount / membersData.pageSize)
                    )}
                  </span>
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={
                      page >=
                      Math.ceil(membersData.totalCount / membersData.pageSize)
                    }
                    onClick={() => setPage((p) => p + 1)}
                    rightIcon={<ChevronRight className="size-3.5" />}
                    className="h-8 text-xs"
                  >
                    Sau
                  </Button>
                </div>
              </div>
            )}
          </div>
        </TabsContent>

        {/* =================================================================
            TAB 2: DEPARTMENTS CRUD
            ================================================================= */}
        <TabsContent value="departments" className="space-y-6">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold text-neutral-700 dark:text-zinc-300">
              Tổng số phòng ban: {departments.length}
            </span>
            <Button
              variant="brand"
              size="sm"
              onClick={handleOpenCreateDept}
              leftIcon={<Plus className="size-4" />}
              className="font-semibold shadow-xs"
            >
              {t("memberManagement.createDepartment")}
            </Button>
          </div>

          {/* Departments Table */}
          <div className="overflow-hidden rounded-3xl border border-neutral-200/80 bg-white shadow-xs dark:border-zinc-800/80 dark:bg-[#0C0C0E]">
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead className="border-b border-neutral-100 bg-neutral-50/70 text-[11px] font-semibold text-neutral-500 uppercase dark:border-zinc-800/80 dark:bg-zinc-900/40 dark:text-zinc-400">
                  <tr>
                    <th className="px-6 py-3.5">Phòng ban</th>
                    <th className="px-6 py-3.5">Slug</th>
                    <th className="px-6 py-3.5">Mô tả</th>
                    <th className="px-6 py-3.5">Thứ tự</th>
                    <th className="px-6 py-3.5">Trạng thái</th>
                    <th className="px-6 py-3.5 text-right">Thao tác</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-neutral-100 font-medium dark:divide-zinc-800/80">
                  {departments.map((dept) => (
                    <tr
                      key={dept.id}
                      className="transition-colors hover:bg-neutral-50/50 dark:hover:bg-zinc-900/30"
                    >
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-2.5">
                          <span
                            className="size-3 shrink-0 rounded-full shadow-2xs"
                            style={{ backgroundColor: dept.color || "#2563EB" }}
                          />
                          <span className="font-bold text-neutral-900 dark:text-white">
                            {dept.name}
                          </span>
                        </div>
                      </td>
                      <td className="px-6 py-4 font-mono text-neutral-500 dark:text-zinc-400">
                        #{dept.slug}
                      </td>
                      <td className="max-w-xs truncate px-6 py-4 text-neutral-500 dark:text-zinc-400">
                        {dept.description || "-"}
                      </td>
                      <td className="px-6 py-4 text-neutral-600 dark:text-zinc-400">
                        {dept.sortOrder ?? 0}
                      </td>
                      <td className="px-6 py-4">
                        {dept.isActive !== false ? (
                          <span className="inline-flex items-center gap-1 rounded-full border border-emerald-500/20 bg-emerald-500/10 px-2 py-0.5 text-[10px] font-semibold text-emerald-600">
                            <CheckCircle2 className="size-3" />
                            {t("memberManagement.active")}
                          </span>
                        ) : (
                          <span className="inline-flex items-center gap-1 rounded-full border border-neutral-200 bg-neutral-100 px-2 py-0.5 text-[10px] font-medium text-neutral-500 dark:bg-zinc-800 dark:text-zinc-400">
                            <XCircle className="size-3" />
                            {t("memberManagement.inactive")}
                          </span>
                        )}
                      </td>
                      <td className="px-6 py-4 text-right">
                        <div className="flex items-center justify-end gap-1.5">
                          <button
                            type="button"
                            onClick={() => handleOpenEditDept(dept)}
                            title={t("memberManagement.editDepartment")}
                            className="rounded-lg p-1.5 text-neutral-500 transition-colors hover:bg-neutral-100 hover:text-neutral-900 dark:hover:bg-zinc-800 dark:hover:text-white"
                          >
                            <Edit2 className="size-3.5" />
                          </button>
                          {dept.isActive !== false ? (
                            <button
                              type="button"
                              onClick={() =>
                                deactivateDeptMutation.mutate(dept.id)
                              }
                              title={t("memberManagement.deactivateDepartment")}
                              className="rounded-lg p-1.5 text-neutral-400 transition-colors hover:bg-rose-50 hover:text-rose-600 dark:hover:bg-rose-950/30"
                            >
                              <Trash2 className="size-3.5" />
                            </button>
                          ) : (
                            <button
                              type="button"
                              onClick={() =>
                                activateDeptMutation.mutate(dept.id)
                              }
                              title={t("memberManagement.activateDepartment")}
                              className="rounded-lg p-1.5 text-neutral-400 transition-colors hover:bg-emerald-50 hover:text-emerald-600 dark:hover:bg-emerald-950/30"
                            >
                              <CheckCircle2 className="size-3.5" />
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </TabsContent>

        {/* =================================================================
            TAB 3: GENERATIONS CRUD
            ================================================================= */}
        <TabsContent value="generations" className="space-y-6">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold text-neutral-700 dark:text-zinc-300">
              Tổng số nhiệm kỳ: {generations.length}
            </span>
            <Button
              variant="brand"
              size="sm"
              onClick={handleOpenCreateGen}
              leftIcon={<Plus className="size-4" />}
              className="font-semibold shadow-xs"
            >
              {t("memberManagement.createGeneration")}
            </Button>
          </div>

          {/* Generations Table */}
          <div className="overflow-hidden rounded-3xl border border-neutral-200/80 bg-white shadow-xs dark:border-zinc-800/80 dark:bg-[#0C0C0E]">
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead className="border-b border-neutral-100 bg-neutral-50/70 text-[11px] font-semibold text-neutral-500 uppercase dark:border-zinc-800/80 dark:bg-zinc-900/40 dark:text-zinc-400">
                  <tr>
                    <th className="px-6 py-3.5">Nhiệm kỳ</th>
                    <th className="px-6 py-3.5">Ngày bắt đầu</th>
                    <th className="px-6 py-3.5">Ngày kết thúc</th>
                    <th className="px-6 py-3.5">Trạng thái</th>
                    <th className="px-6 py-3.5 text-right">Thao tác</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-neutral-100 font-medium dark:divide-zinc-800/80">
                  {generations.map((gen) => (
                    <tr
                      key={gen.id}
                      className="transition-colors hover:bg-neutral-50/50 dark:hover:bg-zinc-900/30"
                    >
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-2.5">
                          <div className="bg-brand text-brand-foreground flex size-7 items-center justify-center rounded-lg text-xs font-bold shadow-2xs">
                            G{gen.number}
                          </div>
                          <span className="font-bold text-neutral-900 dark:text-white">
                            {gen.name || `Gen ${gen.number}`}
                          </span>
                        </div>
                      </td>
                      <td className="px-6 py-4 text-neutral-600 dark:text-zinc-400">
                        {gen.startDate ? gen.startDate.split("T")[0] : "-"}
                      </td>
                      <td className="px-6 py-4 text-neutral-600 dark:text-zinc-400">
                        {gen.endDate ? gen.endDate.split("T")[0] : "-"}
                      </td>
                      <td className="px-6 py-4">
                        {gen.isActive !== false ? (
                          <span className="inline-flex items-center gap-1 rounded-full border border-emerald-500/20 bg-emerald-500/10 px-2 py-0.5 text-[10px] font-semibold text-emerald-600">
                            <CheckCircle2 className="size-3" />
                            {t("memberManagement.active")}
                          </span>
                        ) : (
                          <span className="inline-flex items-center gap-1 rounded-full border border-neutral-200 bg-neutral-100 px-2 py-0.5 text-[10px] font-medium text-neutral-500 dark:bg-zinc-800 dark:text-zinc-400">
                            <XCircle className="size-3" />
                            {t("memberManagement.inactive")}
                          </span>
                        )}
                      </td>
                      <td className="px-6 py-4 text-right">
                        <div className="flex items-center justify-end gap-1.5">
                          <button
                            type="button"
                            onClick={() => handleOpenEditGen(gen)}
                            title={t("memberManagement.editGeneration")}
                            className="rounded-lg p-1.5 text-neutral-500 transition-colors hover:bg-neutral-100 hover:text-neutral-900 dark:hover:bg-zinc-800 dark:hover:text-white"
                          >
                            <Edit2 className="size-3.5" />
                          </button>
                          {gen.isActive !== false && (
                            <button
                              type="button"
                              onClick={() =>
                                deactivateGenMutation.mutate(gen.id)
                              }
                              title={t("memberManagement.deactivateGeneration")}
                              className="rounded-lg p-1.5 text-neutral-400 transition-colors hover:bg-rose-50 hover:text-rose-600 dark:hover:bg-rose-950/30"
                            >
                              <Trash2 className="size-3.5" />
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </TabsContent>
      </Tabs>

      {/* Dialogs */}
      <DepartmentCrudDialog
        department={editingDept}
        open={deptModalOpen}
        onOpenChange={setDeptModalOpen}
      />

      <GenerationCrudDialog
        generation={editingGen}
        open={genModalOpen}
        onOpenChange={setGenModalOpen}
      />

      {selectedMember && (
        <MembershipEditorDialog
          userId={selectedMember.id}
          userDisplayName={selectedMember.displayName}
          open={membershipModalOpen}
          onOpenChange={setMembershipModalOpen}
        />
      )}
    </div>
  );
}
