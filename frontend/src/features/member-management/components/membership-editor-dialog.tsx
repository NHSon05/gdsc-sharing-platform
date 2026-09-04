"use client";

import React, { useState } from "react";
import {
  Plus,
  Trash2,
  Check,
  Star,
  Loader2,
  AlertCircle,
  Shield,
} from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { GenerationSelector } from "./generation-selector";
import { RoleMultiSelector } from "./role-multi-selector";
import { useDepartmentsQuery } from "../hooks/use-lookup-queries";
import {
  useAssignMemberGenerationMutation,
  useAddMemberDepartmentMutation,
  useReplaceMemberRolesMutation,
  useEndDepartmentMembershipMutation,
  useEndClubMembershipMutation,
} from "../hooks/use-admin-membership-mutations";
import { useMemberProfileQuery } from "../hooks/use-lookup-queries";
import { useTranslation } from "@/core/i18n/i18n.context";

export interface MembershipEditorDialogProps {
  userId: string;
  userDisplayName: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function MembershipEditorDialog({
  userId,
  userDisplayName,
  open,
  onOpenChange,
}: MembershipEditorDialogProps) {
  const { t } = useTranslation();

  const { data: profile, isLoading: isProfileLoading } =
    useMemberProfileQuery(userId);
  const { data: departments = [] } = useDepartmentsQuery(false);

  // Mode state for assigning
  const [selectedGenId, setSelectedGenId] = useState("");
  const [assignGenMode, setAssignGenMode] = useState(false);

  // Adding dept state
  const [targetClubMembershipId, setTargetClubMembershipId] = useState<
    string | null
  >(null);
  const [selectedDeptId, setSelectedDeptId] = useState("");
  const [selectedRoleIds, setSelectedRoleIds] = useState<string[]>([]);
  const [isPrimaryDept, setIsPrimaryDept] = useState(false);

  // Editing roles state
  const [editingDeptMembershipId, setEditingDeptMembershipId] = useState<
    string | null
  >(null);
  const [editRoleIds, setEditRoleIds] = useState<string[]>([]);

  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const assignGenMutation = useAssignMemberGenerationMutation(userId);
  const addDeptMutation = useAddMemberDepartmentMutation(userId);
  const replaceRolesMutation = useReplaceMemberRolesMutation(userId);
  const endDeptMutation = useEndDepartmentMembershipMutation(userId);
  const endClubMutation = useEndClubMembershipMutation(userId);

  const handleAssignGen = () => {
    if (!selectedGenId) return;
    setErrorMessage(null);

    assignGenMutation.mutate(
      { generationId: selectedGenId },
      {
        onSuccess: () => {
          setAssignGenMode(false);
          setSelectedGenId("");
        },
        onError: (err) => {
          setErrorMessage(
            err.message || "Không thể gán thành viên vào nhiệm kỳ."
          );
        },
      }
    );
  };

  const handleAddDepartment = (clubMembershipId: string) => {
    if (!selectedDeptId || selectedRoleIds.length === 0) {
      setErrorMessage(
        "Vui lòng chọn phòng ban và ít nhất 1 chức danh/vai trò."
      );
      return;
    }
    setErrorMessage(null);

    addDeptMutation.mutate(
      {
        clubMembershipId,
        request: {
          departmentId: selectedDeptId,
          isPrimary: isPrimaryDept,
          roleIds: selectedRoleIds,
        },
      },
      {
        onSuccess: () => {
          setTargetClubMembershipId(null);
          setSelectedDeptId("");
          setSelectedRoleIds([]);
          setIsPrimaryDept(false);
        },
        onError: (err) => {
          setErrorMessage(
            err.message || "Không thể thêm phòng ban cho thành viên."
          );
        },
      }
    );
  };

  const handleSaveRoles = (deptMembershipId: string) => {
    if (editRoleIds.length === 0) {
      setErrorMessage("Phải có ít nhất một chức danh được gán.");
      return;
    }
    setErrorMessage(null);

    replaceRolesMutation.mutate(
      {
        departmentMembershipId: deptMembershipId,
        request: { roleIds: editRoleIds },
      },
      {
        onSuccess: () => {
          setEditingDeptMembershipId(null);
          setEditRoleIds([]);
        },
        onError: (err) => {
          setErrorMessage(err.message || "Không thể cập nhật chức danh.");
        },
      }
    );
  };

  const isPending =
    assignGenMutation.isPending ||
    addDeptMutation.isPending ||
    replaceRolesMutation.isPending ||
    endDeptMutation.isPending ||
    endClubMutation.isPending;

  const memberships = profile?.memberships || [];

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <div className="flex items-center gap-2">
            <Shield className="text-brand size-5" />
            <DialogTitle>{t("memberManagement.editMembership")}</DialogTitle>
          </div>
          <DialogDescription>
            Quản lý phân bổ nhiệm kỳ, phòng ban và chức danh cho{" "}
            <strong>{userDisplayName}</strong>.
          </DialogDescription>
        </DialogHeader>

        {/* Error message */}
        {errorMessage && (
          <div className="mb-4 flex items-center gap-2 rounded-2xl border border-rose-200 bg-rose-50 p-3 text-xs text-rose-700 dark:border-rose-900/40 dark:bg-rose-950/40 dark:text-rose-400">
            <AlertCircle className="size-4 shrink-0" />
            <span>{errorMessage}</span>
          </div>
        )}

        {isProfileLoading ? (
          <div className="flex items-center justify-center p-8">
            <Loader2 className="text-brand size-6 animate-spin" />
          </div>
        ) : (
          <div className="space-y-6">
            {/* Action Bar: Assign to Gen */}
            <div className="flex items-center justify-between border-b border-neutral-100 pb-4 dark:border-zinc-800/80">
              <span className="text-xs font-bold text-neutral-800 dark:text-zinc-200">
                Danh sách nhiệm kỳ ({memberships.length})
              </span>

              {!assignGenMode && (
                <Button
                  type="button"
                  variant="brand"
                  size="sm"
                  onClick={() => setAssignGenMode(true)}
                  leftIcon={<Plus className="size-3.5" />}
                  className="text-xs font-semibold"
                >
                  {t("memberManagement.addMemberToGen")}
                </Button>
              )}
            </div>

            {/* Gen Assignment Box */}
            {assignGenMode && (
              <div className="border-brand/20 bg-brand/[0.03] dark:border-brand/30 dark:bg-brand/[0.05] space-y-3 rounded-2xl border p-4">
                <h4 className="text-brand text-xs font-bold">
                  Chọn nhiệm kỳ cần gán:
                </h4>
                <GenerationSelector
                  value={selectedGenId}
                  onChange={setSelectedGenId}
                />
                <div className="flex justify-end gap-2">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    disabled={isPending}
                    onClick={() => {
                      setAssignGenMode(false);
                      setSelectedGenId("");
                    }}
                  >
                    {t("memberManagement.cancel")}
                  </Button>
                  <Button
                    type="button"
                    variant="brand"
                    size="sm"
                    disabled={!selectedGenId || isPending}
                    onClick={handleAssignGen}
                    leftIcon={
                      isPending ? (
                        <Loader2 className="size-3.5 animate-spin" />
                      ) : (
                        <Check className="size-3.5" />
                      )
                    }
                  >
                    Gán vào Gen
                  </Button>
                </div>
              </div>
            )}

            {/* List of Memberships */}
            {memberships.length === 0 ? (
              <div className="rounded-2xl border border-dashed border-neutral-200 p-8 text-center text-xs text-neutral-400 dark:border-zinc-800 dark:text-zinc-500">
                Thành viên chưa tham gia nhiệm kỳ Gen nào.
              </div>
            ) : (
              <div className="space-y-4">
                {memberships.map((clubMem) => (
                  <div
                    key={clubMem.id}
                    className="space-y-3 rounded-2xl border border-neutral-200/80 bg-neutral-50/50 p-4 dark:border-zinc-800 dark:bg-zinc-900/40"
                  >
                    {/* Gen Title & End Gen Action */}
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-2">
                        <div className="bg-brand text-brand-foreground flex size-7 items-center justify-center rounded-lg text-xs font-bold shadow-2xs">
                          G{clubMem.generation.number}
                        </div>
                        <span className="text-sm font-bold text-neutral-900 dark:text-white">
                          {clubMem.generation.name ||
                            `Gen ${clubMem.generation.number}`}
                        </span>
                        {clubMem.isActive ? (
                          <span className="rounded-full border border-emerald-500/20 bg-emerald-500/10 px-2 py-0.5 text-[10px] font-semibold text-emerald-600">
                            {t("memberManagement.active")}
                          </span>
                        ) : (
                          <span className="rounded-full border bg-neutral-100 px-2 py-0.5 text-[10px] text-neutral-500">
                            {t("memberManagement.inactive")}
                          </span>
                        )}
                      </div>

                      <div className="flex items-center gap-1.5">
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={() => {
                            setTargetClubMembershipId(clubMem.id);
                            setSelectedDeptId("");
                            setSelectedRoleIds([]);
                          }}
                          leftIcon={<Plus className="size-3" />}
                          className="h-7 px-2.5 text-[11px]"
                        >
                          {t("memberManagement.addDeptMembership")}
                        </Button>

                        {clubMem.isActive && (
                          <button
                            type="button"
                            onClick={() => endClubMutation.mutate(clubMem.id)}
                            title={t("memberManagement.endMembership")}
                            className="p-1 text-neutral-400 transition-colors hover:text-rose-600"
                          >
                            <Trash2 className="size-3.5" />
                          </button>
                        )}
                      </div>
                    </div>

                    {/* Adding Dept into this Gen Box */}
                    {targetClubMembershipId === clubMem.id && (
                      <div className="border-brand/20 space-y-3 rounded-xl border bg-white p-3.5 dark:border-zinc-700 dark:bg-zinc-800/80">
                        <div className="grid grid-cols-1 gap-2.5 sm:grid-cols-2">
                          <div>
                            <label className="mb-1 block text-[11px] font-semibold text-neutral-700 dark:text-zinc-300">
                              {t("memberManagement.selectDept")}
                            </label>
                            <select
                              value={selectedDeptId}
                              onChange={(e) =>
                                setSelectedDeptId(e.target.value)
                              }
                              className="w-full appearance-none rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 text-xs text-neutral-900 focus:outline-hidden dark:border-zinc-700 dark:bg-zinc-900 dark:text-zinc-100"
                            >
                              <option value="">Chọn phòng ban...</option>
                              {departments.map((d) => (
                                <option key={d.id} value={d.id}>
                                  {d.name}
                                </option>
                              ))}
                            </select>
                          </div>

                          <div>
                            <label className="mb-1 block text-[11px] font-semibold text-neutral-700 dark:text-zinc-300">
                              {t("memberManagement.selectRoles")}
                            </label>
                            <RoleMultiSelector
                              value={selectedRoleIds}
                              onChange={setSelectedRoleIds}
                            />
                          </div>
                        </div>

                        <label className="flex cursor-pointer items-center gap-2 text-xs text-neutral-700 dark:text-zinc-300">
                          <input
                            type="checkbox"
                            checked={isPrimaryDept}
                            onChange={(e) => setIsPrimaryDept(e.target.checked)}
                            className="accent-brand size-3.5 rounded-sm"
                          />
                          <span>{t("memberManagement.isPrimaryDept")}</span>
                        </label>

                        <div className="flex justify-end gap-2 pt-1">
                          <Button
                            type="button"
                            variant="outline"
                            size="sm"
                            onClick={() => setTargetClubMembershipId(null)}
                            className="h-7 text-xs"
                          >
                            {t("memberManagement.cancel")}
                          </Button>
                          <Button
                            type="button"
                            variant="brand"
                            size="sm"
                            disabled={
                              !selectedDeptId ||
                              selectedRoleIds.length === 0 ||
                              isPending
                            }
                            onClick={() => handleAddDepartment(clubMem.id)}
                            className="h-7 text-xs"
                          >
                            Lưu phòng ban
                          </Button>
                        </div>
                      </div>
                    )}

                    {/* Departments in this Gen */}
                    <div className="space-y-2">
                      {clubMem.departments.map((deptMem) => (
                        <div
                          key={deptMem.id}
                          className="flex flex-col gap-2 rounded-xl border border-neutral-200/60 bg-white p-3 sm:flex-row sm:items-center sm:justify-between dark:border-zinc-800 dark:bg-zinc-900"
                        >
                          <div className="space-y-1">
                            <div className="flex items-center gap-2">
                              <span
                                className="size-2.5 rounded-full"
                                style={{
                                  backgroundColor:
                                    deptMem.department.color || "#2563EB",
                                }}
                              />
                              <span className="text-xs font-bold text-neutral-900 dark:text-white">
                                {deptMem.department.name}
                              </span>
                              {deptMem.isPrimary && (
                                <span className="bg-brand/10 text-brand inline-flex items-center gap-0.5 rounded-md px-1.5 py-0.5 text-[9px] font-bold">
                                  <Star className="size-2.5 fill-current" />
                                  Primary
                                </span>
                              )}
                            </div>

                            {/* Editing Roles Mode */}
                            {editingDeptMembershipId === deptMem.id ? (
                              <div className="mt-2 space-y-2">
                                <RoleMultiSelector
                                  value={editRoleIds}
                                  onChange={setEditRoleIds}
                                />
                                <div className="flex gap-2">
                                  <Button
                                    type="button"
                                    variant="brand"
                                    size="sm"
                                    onClick={() => handleSaveRoles(deptMem.id)}
                                    className="h-6 px-2 text-[10px]"
                                  >
                                    Lưu vai trò
                                  </Button>
                                  <Button
                                    type="button"
                                    variant="outline"
                                    size="sm"
                                    onClick={() =>
                                      setEditingDeptMembershipId(null)
                                    }
                                    className="h-6 px-2 text-[10px]"
                                  >
                                    Hủy
                                  </Button>
                                </div>
                              </div>
                            ) : (
                              <div className="flex flex-wrap items-center gap-1">
                                {deptMem.roles.map((r) => (
                                  <span
                                    key={r.id || r.code}
                                    className="rounded-md bg-neutral-100 px-2 py-0.5 text-[10px] font-medium text-neutral-600 dark:bg-zinc-800 dark:text-zinc-300"
                                  >
                                    {r.name || r.code}
                                  </span>
                                ))}
                              </div>
                            )}
                          </div>

                          {/* Dept Actions */}
                          <div className="flex items-center gap-1.5 self-end sm:self-center">
                            {editingDeptMembershipId !== deptMem.id && (
                              <Button
                                type="button"
                                variant="outline"
                                size="sm"
                                onClick={() => {
                                  setEditingDeptMembershipId(deptMem.id);
                                  setEditRoleIds(
                                    deptMem.roles.map((r) => r.id)
                                  );
                                }}
                                className="h-6 px-2 text-[10px]"
                              >
                                Đổi vai trò
                              </Button>
                            )}
                            <button
                              type="button"
                              onClick={() => endDeptMutation.mutate(deptMem.id)}
                              title="Gỡ phòng ban"
                              className="p-1 text-neutral-400 transition-colors hover:text-rose-600"
                            >
                              <Trash2 className="size-3.5" />
                            </button>
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        <DialogFooter className="mt-6">
          <Button
            type="button"
            variant="outline"
            size="md"
            onClick={() => onOpenChange(false)}
          >
            Đóng
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
