import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";

export default defineConfig({
  plugins: [react()],
  build: {
    outDir: path.resolve(__dirname, "../ACI.Web/wwwroot/gantt-dist"),
    emptyOutDir: false,
    rollupOptions: {
      input: path.resolve(__dirname, "src/main.jsx"),
      output: {
        entryFileNames: "gantt-bundle.js",
        chunkFileNames: "gantt-[name].js",
        assetFileNames: (info) => {
          if (info.name?.endsWith(".css")) return "gantt-bundle.css";
          return "gantt-assets/[name][extname]";
        },
        // IIFE로 빌드: Razor 페이지에서 그냥 <script>로 로드 가능
        format: "iife",
        name: "ACIGantt",
      },
    },
  },
  server: {
    port: 5174,
    proxy: {
      "/api": "https://localhost:7001",
    },
  },
});
