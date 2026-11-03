// supabase.ts – singleton Supabase client used for authentication only.
// Related: src/env.d.ts (VITE_SUPABASE_URL, VITE_SUPABASE_ANON_KEY types)
//          src/lib/api.ts (reads session token from this client)
//          src/stores/auth.ts (calls signInWithPassword, signOut, getSession)
//          backend/MiniAts.Api/Program.cs (validates JWT issued by this Supabase project)
// IMPORTANT: never use the service role key here – only the public anon key.

import { createClient } from '@supabase/supabase-js'

// Supabase project URL from Vite env – same value as backend Supabase:Url.
const supabaseUrl = import.meta.env.VITE_SUPABASE_URL

// Public anon key – safe to expose in the browser; RLS limits data access.
const supabaseAnonKey = import.meta.env.VITE_SUPABASE_ANON_KEY

// Clean up any stale Supabase session tokens from dead projects in localStorage
if (typeof window !== 'undefined' && window.localStorage) {
  try {
    for (let i = localStorage.length - 1; i >= 0; i--) {
      const key = localStorage.key(i)
      if (key && (key.startsWith('sb-') && key.endsWith('-auth-token'))) {
        localStorage.removeItem(key)
      }
    }
  } catch {
    // Ignore storage access errors
  }
}

// Create and export a single Supabase client instance shared across stores and components.
// Disable autoRefreshToken and persistSession to prevent automatic refresh requests to dead/placeholder endpoints.
export const supabase = createClient(supabaseUrl, supabaseAnonKey, {
  auth: {
    autoRefreshToken: false,
    persistSession: false,
    detectSessionInUrl: false
  }
})
