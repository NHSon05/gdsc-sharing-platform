export const en = {
  common: {
    backToHome: "Back to Home",
    login: "Login",
    signUp: "Sign up",
    viewDetail: "View Detail",
    language: "Language",
    theme: "Theme",
    orContinueWith: "Or continue with",
    termsNotice: "By clicking continue, you agree to our",
    termsOfService: "Terms of Service",
    and: "and",
    privacyPolicy: "Privacy Policy",
    dontHaveAccount: "Don't have an account?",
    footerNote: "GDSC Sharing Platform © 2026. Built by GDSC community.",
  },
  hero: {
    slogan: "By community, for community",
    title: "By community, for community.",
    subtitle:
      "A knowledge-sharing and learning platform for GDSC members to collaborate on roadmaps, share insights, and build with confidence.",
    trustedBy: "trusted by 50+ GDSC members",
  },
  metrics: {
    apiOptimizationTitle: "API Response Time Optimization",
    apiOptimizationDesc:
      "Reduced latency by 47% using edge caching and query optimization.",
    systemHealthTitle: "System Health Check",
    allServicesOperational: "All services operational",
    uptime: "99.9% uptime",
  },
  tracks: {
    heading: "Specialized Tracks",
    subheading:
      "Structured learning paths and collaborative roadmaps designed for GDSC builders.",
    frontend: {
      title: "Frontend & UI/UX",
      desc: "Modern UI architecture, state machines, high-performance web engineering, and accessible user experiences with Next.js, React 19, and Tailwind CSS.",
    },
    backend: {
      title: "Backend & System Design",
      desc: "High-throughput distributed systems, Clean Architecture, resilient microservices, and robust APIs powered by ASP.NET Core, Go, and PostgreSQL.",
    },
    ai: {
      title: "Artificial Intelligence",
      desc: "Applied Generative AI, autonomous agents, RAG pipelines, prompt engineering, and production-ready machine learning workflows.",
    },
    devops: {
      title: "DevOps & Cloud",
      desc: "Automated CI/CD workflows, Docker containerization, Kubernetes orchestration, and scalable cloud infrastructure deployments on GCP and AWS.",
    },
    business: {
      title: "Business & Product",
      desc: "Product discovery, technical business analysis, agile sprint execution, MVP validation, and high-impact project pitching.",
    },
    marketing: {
      title: "Marketing & DevRel",
      desc: "Developer relations, technical branding, community event operations, content marketing, and strategic developer engagement.",
    },
  },
  login: {
    title: "Welcome back",
    subtitle: "Login to your GDSC platform account",
    emailLabel: "Email",
    emailPlaceholder: "member@gdsc.dev",
    passwordLabel: "Password",
    passwordPlaceholder: "••••••••",
    forgotPassword: "Forgot password?",
    signInButton: "Sign in",
  },
} as const;

type DeepStringRecord<T> = {
  [K in keyof T]: T[K] extends object ? DeepStringRecord<T[K]> : string;
};

export type TranslationDictionary = DeepStringRecord<typeof en>;
