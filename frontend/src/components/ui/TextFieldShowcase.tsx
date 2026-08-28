"use client";

import { TextField } from "@/components/ui/input";

export function TextFieldShowcase() {
  const searchIcon = (
    <svg
      className="h-4 w-4 fill-none stroke-current"
      viewBox="0 0 24 24"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <circle cx="11" cy="11" r="8" />
      <path d="m21 21-4.3-4.3" />
    </svg>
  );

  return (
    <div className="mx-auto w-full max-w-6xl px-4 py-12 font-sans select-none">
      <div className="mb-10 text-center">
        <span className="rounded-full border border-emerald-200/60 bg-emerald-50 px-3.5 py-1 text-xs font-semibold tracking-wide text-emerald-700 uppercase dark:border-emerald-800/50 dark:bg-emerald-950/50 dark:text-emerald-400">
          Design System
        </span>
        <h2 className="mt-3 text-3xl font-bold tracking-tight text-neutral-900 dark:text-zinc-100">
          Text Field Component Matrix
        </h2>
        <p className="mt-2 text-sm text-neutral-600 dark:text-zinc-400">
          Complete state variants matching the design specifications with
          inside/outside labels, search icons, quick clear, and trailing
          dropdowns.
        </p>
      </div>

      {/* 4-Column Matrix matching Figma spec */}
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-4">
        {/* Column 1: Inside Label + Trailing Value */}
        <div className="flex flex-col gap-5">
          <div className="font-mono text-xs font-semibold tracking-wider text-neutral-400 uppercase">
            Col 1: Inside Label
          </div>

          {/* Row 1: Default Empty */}
          <TextField
            label="Label"
            required
            labelVariant="inside"
            trailingValue="Value"
            supportingText="Supporting text"
          />

          {/* Row 2: Default */}
          <TextField
            label="Label"
            required
            labelVariant="inside"
            trailingValue="Value"
            supportingText="Supporting text"
          />

          {/* Row 3: Disabled */}
          <TextField
            label="Label"
            required
            labelVariant="inside"
            disabled
            trailingValue="Value"
            supportingText="Supporting text"
          />

          {/* Row 4: Error State */}
          <TextField
            label="Label"
            required
            labelVariant="inside"
            error="Supporting text"
            trailingValue="Value"
          />

          {/* Row 5: Filled / Clearable */}
          <TextField
            label="Label"
            required
            labelVariant="inside"
            defaultValue="Input Text Field"
            clearable
            trailingValue="Value"
            supportingText="Supporting text"
          />
        </div>

        {/* Column 2: Outside Label + Search Prefix + Trailing Value */}
        <div className="flex flex-col gap-5">
          <div className="font-mono text-xs font-semibold tracking-wider text-neutral-400 uppercase">
            Col 2: Outside + Prefix Icon
          </div>

          {/* Row 1: Default Empty with Search */}
          <TextField
            label="Label"
            required
            labelVariant="outside"
            placeholder="Placeholder"
            startIcon={searchIcon}
            trailingValue="Value"
            supportingText="Supporting text"
          />

          {/* Row 2: Default */}
          <TextField
            label="Label"
            required
            labelVariant="outside"
            placeholder="Placeholder"
            startIcon={searchIcon}
            trailingValue="Value"
            supportingText="Supporting text"
          />

          {/* Row 3: Disabled */}
          <TextField
            label="Label"
            required
            labelVariant="outside"
            placeholder="Placeholder"
            disabled
            startIcon={searchIcon}
            trailingValue="Value"
            supportingText="Supporting text"
          />

          {/* Row 4: Error State */}
          <TextField
            label="Label"
            required
            labelVariant="outside"
            placeholder="Placeholder"
            startIcon={searchIcon}
            error="Supporting text"
            trailingValue="Value"
          />

          {/* Row 5: Filled + Search + Clearable */}
          <TextField
            label="Label"
            required
            labelVariant="outside"
            defaultValue="Input Text Field"
            startIcon={searchIcon}
            clearable
            trailingValue="Value"
            supportingText="Supporting text"
          />
        </div>

        {/* Column 3: Inside Label + Filled Variants */}
        <div className="flex flex-col gap-5">
          <div className="font-mono text-xs font-semibold tracking-wider text-neutral-400 uppercase">
            Col 3: Inside Label Filled
          </div>

          {/* Row 1: Simple Inside */}
          <TextField
            label="Label"
            required
            labelVariant="inside"
            trailingValue="Value"
            supportingText="Supporting text"
          />

          {/* Row 2: Disabled Inside */}
          <TextField
            label="Label"
            required
            labelVariant="inside"
            disabled
            trailingValue="Value"
            supportingText="Supporting text"
          />

          {/* Row 3: Error Inside */}
          <TextField
            label="Label"
            required
            labelVariant="inside"
            error="Supporting text"
            trailingValue="Value"
          />

          {/* Row 4: Filled with Value */}
          <TextField
            label="Label"
            required
            labelVariant="inside"
            defaultValue="Input Text Field"
            trailingValue="Value"
            supportingText="Supporting text"
          />

          {/* Row 5: Error Filled */}
          <TextField
            label="Label"
            required
            labelVariant="inside"
            defaultValue="Input Text Field"
            error="Supporting text"
            trailingValue="Value"
          />
        </div>

        {/* Column 4: Outside Label + Filled Search + Clearable */}
        <div className="flex flex-col gap-5">
          <div className="font-mono text-xs font-semibold tracking-wider text-neutral-400 uppercase">
            Col 4: Outside + Search Filled
          </div>

          {/* Row 1: Search + Outside */}
          <TextField
            label="Label"
            required
            labelVariant="outside"
            placeholder="Placeholder"
            startIcon={searchIcon}
            trailingValue="Value"
            supportingText="Supporting text"
          />

          {/* Row 2: Search + Outside Disabled */}
          <TextField
            label="Label"
            required
            labelVariant="outside"
            placeholder="Placeholder"
            disabled
            startIcon={searchIcon}
            trailingValue="Value"
            supportingText="Supporting text"
          />

          {/* Row 3: Search + Outside Error */}
          <TextField
            label="Label"
            required
            labelVariant="outside"
            placeholder="Placeholder"
            startIcon={searchIcon}
            error="Supporting text"
            trailingValue="Value"
          />

          {/* Row 4: Search + Filled + Clearable */}
          <TextField
            label="Label"
            required
            labelVariant="outside"
            defaultValue="Input Text Field"
            startIcon={searchIcon}
            clearable
            trailingValue="Value"
            supportingText="Supporting text"
          />

          {/* Row 5: Search + Error Filled */}
          <TextField
            label="Label"
            required
            labelVariant="outside"
            defaultValue="Input Text Field"
            startIcon={searchIcon}
            error="Supporting text"
            trailingValue="Value"
          />
        </div>
      </div>
    </div>
  );
}
