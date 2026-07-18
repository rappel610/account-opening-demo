import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// Proxying /api to the backend during dev means the frontend can call
// relative paths like "/api/onboarding/session" without hardcoding
// localhost:5001 everywhere, and it avoids CORS friction locally too.
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      "/api": {
        target: "http://localhost:5000",
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
