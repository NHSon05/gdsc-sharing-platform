"use client";

import React, { useState, Suspense } from "react";
import { useSearchParams, useRouter, usePathname } from "next/navigation";
import {
  LearningSubNav,
  LearningSidebar,
  QuestionCard,
  QuestionDetailModal,
  LearningSkeleton,
  LearningEmptyState,
  useInterviewQuestionsQuery,
  type InterviewQuestionSummary,
} from "@/features/learning";
import { Button } from "@/components/ui/button";
import { AlertCircle, ChevronLeft, ChevronRight } from "lucide-react";
import { useTranslation } from "@/core/i18n/i18n.context";

function LearningPageContent() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const pathname = usePathname();
  const { t } = useTranslation();

  // Search & Filter State from URL or local
  const currentSearch = searchParams.get("search") || "";
  const currentDepartment = searchParams.get("department") || undefined;
  const currentTopic = searchParams.get("topic") || undefined;
  const currentPage = Math.max(
    1,
    parseInt(searchParams.get("page") || "1", 10)
  );
  const pageSize = Math.min(
    100,
    Math.max(1, parseInt(searchParams.get("pageSize") || "100", 10))
  );

  const [activeCategoryLabel, setActiveCategoryLabel] = useState<string | null>(
    null
  );

  // Modal Detail State
  const [modalState, setModalState] = useState<{
    open: boolean;
    item: InterviewQuestionSummary | null;
    indexNumber?: number;
  }>({
    open: false,
    item: null,
  });

  // Query total count of all questions in database from API (no filter)
  const { data: overallData } = useInterviewQuestionsQuery({
    page: 1,
    pageSize: 1,
  });

  // Query interview questions for active filters
  const { data, isLoading, isError, error, refetch } =
    useInterviewQuestionsQuery({
      page: currentPage,
      pageSize,
      search: currentSearch.trim() || undefined,
      department: currentDepartment,
      topic: currentTopic,
    });

  const updateQueryParams = (newParams: Record<string, string | undefined>) => {
    const params = new URLSearchParams(searchParams.toString());
    Object.entries(newParams).forEach(([key, val]) => {
      if (val) {
        params.set(key, val);
      } else {
        params.delete(key);
      }
    });
    router.replace(`${pathname}?${params.toString()}`);
  };

  const handleSearchChange = (value: string) => {
    updateQueryParams({ search: value || undefined, page: "1" });
  };

  const handleSelectCategory = (cat: {
    department?: string;
    topic?: string;
    label: string;
  }) => {
    setActiveCategoryLabel(cat.label);
    updateQueryParams({
      department: cat.department,
      topic: cat.topic,
      page: "1",
    });
  };

  const handleResetFilters = () => {
    setActiveCategoryLabel(null);
    router.replace(pathname);
  };

  const handleOpenModal = (
    item: InterviewQuestionSummary,
    indexNumber: number
  ) => {
    setModalState({ open: true, item, indexNumber });
  };

  const items = data?.items || [];
  const total = data?.total ?? 0;
  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  // Determine active category id for sidebar
  const activeCategoryId = currentTopic
    ? `topic-${currentTopic}`
    : currentDepartment
      ? `dept-${currentDepartment}`
      : "all";

  const displayCategoryTitle =
    activeCategoryLabel || t("learning.allCategories");
  const countText =
    total > 0
      ? t("learning.totalQuestions")
          .replace(
            "{range}",
            `${(currentPage - 1) * pageSize + 1} - ${Math.min(currentPage * pageSize, total)}`
          )
          .replace("{total}", String(total))
      : t("learning.totalQuestionsEmpty");

  return (
    <div className="flex min-h-[calc(100vh-4rem)] flex-col bg-neutral-50/50 dark:bg-[#070709]">
      {/* Top Sub-Navigation with Search Bar */}
      <LearningSubNav
        searchTerm={currentSearch}
        onSearchChange={handleSearchChange}
      />

      {/* Main Content Area: Split Sidebar + Question Feed */}
      <div className="mx-auto flex w-full max-w-7xl flex-1">
        {/* Left Categories Sidebar */}
        <LearningSidebar
          activeCategory={activeCategoryId}
          onSelectCategory={handleSelectCategory}
          totalCount={overallData?.total}
          activeCategoryCount={data?.total}
        />

        {/* Right Questions Feed */}
        <main className="flex-1 p-4 sm:p-6 lg:p-8">
          {/* Header row: Category Title & Count */}
          <div className="mb-6 flex flex-wrap items-center justify-between gap-3 border-b border-neutral-400 pb-4 dark:border-zinc-800/80">
            <div>
              <h1 className="text-lg font-bold tracking-tight text-neutral-900 dark:text-white">
                {displayCategoryTitle}
              </h1>
              <p className="text-sm text-neutral-800 dark:text-zinc-400">
                {countText}
              </p>
            </div>

            {/* Level indicator note */}
            <div className="flex items-center gap-4 text-sm text-neutral-800 dark:text-zinc-500">
              <span className="inline-block size-2 rounded-full bg-blue-500" />
              <span>{t("learning.basicLevel")}</span>
              <span className="inline-block size-2 rounded-full bg-amber-500" />
              <span>{t("learning.intermediateLevel")}</span>
              <span className="inline-block size-2 rounded-full bg-rose-500" />
              <span>{t("learning.advancedLevel")}</span>
            </div>
          </div>

          {/* Question List */}
          {isLoading ? (
            <LearningSkeleton />
          ) : isError ? (
            <div className="flex min-h-64 flex-col items-center justify-center rounded-3xl border border-neutral-200 bg-white p-8 text-center dark:border-zinc-800 dark:bg-[#0C0C0E]">
              <AlertCircle className="size-8 text-rose-500" />
              <p className="mt-2 text-sm font-bold text-neutral-800 dark:text-zinc-200">
                {t("learning.loadFailed")}
              </p>
              <p className="mt-1 text-xs text-neutral-500 dark:text-zinc-400">
                {error?.message || t("learning.loadFailedDesc")}
              </p>
              <Button
                variant="brand"
                size="sm"
                onClick={() => refetch()}
                className="mt-4"
              >
                {t("learning.retry")}
              </Button>
            </div>
          ) : items.length === 0 ? (
            <LearningEmptyState
              searchTerm={currentSearch}
              onReset={handleResetFilters}
            />
          ) : (
            <div className="space-y-3">
              {items.map((item, index) => {
                const questionNumber = (currentPage - 1) * pageSize + index + 1;
                return (
                  <QuestionCard
                    key={item.id}
                    item={item}
                    indexNumber={questionNumber}
                    onOpenModal={handleOpenModal}
                  />
                );
              })}

              {/* Pagination Controls */}
              {totalPages > 1 && (
                <div className="mt-8 flex items-center justify-between border-t border-neutral-200/80 pt-4 dark:border-zinc-800/80">
                  <span className="text-xs text-neutral-500 dark:text-zinc-400">
                    {t("learning.pageIndicator")
                      .replace("{page}", String(currentPage))
                      .replace("{totalPages}", String(totalPages))}
                  </span>

                  <div className="flex items-center gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      disabled={currentPage <= 1}
                      onClick={() =>
                        updateQueryParams({ page: String(currentPage - 1) })
                      }
                      className="h-8 text-xs font-semibold"
                    >
                      <ChevronLeft className="mr-1 size-3.5" />
                      <span>{t("learning.previous")}</span>
                    </Button>

                    <Button
                      variant="outline"
                      size="sm"
                      disabled={currentPage >= totalPages}
                      onClick={() =>
                        updateQueryParams({ page: String(currentPage + 1) })
                      }
                      className="h-8 text-xs font-semibold"
                    >
                      <span>{t("learning.next")}</span>
                      <ChevronRight className="ml-1 size-3.5" />
                    </Button>
                  </div>
                </div>
              )}
            </div>
          )}
        </main>
      </div>

      {/* Question Detail Modal */}
      <QuestionDetailModal
        questionId={modalState.item?.id || null}
        questionTitle={modalState.item?.question}
        level={modalState.item?.level}
        indexNumber={modalState.indexNumber}
        open={modalState.open}
        onOpenChange={(open) => setModalState((prev) => ({ ...prev, open }))}
      />
    </div>
  );
}

export default function LearningPage() {
  return (
    <Suspense
      fallback={
        <div className="mx-auto max-w-7xl p-8">
          <LearningSkeleton />
        </div>
      }
    >
      <LearningPageContent />
    </Suspense>
  );
}
