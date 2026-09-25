/// <reference types="vite/client" />

// Declare frontend environment variables for TypeScript autocompletion.
// Related: vite.config.ts, frontend/.env.example
//          src/lib/supabase.ts (VITE_SUPABASE_URL, VITE_SUPABASE_ANON_KEY)
//          src/lib/api.ts (VITE_API_BASE_URL)

interface ImportMetaEnv {
  readonly VITE_SUPABASE_URL: string
  readonly VITE_SUPABASE_ANON_KEY: string
  readonly VITE_API_BASE_URL: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
