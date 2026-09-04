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
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import {
  useDepartmentsQuery,
  useGenerationsQuery,
  useDeactivateDepartmentMutation,
  useActivateDepartmentMutation,
  useDeactivateGenerationMutation,
  DepartmentCrudDialog,
  GenerationCrudDialog,
  MembershipEditorDialog,
} from "@/features/member-management";
import { useCurrentUserQuery } from "@/features/auth";
import { useSessionStore } from "@/core/session/session.store";
import { selectCurrentUser } from "@/core/session/session.selectors";
import type {
  DepartmentDto,
  GenerationDto,
} from "@/features/profile/types/profile.types";
import { useTranslation } from "@/core/i18n/i18n.context";

export default function AdminUsersPage() {
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
  const { data: queriedUser } = useCurrentUserQuery();
  const storeUser = useSessionStore(selectCurrentUser);
  const currentUser = queriedUser || storeUser;

  // Mutations
  const deactivateDeptMutation = useDeactivateDepartmentMutation();
  const activateDeptMutation = useActivateDepartmentMutation();
  const deactivateGenMutation = useDeactivateGenerationMutation();

  const [memberSearch, setMemberSearch] = useState("");

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

  return (
    <div className="animate-in fade-in mx-auto max-w-6xl space-y-8 px-4 py-8 font-sans duration-300 sm:px-6 lg:px-8">
      {/* Page Header */}
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
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="relative max-w-sm flex-1">
              <Search className="absolute top-1/2 left-3 size-4 -translate-y-1/2 text-neutral-400 dark:text-zinc-500" />
              <input
                type="text"
                value={memberSearch}
                onChange={(e) => setMemberSearch(e.target.value)}
                placeholder="Tìm kiếm thành viên theo tên..."
                className="focus:border-brand w-full rounded-2xl border border-neutral-200/90 bg-white py-2 pr-4 pl-9 text-xs text-neutral-900 placeholder:text-neutral-400 focus:outline-hidden dark:border-zinc-800 dark:bg-zinc-900/90 dark:text-zinc-100"
              />
            </div>
          </div>

          {/* Members Table */}
          <div className="overflow-hidden rounded-3xl border border-neutral-200/80 bg-white shadow-xs dark:border-zinc-800/80 dark:bg-[#0C0C0E]">
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead className="border-b border-neutral-100 bg-neutral-50/70 text-[11px] font-semibold text-neutral-500 uppercase dark:border-zinc-800/80 dark:bg-zinc-900/40 dark:text-zinc-400">
                  <tr>
                    <th className="px-6 py-3.5">Thành viên</th>
                    <th className="px-6 py-3.5">Email</th>
                    <th className="px-6 py-3.5">Phòng ban</th>
                    <th className="px-6 py-3.5">Quyền hệ thống</th>
                    <th className="px-6 py-3.5 text-right">Thao tác</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-neutral-100 font-medium dark:divide-zinc-800/80">
                  {currentUser && (
                    <tr className="transition-colors hover:bg-neutral-50/50 dark:hover:bg-zinc-900/30">
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-3">
                          <div className="bg-brand text-brand-foreground flex size-8 items-center justify-center rounded-full text-xs font-bold shadow-2xs">
                            {currentUser.displayName?.charAt(0).toUpperCase() ||
                              "U"}
                          </div>
                          <div>
                            <span className="font-bold text-neutral-900 dark:text-white">
                              {currentUser.displayName}
                            </span>
                            <span className="ml-2 rounded-md bg-neutral-100 px-1.5 py-0.5 text-[10px] text-neutral-600 dark:bg-zinc-800 dark:text-zinc-400">
                              {currentUser.studentCode || "Chưa có MSSV"}
                            </span>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 text-neutral-600 dark:text-zinc-400">
                        {currentUser.email}
                      </td>
                      <td className="px-6 py-4 text-neutral-600 dark:text-zinc-400">
                        {currentUser.department?.name || "Software"}
                      </td>
                      <td className="px-6 py-4">
                        <span className="bg-brand/10 text-brand border-brand/20 rounded-full border px-2.5 py-0.5 text-[10px] font-semibold">
                          {currentUser.roles?.join(", ") || "Admin"}
                        </span>
                      </td>
                      <td className="px-6 py-4 text-right">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() =>
                            handleOpenMembershipEditor({
                              id: currentUser.id,
                              displayName: currentUser.displayName,
                            })
                          }
                          leftIcon={<Edit2 className="size-3" />}
                          className="text-xs"
                        >
                          {t("memberManagement.editMembership")}
                        </Button>
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
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
