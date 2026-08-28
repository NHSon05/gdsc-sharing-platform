import type { NextConfig } from "next";

const API_DESTINATION =
  process.env.BACKEND_API_URL ||
  process.env.NEXT_PUBLIC_API_URL ||
  "http://localhost:5184";

const nextConfig: NextConfig = {
  reactCompiler: true,
  images: {
    qualities: [75, 100],
  },
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: `${API_DESTINATION.replace(/\/+$/, "")}/api/:path*`,
      },
    ];
  },
};

export default nextConfig;
