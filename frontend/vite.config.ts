// vite.config.ts – Vite build configuration for Mini ATS frontend.
// Related: package.json (plugin versions)
//          src/env.d.ts (VITE_ env variable types)
//          frontend/vercel.json (SPA rewrite for Vercel deploy)

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [
    // Vue plugin compiles .vue SFCs including <script setup> and <style scoped>.
    vue()
  ],
  server: {
    // Local dev server port – matches README and CORS config in .NET backend.
    port: 5173
  }
})
