/**
 * RFC 7807 Problem Details representation
 */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
  extensions?: Record<string, unknown>;
  [key: string]: unknown;
}
