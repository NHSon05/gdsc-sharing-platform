"use client";

import React, { useMemo } from "react";
import {
  MultiSelect,
  type MultiSelectOption,
} from "@/components/ui/multi-select";
import { useClubRolesQuery } from "../hooks/use-lookup-queries";
import { useTranslation } from "@/core/i18n/i18n.context";

export interface RoleMultiSelectorProps {
  value: string[];
  onChange: (selectedRoleIds: string[]) => void;
  includeInactive?: boolean;
  disabled?: boolean;
  className?: string;
}

export function RoleMultiSelector({
  value,
  onChange,
  includeInactive = false,
  disabled = false,
  className,
}: RoleMultiSelectorProps) {
  const { t } = useTranslation();
  const { data: roles = [], isLoading } = useClubRolesQuery(includeInactive);

  const options = useMemo<MultiSelectOption[]>(() => {
    return roles.map((role) => ({
      value: role.id,
      label: role.name || role.code,
      description: role.description || role.code,
    }));
  }, [roles]);

  return (
    <MultiSelect
      options={options}
      value={value}
      onChange={onChange}
      placeholder={
        isLoading ? "Đang tải vai trò..." : t("memberManagement.selectRoles")
      }
      searchPlaceholder="Tìm kiếm chức danh/vai trò..."
      disabled={disabled || isLoading}
      className={className}
    />
  );
}
