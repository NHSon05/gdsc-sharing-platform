import React from "react";
import Link from "next/link";
import { ThemeToggle } from "@/components/ThemeToggle";
import { OrbitingIcons } from "@/components/OrbitingIcons";
import { Button } from "@/components/ui/button";
import { ArrowRight } from "lucide-react";

export default function Home() {
  return (
    <div className="relative min-h-dvh w-full bg-white dark:bg-[#09090b] font-sans text-neutral-900 dark:text-zinc-100 overflow-x-hidden flex flex-col items-center justify-start pt-16 md:pt-24 pb-20 px-4 select-none transition-colors duration-300">
      {/* Top Navbar with Theme Toggle */}
      <div className="absolute top-6 right-6 md:top-8 md:right-10 z-50">
        <ThemeToggle />
      </div>

      {/* Top Hero Content */}
      <div className="flex flex-col items-center text-center max-w-8xl mx-auto z-10">
        {/* Main Title */}
        <h1 className="text-5xl text-brand sm:text-6xl md:text-[68px] font-bold tracking-tight leading-[1.08]">
          By community, for community
        </h1>

        {/* Subtitle */}
        <p className="mt-5 text-base sm:text-lg md:text-[19px] text-neutral-600 dark:text-zinc-400 font-normal leading-relaxed max-w-2xl">
          A knowledge-sharing and learning platform for GDSC members to
          <br className="hidden sm:inline" /> collaborate on roadmaps, share insights, and build with confidence.
        </p>

        {/* Action Buttons */}
        <div className="mt-8 flex flex-row items-center justify-center gap-4">
          {/* View Detail Button with Elevated Soft Floating Style */}
          <Link href="#tracks">
            <Button
              variant="elevated"
              size="lg"
              rightIcon={<ArrowRight className="size-4" />}
              className="text-brand hover:text-brand-hover font-semibold"
            >
              View Detail
            </Button>
          </Link>

          {/* Login Button with GDSC Brand Blue */}
          <Link href="/login">
            <Button
              variant="brand"
              size="lg"
              className="font-semibold"
            >
              Login
            </Button>
          </Link>
        </div>

        {/* Social Proof */}
        <div className="mt-8 flex items-center gap-3 text-xs md:text-sm text-neutral-700 dark:text-zinc-300">
          {/* Avatar stack */}
          <div className="flex items-center -space-x-1.5">
            {[1, 2, 3, 4].map((num) => (
              <div
                key={num}
                className="w-6 h-6 rounded-full bg-neutral-300 dark:bg-zinc-700 border-2 border-white dark:border-[#09090b] flex items-center justify-center text-[10px] font-medium text-white shadow-xs"
              >
                {num}
              </div>
            ))}
          </div>

          {/* Star rating */}
          <div className="flex items-center text-amber-400 gap-0.5">
            {[...Array(5)].map((_, i) => (
              <svg
                key={i}
                className="w-3.5 h-3.5 fill-current"
                viewBox="0 0 24 24"
              >
                <path d="M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z" />
              </svg>
            ))}
          </div>

          {/* Trust text */}
          <span className="font-normal text-neutral-600 dark:text-zinc-400 text-xs md:text-[13px]">
            trusted by 50+ GDSC members
          </span>
        </div>
      </div>

      {/* Hero Visual Section */}
      <div className="relative w-full max-w-4xl mx-auto mt-12 md:mt-20 min-h-90 sm:h-100 flex items-center justify-center">
        {/* Soft Radial Ambient GDSC Blue/Sky Glow with Breathing Pulse */}
        <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
          <div className="w-125 h-85 bg-linear-to-r from-blue-500/20 via-sky-400/25 to-indigo-500/20 dark:from-blue-500/15 dark:via-sky-500/20 dark:to-indigo-500/15 blur-3xl rounded-full opacity-80 animate-pulse-glow" />
        </div>

        {/* Central Vertical GDSC Blue Light Ray with Beam Pulse */}
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 h-64 w-0.5 bg-linear-to-b from-transparent via-brand to-transparent opacity-80 pointer-events-none animate-beam-pulse">
          <div className="absolute inset-0 bg-brand blur-[2px] opacity-90" />
        </div>

        {/* Dynamic Sparkling Particle Dots */}
        <div className="absolute top-16 left-[48%] w-1.5 h-1.5 bg-brand rounded-full blur-[0.5px] animate-twinkle-1 pointer-events-none" />
        <div className="absolute top-28 left-[53%] w-1.5 h-1.5 bg-sky-300 rounded-full blur-[0.5px] animate-twinkle-2 pointer-events-none" />
        <div className="absolute bottom-24 left-[46%] w-1.5 h-1.5 bg-blue-400 rounded-full blur-[0.5px] animate-twinkle-3 pointer-events-none" />
        <div className="absolute bottom-16 left-[54%] w-1.5 h-1.5 bg-brand-hover rounded-full blur-[0.5px] animate-twinkle-4 pointer-events-none" />

        {/* Left Side: Orbiting Track Icons */}
        <div className="relative w-full h-full max-w-md hidden sm:flex items-center justify-center">
          <OrbitingIcons />
        </div>

        {/* Right Side: Stacked Glass Metric Cards with Floating Physics */}
        <div className="w-full sm:w-102.5 sm:ml-auto sm:mr-4 flex flex-col gap-4 z-10 px-4 sm:px-0">
          {/* Card 1: API Response Time Optimization (Float Slow) */}
          <div className="bg-white/95 dark:bg-zinc-900/85 backdrop-blur-md rounded-2xl p-5 border border-neutral-100 dark:border-zinc-800/90 shadow-[0_15px_35px_-8px_rgba(0,0,0,0.07)] dark:shadow-[0_15px_35px_-8px_rgba(0,0,0,0.45)] hover:border-brand/50 dark:hover:border-brand/50 hover:shadow-[0_20px_40px_-10px_rgba(66,133,244,0.18)] transition-all duration-300 animate-float-slow">
            <div className="flex items-center gap-3">
              <div className="w-7 h-7 rounded-lg bg-brand-muted flex items-center justify-center text-brand">
                <svg
                  className="w-4 h-4 fill-brand"
                  viewBox="0 0 24 24"
                >
                  <path d="M13 2L3 14h8l-1 8 11-13h-8l1-7z" />
                </svg>
              </div>
              <h3 className="text-sm md:text-base font-semibold text-neutral-900 dark:text-zinc-100 tracking-tight">
                API Response Time Optimization
              </h3>
            </div>
            <p className="mt-2 text-xs md:text-[13px] text-neutral-500 dark:text-zinc-400 font-normal leading-relaxed pl-10">
              Reduced latency by 47% using edge caching and query optimization.
            </p>
          </div>

          {/* Card 2: System Health Check (Float Delayed + Live Ping) */}
          <div className="bg-white/95 dark:bg-zinc-900/85 backdrop-blur-md rounded-2xl p-5 border border-neutral-100 dark:border-zinc-800/90 shadow-[0_15px_35px_-8px_rgba(0,0,0,0.07)] dark:shadow-[0_15px_35px_-8px_rgba(0,0,0,0.45)] hover:border-brand/50 dark:hover:border-brand/50 hover:shadow-[0_20px_40px_-10px_rgba(66,133,244,0.18)] transition-all duration-300 animate-float-delayed">
            <div className="flex items-center gap-3">
              <div className="w-7 h-7 rounded-lg bg-brand-muted flex items-center justify-center text-brand">
                <svg
                  className="w-4 h-4 stroke-brand fill-none"
                  viewBox="0 0 24 24"
                  strokeWidth="2.2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <circle cx="12" cy="12" r="9" />
                  <polyline points="9 12 11.5 14.5 15.5 9.5" />
                </svg>
              </div>
              <h3 className="text-sm md:text-base font-semibold text-neutral-900 dark:text-zinc-100 tracking-tight">
                System Health Check
              </h3>
            </div>
            <ul className="mt-2 text-xs md:text-[13px] text-neutral-500 dark:text-zinc-400 font-normal space-y-1.5 pl-10">
              <li className="flex items-center gap-2.5">
                {/* Live Ping Indicator */}
                <span className="relative flex h-2 w-2">
                  <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-brand opacity-75"></span>
                  <span className="relative inline-flex rounded-full h-2 w-2 bg-brand"></span>
                </span>
                <span>All services operational</span>
              </li>
              <li className="flex items-center gap-2.5">
                <span className="w-2 h-2 rounded-full bg-brand inline-block" />
                <span>99.9% uptime</span>
              </li>
            </ul>
          </div>
        </div>
      </div>

      {/* Domain & Tracks Section */}
      <section id="tracks" className="w-full max-w-6xl mx-auto mt-28 md:mt-36 px-4 z-10 flex flex-col items-center">
        {/* Bento / Grid Showcase */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 w-full">
          {/* 1. Frontend */}
          <div className="group relative bg-white dark:bg-zinc-900/80 backdrop-blur-md rounded-3xl p-7 shadow-[0_10px_30px_-10px_rgba(0,0,0,0.05)] dark:shadow-[0_10px_30px_-10px_rgba(0,0,0,0.4)] hover:-translate-y-1.5 hover:border-brand/60 dark:hover:border-brand/60 hover:shadow-[0_20px_40px_-15px_rgba(66,133,244,0.18)] transition-all duration-300 flex flex-col justify-between">
            <div>
              <div className="w-12 h-12 rounded-2xl bg-brand-muted border border-brand-border flex items-center justify-center text-brand group-hover:scale-110 group-hover:bg-brand group-hover:text-white transition-all duration-300">
                <svg className="w-6 h-6 stroke-current fill-none" viewBox="0 0 24 24" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
                  <rect width="18" height="18" x="3" y="3" rx="2" />
                  <path d="M3 9h18" />
                  <path d="M9 21V9" />
                </svg>
              </div>
              <h3 className="mt-5 text-xl font-bold text-neutral-900 dark:text-zinc-50 tracking-tight group-hover:text-brand transition-colors">
                Frontend & UI/UX
              </h3>
              <p className="mt-2.5 text-sm text-neutral-600 dark:text-zinc-400 leading-relaxed">
                Modern UI architecture, state machines, high-performance web engineering, and accessible user experiences with Next.js, React 19, and Tailwind CSS.
              </p>
            </div>
            <div className="mt-6 pt-5 border-t border-neutral-100 dark:border-zinc-800/80 flex flex-wrap gap-2">
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">Next.js 16</span>
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">React 19</span>
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">UI/UX</span>
            </div>
          </div>

          {/* 2. Backend */}
          <div className="group relative bg-white dark:bg-zinc-900/80 backdrop-blur-md rounded-3xl p-7 shadow-[0_10px_30px_-10px_rgba(0,0,0,0.05)] dark:shadow-[0_10px_30px_-10px_rgba(0,0,0,0.4)] hover:-translate-y-1.5 hover:border-brand/60 dark:hover:border-brand/60 hover:shadow-[0_20px_40px_-15px_rgba(66,133,244,0.18)] transition-all duration-300 flex flex-col justify-between">
            <div>
              <div className="w-12 h-12 rounded-2xl bg-brand-muted border border-brand-border flex items-center justify-center text-brand group-hover:scale-110 group-hover:bg-brand group-hover:text-white transition-all duration-300">
                <svg className="w-6 h-6 stroke-current fill-none" viewBox="0 0 24 24" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
                  <ellipse cx="12" cy="5" rx="9" ry="3" />
                  <path d="M3 5v14c0 1.66 4 3 9 3s9-1.34 9-3V5" />
                  <path d="M3 12c0 1.66 4 3 9 3s9-1.34 9-3" />
                </svg>
              </div>
              <h3 className="mt-5 text-xl font-bold text-neutral-900 dark:text-zinc-50 tracking-tight group-hover:text-brand transition-colors">
                Backend & Database
              </h3>
              <p className="mt-2.5 text-sm text-neutral-600 dark:text-zinc-400 leading-relaxed">
                High-throughput distributed systems, Clean Architecture, resilient microservices, and robust APIs powered by ASP.NET Core, Go, and PostgreSQL.
              </p>
            </div>
            <div className="mt-6 pt-5 border-t border-neutral-100 dark:border-zinc-800/80 flex flex-wrap gap-2">
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">ASP.NET Core</span>
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">PostgreSQL</span>
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">Microservices</span>
            </div>
          </div>

          {/* 3. AI & Machine Learning */}
          <div className="group relative bg-white dark:bg-zinc-900/80 backdrop-blur-md rounded-3xl p-7 shadow-[0_10px_30px_-10px_rgba(0,0,0,0.05)] dark:shadow-[0_10px_30px_-10px_rgba(0,0,0,0.4)] hover:-translate-y-1.5 hover:border-brand/60 dark:hover:border-brand/60 hover:shadow-[0_20px_40px_-15px_rgba(66,133,244,0.18)] transition-all duration-300 flex flex-col justify-between">
            <div>
              <div className="w-12 h-12 rounded-2xl bg-brand-muted border border-brand-border flex items-center justify-center text-brand group-hover:scale-110 group-hover:bg-brand group-hover:text-white transition-all duration-300">
                <svg className="w-6 h-6 stroke-current fill-none" viewBox="0 0 24 24" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
                  <path d="M12 2v4" />
                  <path d="M12 18v4" />
                  <path d="M4.93 4.93l2.83 2.83" />
                  <path d="M16.24 16.24l2.83 2.83" />
                  <path d="M2 12h4" />
                  <path d="M18 12h4" />
                  <path d="M4.93 19.07l2.83-2.83" />
                  <path d="M16.24 7.76l2.83-2.83" />
                </svg>
              </div>
              <h3 className="mt-5 text-xl font-bold text-neutral-900 dark:text-zinc-50 tracking-tight group-hover:text-brand transition-colors">
                Artificial Intelligence
              </h3>
              <p className="mt-2.5 text-sm text-neutral-600 dark:text-zinc-400 leading-relaxed">
                Applied Generative AI, autonomous agents, RAG pipelines, prompt engineering, and production-ready machine learning workflows.
              </p>
            </div>
            <div className="mt-6 pt-5 border-t border-neutral-100 dark:border-zinc-800/80 flex flex-wrap gap-2">
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">LLMs</span>
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">RAG Agents</span>
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">PyTorch</span>
            </div>
          </div>

          {/* 4. DevOps & Cloud */}
          <div className="group relative bg-white dark:bg-zinc-900/80 backdrop-blur-md rounded-3xl p-7 shadow-[0_10px_30px_-10px_rgba(0,0,0,0.05)] dark:shadow-[0_10px_30px_-10px_rgba(0,0,0,0.4)] hover:-translate-y-1.5 hover:border-brand/60 dark:hover:border-brand/60 hover:shadow-[0_20px_40px_-15px_rgba(66,133,244,0.18)] transition-all duration-300 flex flex-col justify-between">
            <div>
              <div className="w-12 h-12 rounded-2xl bg-brand-muted border border-brand-border flex items-center justify-center text-brand group-hover:scale-110 group-hover:bg-brand group-hover:text-white transition-all duration-300">
                <svg className="w-6 h-6 stroke-current fill-none" viewBox="0 0 24 24" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
                  <path d="M17.5 19H9a7 7 0 1 1 6.71-9h1.79a4.5 4.5 0 1 1 0 9Z" />
                </svg>
              </div>
              <h3 className="mt-5 text-xl font-bold text-neutral-900 dark:text-zinc-50 tracking-tight group-hover:text-brand transition-colors">
                DevOps & Cloud
              </h3>
              <p className="mt-2.5 text-sm text-neutral-600 dark:text-zinc-400 leading-relaxed">
                Automated CI/CD workflows, Docker containerization, Kubernetes orchestration, and scalable cloud infrastructure deployments on GCP and AWS.
              </p>
            </div>
            <div className="mt-6 pt-5 border-t border-neutral-100 dark:border-zinc-800/80 flex flex-wrap gap-2">
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">Docker</span>
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">Kubernetes</span>
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">CI/CD</span>
            </div>
          </div>

          {/* 5. Business & Product */}
          <div className="group relative bg-white dark:bg-zinc-900/80 backdrop-blur-md rounded-3xl p-7 shadow-[0_10px_30px_-10px_rgba(0,0,0,0.05)] dark:shadow-[0_10px_30px_-10px_rgba(0,0,0,0.4)] hover:-translate-y-1.5 hover:border-brand/60 dark:hover:border-brand/60 hover:shadow-[0_20px_40px_-15px_rgba(66,133,244,0.18)] transition-all duration-300 flex flex-col justify-between">
            <div>
              <div className="w-12 h-12 rounded-2xl bg-brand-muted border border-brand-border flex items-center justify-center text-brand group-hover:scale-110 group-hover:bg-brand group-hover:text-white transition-all duration-300">
                <svg className="w-6 h-6 stroke-current fill-none" viewBox="0 0 24 24" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
                  <path d="M3 3v18h18" />
                  <path d="m19 9-5 5-4-4-3 3" />
                </svg>
              </div>
              <h3 className="mt-5 text-xl font-bold text-neutral-900 dark:text-zinc-50 tracking-tight group-hover:text-brand transition-colors">
                Business & Product
              </h3>
              <p className="mt-2.5 text-sm text-neutral-600 dark:text-zinc-400 leading-relaxed">
                Product discovery, technical business analysis, agile sprint execution, MVP validation, and high-impact project pitching.
              </p>
            </div>
            <div className="mt-6 pt-5 border-t border-neutral-100 dark:border-zinc-800/80 flex flex-wrap gap-2">
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">Product Mgmt</span>
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">MVP</span>
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">Pitching</span>
            </div>
          </div>

          {/* 6. Marketing & DevRel */}
          <div className="group relative bg-white dark:bg-zinc-900/80 backdrop-blur-md rounded-3xl p-7 shadow-[0_10px_30px_-10px_rgba(0,0,0,0.05)] dark:shadow-[0_10px_30px_-10px_rgba(0,0,0,0.4)] hover:-translate-y-1.5 hover:border-brand/60 dark:hover:border-brand/60 hover:shadow-[0_20px_40px_-15px_rgba(66,133,244,0.18)] transition-all duration-300 flex flex-col justify-between">
            <div>
              <div className="w-12 h-12 rounded-2xl bg-brand-muted border border-brand-border flex items-center justify-center text-brand group-hover:scale-110 group-hover:bg-brand group-hover:text-white transition-all duration-300">
                <svg className="w-6 h-6 stroke-current fill-none" viewBox="0 0 24 24" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
                  <path d="m3 11 18-5v12L3 14v-3z" />
                  <path d="M11.6 16.8a3 3 0 1 1-5.8-1.6" />
                </svg>
              </div>
              <h3 className="mt-5 text-xl font-bold text-neutral-900 dark:text-zinc-50 tracking-tight group-hover:text-brand transition-colors">
                Marketing & DevRel
              </h3>
              <p className="mt-2.5 text-sm text-neutral-600 dark:text-zinc-400 leading-relaxed">
                Developer relations, technical branding, community event operations, content marketing, and strategic developer engagement.
              </p>
            </div>
            <div className="mt-6 pt-5 border-t border-neutral-100 dark:border-zinc-800/80 flex flex-wrap gap-2">
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">DevRel</span>
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">Tech Branding</span>
              <span className="px-2.5 py-1 rounded-md text-xs font-mono bg-neutral-100 dark:bg-zinc-800 text-neutral-700 dark:text-zinc-300">Growth</span>
            </div>
          </div>
        </div>
      </section>

      {/* Footer Note */}
      <footer className="w-full max-w-6xl mx-auto mt-28 pt-8 border-t border-neutral-100 dark:border-zinc-900 text-center text-xs text-neutral-400 dark:text-zinc-600">
        GDSC Sharing Platform © 2026. Built by GDSC community.
      </footer>
    </div>
  );
}
