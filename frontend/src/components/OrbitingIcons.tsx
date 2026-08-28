"use client";

import React from "react";

interface TrackIcon {
  id: string;
  label: string;
  render: () => React.ReactNode;
}

const trackIcons: TrackIcon[] = [
  {
    id: "terminal",
    label: "Dev / Terminal",
    render: () => (
      <span className="font-mono text-base leading-none font-bold">&gt;_</span>
    ),
  },
  {
    id: "marketing",
    label: "Marketing & DevRel",
    render: () => (
      <svg
        className="h-5 w-5 fill-none stroke-current"
        viewBox="0 0 24 24"
        strokeWidth="1.8"
        strokeLinecap="round"
        strokeLinejoin="round"
      >
        <path d="m3 11 18-5v12L3 14v-3z" />
        <path d="M11.6 16.8a3 3 0 1 1-5.8-1.6" />
      </svg>
    ),
  },
  {
    id: "cloud",
    label: "Cloud & DevOps",
    render: () => (
      <svg
        className="h-5 w-5 fill-none stroke-current"
        viewBox="0 0 24 24"
        strokeWidth="1.8"
        strokeLinecap="round"
        strokeLinejoin="round"
      >
        <path d="M17.5 19H9a7 7 0 1 1 6.71-9h1.79a4.5 4.5 0 1 1 0 9Z" />
      </svg>
    ),
  },
  {
    id: "database",
    label: "Backend & Database",
    render: () => (
      <svg
        className="h-5 w-5 fill-none stroke-current"
        viewBox="0 0 24 24"
        strokeWidth="1.8"
        strokeLinecap="round"
        strokeLinejoin="round"
      >
        <ellipse cx="12" cy="5" rx="9" ry="3" />
        <path d="M3 5v14c0 1.66 4 3 9 3s9-1.34 9-3V5" />
        <path d="M3 12c0 1.66 4 3 9 3s9-1.34 9-3" />
      </svg>
    ),
  },
  {
    id: "business",
    label: "Business & Strategy",
    render: () => (
      <svg
        className="h-5 w-5 fill-none stroke-current"
        viewBox="0 0 24 24"
        strokeWidth="1.8"
        strokeLinecap="round"
        strokeLinejoin="round"
      >
        <path d="M3 3v18h18" />
        <path d="m19 9-5 5-4-4-3 3" />
      </svg>
    ),
  },
  {
    id: "code",
    label: "Frontend Code",
    render: () => (
      <span className="font-mono text-sm leading-none font-bold">
        &lt;/&gt;
      </span>
    ),
  },
];

export function OrbitingIcons() {
  const radius = 135; // radius in pixels
  const count = trackIcons.length;

  return (
    <div className="group/orbit relative flex h-[340px] w-[340px] items-center justify-center select-none">
      {/* Subtle Orbital Track Ring */}
      <div className="border-brand/30 dark:border-brand/25 pointer-events-none absolute h-[270px] w-[270px] rounded-full border border-dashed" />

      {/* Orbit Container with Smooth Rotation */}
      <div
        className="absolute inset-0 flex animate-[spin_28s_linear_infinite] items-center justify-center group-hover/orbit:[animation-play-state:paused]"
        style={{ willChange: "transform" }}
      >
        {trackIcons.map((icon, index) => {
          const angleDeg = (index * 360) / count - 90; // start from top
          const angleRad = (angleDeg * Math.PI) / 180;
          const x = Math.round(radius * Math.cos(angleRad));
          const y = Math.round(radius * Math.sin(angleRad));

          return (
            <div
              key={icon.id}
              className="absolute flex items-center justify-center"
              style={{
                transform: `translate(${x}px, ${y}px)`,
              }}
            >
              {/* Counter-rotation to keep icons upright */}
              <div
                className="animate-[spin_28s_linear_infinite] [animation-direction:reverse] group-hover/orbit:[animation-play-state:paused]"
                style={{ willChange: "transform" }}
              >
                <div
                  title={icon.label}
                  className="hover:text-brand dark:hover:text-brand-hover hover:border-brand/40 dark:hover:border-brand/40 flex h-[52px] w-[52px] cursor-pointer items-center justify-center rounded-2xl border border-neutral-100/90 bg-white text-neutral-800 shadow-[0_10px_28px_-6px_rgba(0,0,0,0.08)] transition-all duration-200 hover:scale-110 dark:border-zinc-800/90 dark:bg-zinc-900/95 dark:text-zinc-200 dark:shadow-[0_10px_28px_-6px_rgba(0,0,0,0.5)]"
                >
                  {icon.render()}
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
