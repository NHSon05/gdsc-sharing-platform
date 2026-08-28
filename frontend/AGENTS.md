# AGENTS.md — Frontend

This file defines the shared working rules for every agent modifying the
frontend under `frontend/`. Instructions in a more deeply nested `AGENTS.md` may add
more specific rules for that subtree, but must not weaken security, type safety,
accessibility, or verification requirements in this file.

For the in-depth architectural specification, dependency model, and state ownership,
refer to `docs/ARCHITECTURE.md`.

## 1. Product context

GDSC Sharing Platform is a knowledge-sharing and learning application for GDSC (Google Developer Groups On Campus) members.

Primary roles:

- `Admin`: manages roadmaps, sharing schedules, assignments, and member roles.
- `Member`: creates sharing content, attaches links, writes notes, and views learning history.

The frontend consumes the ASP.NET Core API in the repository root. Authentication
uses JWT access tokens and rotating refresh tokens. API errors follow RFC 7807
Problem Details and may include a `traceId`.

## 2. Current stack

- **Framework**: Next.js 16 App Router
- **Runtime / UI Library**: React 19
- **Language**: TypeScript 5 with `strict: true`
- **Linting**: ESLint 9 with `eslint-config-next`
- **Styling**: Tailwind CSS v4 (`@tailwindcss/postcss`) with CSS custom properties in `src/app/globals.css`
- **UI Primitives**: Base UI (`@base-ui/react`), `class-variance-authority`, Lucide React
- **Server State Management**: TanStack React Query (`@tanstack/react-query`)
- **Client State Management**: Zustand (`zustand`)
- **HTTP Transport**: Axios (`axios`)
- **Path Alias**: `@/*` maps to `src/*`

Do not introduce competing state managers, UI component libraries, CSS frameworks,
or unapproved dependencies outside of this stack without documented architecture approval.

## 3. Source layout

The codebase follows the 4-layer enterprise architecture defined in `docs/ARCHITECTURE.md`:

```text
frontend/
├── src/
│   ├── app/                 # Routes, layouts, providers, loading/error boundaries, globals.css
│   ├── core/                # Core technical infrastructure
│   │   ├── config/          # Validated runtime environment config (env.ts)
│   │   ├── http/            # Axios clients, interceptors, problem-details, error normalization
│   │   ├── query/           # TanStack QueryClient & QueryProvider
│   │   └── session/         # In-memory session store & selectors (Zustand)
│   ├── features/            # Feature-driven business modules (auth, roadmap, sharing, members)
│   │   ├── [feature]/
│   │   │   ├── api/         # Plain endpoint API calls
│   │   │   ├── hooks/       # React Query hooks (useXxxQuery, useXxxMutation)
│   │   │   ├── queries/     # Query key factories
│   │   │   ├── types/       # Feature domain & request/response types
│   │   │   └── index.ts     # Controlled public feature exports
│   ├── components/          # Reusable shared UI & layout components
│   │   ├── ui/              # Reusable presentation components (Button, TextField, Card, etc.)
│   │   └── auth/            # Auth-specific UI components
│   ├── shared/              # Cross-feature shared layout, hooks, types, constants
│   ├── assets/              # Static media, images, and brand assets
│   ├── lib/                 # Shared utilities (cn, etc.)
│   ├── hooks/               # Truly cross-cutting hooks
│   └── styles/              # Supplemental styles
├── public/                  # Public static assets
├── docs/
│   └── ARCHITECTURE.md      # Complete frontend architecture specification
└── AGENTS.md
```

Dependency direction rules:

- `app` → `features` → `core`
- `app` → `shared` / `components`
- `features` → `shared` / `components`
- `core` must NEVER import from `features` (prevents circular dependencies with HTTP/auth).

## 4. Next.js and React rules

- Prefer Server Components. Add `"use client"` only when a component needs browser APIs, local state, effects, or event handlers.
- Keep client-component boundaries small. Do not convert an entire page or layout into a Client Component for one interactive control.
- Fetch initial read-only data in Server Components when authentication and cache behavior permit it.
- Use route-level `loading.tsx`, `error.tsx`, and `not-found.tsx` where the user needs clear feedback.
- Never perform side effects during rendering.
- Do not copy props into state unless there is a deliberate synchronization rule.
- Use stable domain identifiers as React keys; never use array indexes for mutable lists.
- Use `next/link` for internal navigation and `next/image` for meaningful raster images.
- Keep route files focused on composition. Move complex feature behavior out of `page.tsx`.

## 5. TypeScript rules

- Keep `strict` mode enabled.
- Do not use `any`, `@ts-ignore`, or unsafe type assertions to silence errors.
- Use `unknown` at untrusted boundaries and narrow it explicitly.
- Model API request and response payloads with named types.
- Prefer discriminated unions for finite UI and request states.
- Use `type` for unions and compositions; use `interface` for public object contracts that are intentionally extendable.
- Do not duplicate backend enums as arbitrary strings throughout components. Define one frontend contract and map display labels separately.
- Treat values from forms, URL parameters, storage, and API responses as untrusted input.
- Avoid non-null assertions unless an invariant is immediately evident and documented.

## 6. API conventions

All HTTP calls must go through the shared client under `src/core/http/` or a feature-specific service built on top of it. Components must not scatter raw `fetch` calls throughout the tree.

API rules:

- Keep the API origin in `NEXT_PUBLIC_API_URL`; do not hard-code deployment hosts in components.
- Confirm the real backend route before coding. Authentication endpoints are currently rooted at `/api/auth`, while `/api/v1` is a separate API status endpoint. Do not assume every endpoint is below `/api/v1`.
- Send `Content-Type: application/json` only when the request has a JSON body. Support `FormData` for uploads.
- Preserve caller-provided headers and options.
- Support `AbortSignal` so abandoned navigation and repeated searches can cancel requests.
- Parse RFC 7807 responses into one typed `ApiError` containing at least `status`, `title`, `detail`, `validationErrors`, and `traceId` when present.
- Never show raw stack traces or internal exception details to users.
- Handle `204 No Content` without attempting to parse JSON.
- Distinguish validation errors (`400`), unauthenticated (`401`), forbidden (`403`), not found (`404`), and unexpected failures.
- Do not retry mutations automatically. Retry safe reads only with an explicit, bounded policy.

## 7. Authentication and security

- Never log passwords, access tokens, refresh tokens, or full Authorization headers.
- Never embed secrets in `NEXT_PUBLIC_*` variables. These variables are visible in the browser; only public configuration belongs there.
- Do not persist refresh tokens in `localStorage`, `sessionStorage`, IndexedDB, or non-HttpOnly cookies.
- Prefer a backend-controlled `Secure`, `HttpOnly`, `SameSite` refresh cookie and keep short-lived access tokens in memory (Zustand session store).
- Refresh tokens are rotating and single-use. The `refresh-coordinator.ts` manages single-flight refresh promises to prevent concurrent refresh token invalidation.
- When refresh fails with `401`, clear local authentication state, clear QueryClient private cache, and redirect to login once. Prevent refresh loops.
- After `logout-all`, clear every local session artifact and cached private user response.
- Client-side role checks improve UX but are not authorization. The API remains the source of truth.
- Do not render protected data before authentication status is known.
- Avoid `dangerouslySetInnerHTML`. If rich content is required, sanitize it with an approved and tested policy.

## 8. Components, forms and state

- A component should have one clear responsibility.
- Prefer composition over large components controlled by many boolean props.
- Keep server state (TanStack Query), URL state, form state, and local client state (Zustand) conceptually separate.
- Put filters, search terms, sorting, and pagination in the URL when users should be able to refresh, bookmark, or share the view.
- Disable a submit action while the same mutation is pending.
- Prevent accidental duplicate submissions.
- Show field-level validation close to the relevant field and retain a concise form-level message when necessary.
- Do not trim or transform passwords. Normalize email only according to the API contract.
- Preserve user-entered values after a recoverable failure.
- Every asynchronous screen must intentionally support loading, empty, success, and error states.

## 9. Styling and design system

- Reuse existing design tokens in `:root` and `.dark` in `globals.css`:
  - **GDSC Brand Tokens**: `--brand: #4285F4`, `--brand-hover`, `--brand-foreground`, `--brand-muted`, `--brand-border`, `--brand-glow`.
  - **GDSC Identity Colors**: `--gdsc-blue`, `--gdsc-red`, `--gdsc-yellow`, `--gdsc-green`.
  - **Semantic Tokens**: `--background`, `--foreground`, `--card`, `--border`, `--ring`, etc.
- **Liquid Glass Refraction**: Use `variant="liquid-glass"` on Card or glass components with subtle inner shadows (`shadow-[...,inset_0_1px_0_rgba(255,255,255,0.85)]`) to simulate physical edge refraction.
- **Micro-Animations & Physics**: Isolate continuous perpetual motion (like orbital rotations) in leaf Client Components with GPU-accelerated transforms.
- **Responsive Layout**: Use mobile-first responsive layouts and verify at mobile, tablet, and desktop widths. Prefer `min()`, `max()`, `clamp()`, grid, and flexbox over fixed pixel widths.
- Respect `prefers-reduced-motion` for non-essential animations.

## 10. Accessibility and copywriting

- **Language Standard**: The platform is standardized in 100% English (`<html lang="en">`). Keep all user-facing copy concise, natural, and idiomatic English.
- **Strict Anti-Emoji Policy**: Never use unicode emojis in UI text or icons; strictly use vector SVGs (e.g. Lucide React).
- Use semantic HTML before adding ARIA.
- Every form control needs a visible label or an equivalent accessible name (`TextField` supports both inside and outside label variants).
- Interactive elements must be reachable and usable by keyboard.
- Maintain a visible focus state with sufficient contrast (`focus-within:border-brand`, `ring-brand`).
- Provide useful `alt` text for meaningful images and empty `alt=""` for decorative images.
- Associate validation errors with fields using `aria-describedby`.

## 11. Verification and testing

For every change, verify behavior with automated linting and builds:

```bash
cd frontend
pnpm run lint
pnpm run build
```

Do not claim verification passed unless the commands were actually executed. If an environment dependency prevents a check, report the exact status and command that remains to be run.

## 12. Environment and package management

- **Package Manager**: Use `pnpm` (lockfile: `pnpm-lock.yaml`).
- Commit `.env.example`; never commit `.env`, credentials, or real tokens.
- Document every new environment variable in `.env.example` and the root `README.md` when it affects setup.
- Do not manually edit `pnpm-lock.yaml`.
- Before adding a dependency, confirm the platform cannot reasonably provide the behavior with existing dependencies or browser APIs.
- Pin dependencies through the lockfile and include the reason for any new package in the handoff.

## 13. Change boundaries

- Preserve unrelated user changes.
- Do not modify backend contracts merely to make a frontend implementation convenient. Coordinate or document required API changes.
- Do not rename public routes, request fields, or response fields without updating every consumer and relevant documentation.
- Do not perform broad visual redesigns unless the request includes them.
- Keep refactors separate from behavior changes when practical.
- Remove dead code created by the change, but do not delete unrelated files.

## 14. Definition of done

A frontend task is complete only when all applicable items are true:

- The requested user flow works from entry to success and failure states.
- Server/Client Component boundaries are intentional.
- TypeScript remains strict with no new unsafe suppressions.
- Loading, empty, error, and disabled states are handled.
- Keyboard use, labels, focus, and responsive layout were checked.
- Authentication data and secrets are handled safely.
- API errors preserve useful `traceId` information without exposing internals.
- `pnpm run lint` (or `npm run lint`) passes with 0 errors.
- `pnpm run build` (or `npm run build`) passes.
- No unrelated files or behavior were changed.

## 15. Agent handoff format

At the end of a frontend task, report:

1. What changed from the user's perspective.
2. The main files changed.
3. Commands actually run and their results.
4. Any remaining limitations, API dependencies, or follow-up risks.
