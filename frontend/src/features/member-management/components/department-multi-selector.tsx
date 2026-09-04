"use client";

import React, { useMemo } from "react";
import {
  MultiSelect,
  type MultiSelectOption,
} from "@/components/ui/multi-select";
import { useDepartmentsQuery } from "../hooks/use-lookup-queries";
import { useTranslation } from "@/core/i18n/i18n.context";

export interface DepartmentMultiSelectorProps {
  value: string[];
  onChange: (selectedIds: string[]) => void;
  includeInactive?: boolean;
  disabled?: boolean;
  className?: string;
}

export function DepartmentMultiSelector({
  value,
  onChange,
  includeInactive = false,
  disabled = false,
  className,
}: DepartmentMultiSelectorProps) {
  const { t } = useTranslation();
  const { data: departments = [], isLoading } =
    useDepartmentsQuery(includeInactive);

  const options = useMemo<MultiSelectOption[]>(() => {
    return departments.map((dept) => ({
      value: dept.id,
      label: dept.name,
      description: `#${dept.slug}`,
      color: dept.color || "#2563EB",
    }));
  }, [departments]);

  return (
    <MultiSelect
      options={options}
      value={value}
      onChange={onChange}
      placeholder={
        isLoading ? "Đang tải phòng ban..." : t("memberManagement.selectDept")
      }
      searchPlaceholder="Tìm kiếm phòng ban..."
      disabled={disabled || isLoading}
      className={className}
    />
  );
}
