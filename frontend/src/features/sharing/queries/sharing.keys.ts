import type {
  ContentQueryParams,
  ScheduleQueryParams,
} from "../types/sharing.types";

export const sharingKeys = {
  all: ["sharing"] as const,

  contents: {
    all: () => [...sharingKeys.all, "contents"] as const,
    lists: () => [...sharingKeys.contents.all(), "list"] as const,
    list: (params?: ContentQueryParams) =>
      [...sharingKeys.contents.lists(), params ?? {}] as const,
    infiniteLists: () =>
      [...sharingKeys.contents.all(), "infinite-list"] as const,
    infiniteList: (params?: ContentQueryParams) =>
      [...sharingKeys.contents.infiniteLists(), params ?? {}] as const,
    mineLists: () => [...sharingKeys.contents.all(), "mine"] as const,
    mineList: (params?: ContentQueryParams) =>
      [...sharingKeys.contents.mineLists(), params ?? {}] as const,
    adminLists: () => [...sharingKeys.contents.all(), "admin"] as const,
    adminList: (params?: ContentQueryParams) =>
      [...sharingKeys.contents.adminLists(), params ?? {}] as const,
    details: () => [...sharingKeys.contents.all(), "detail"] as const,
    detail: (slugOrId: string) =>
      [...sharingKeys.contents.details(), slugOrId] as const,
    mineDetail: (id: string) =>
      [...sharingKeys.contents.all(), "mine-detail", id] as const,
  },

  schedules: {
    all: () => [...sharingKeys.all, "schedules"] as const,
    lists: () => [...sharingKeys.schedules.all(), "list"] as const,
    list: (params?: ScheduleQueryParams) =>
      [...sharingKeys.schedules.lists(), params ?? {}] as const,
    mineLists: () => [...sharingKeys.schedules.all(), "mine"] as const,
    mineList: (params?: ScheduleQueryParams) =>
      [...sharingKeys.schedules.mineLists(), params ?? {}] as const,
    adminLists: () => [...sharingKeys.schedules.all(), "admin"] as const,
    adminList: (params?: ScheduleQueryParams) =>
      [...sharingKeys.schedules.adminLists(), params ?? {}] as const,
    details: () => [...sharingKeys.schedules.all(), "detail"] as const,
    detail: (id: string) => [...sharingKeys.schedules.details(), id] as const,
  },

  tags: {
    all: () => [...sharingKeys.all, "tags"] as const,
    admin: () => [...sharingKeys.tags.all(), "admin"] as const,
  },

  resources: {
    byContent: (contentId: string) =>
      [...sharingKeys.all, "resources", contentId] as const,
  },
};
