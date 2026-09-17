export type InterviewQuestionLevel = "basic" | "intermediate" | "advanced";

export interface InterviewQuestionQuery {
  page?: number;
  pageSize?: number;
  search?: string;
  level?: InterviewQuestionLevel | string;
  department?: string;
  topic?: string;
}

export interface InterviewQuestionSummary {
  id: string;
  question: string;
  slug: string;
  level: string;
  status: string;
  needsReview: boolean;
}

export interface InterviewQuestionPage {
  items: InterviewQuestionSummary[];
  total: number;
  page: number;
  pageSize: number;
}

export interface CodeExample {
  language: string;
  code: string;
}

export interface AnswerData {
  summary: string[];
  details: string[];
  points?: string[];
  sections?: { title?: string; content?: string }[];
  codeExamples?: CodeExample[];
  tables?: { headers?: string[]; rows?: string[][] }[];
}

export interface InterviewQuestionDetailPayload {
  question: string;
  level: string;
  levelLabel?: string;
  access?: string;
  departments?: string[];
  topics?: string[];
  answer: AnswerData | null;
  sources?: { file?: string; sourceUrl?: string; sourceSlug?: string }[];
}

export interface InterviewQuestionDetail {
  id: string;
  status: string;
  data: InterviewQuestionDetailPayload;
}

export interface CategoryItem {
  id: string;
  label: string;
  labelEn?: string;
  count?: number;
  iconName?: string;
  isNew?: boolean;
  department?: string;
  topic?: string;
}

export interface CategoryGroup {
  id: string;
  title: string;
  titleEn?: string;
  items: CategoryItem[];
}
