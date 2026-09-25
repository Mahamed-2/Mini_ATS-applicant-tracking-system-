// api.ts – authenticated fetch client for all .NET REST API calls.
// Related: src/lib/supabase.ts (reads session JWT)
//          src/env.d.ts (VITE_API_BASE_URL type)
//          src/stores/auth.ts (on 401: sign out and redirect)
//          backend/MiniAts.Api/Program.cs (JWT validation, CORS)
//          backend/MiniAts.Api/Api/Controllers/ (all endpoints called via this client)

import { supabase } from './supabase'

// Base URL of the .NET REST API – set via VITE_API_BASE_URL env var.
const baseUrl = import.meta.env.VITE_API_BASE_URL

/** Generic error shape returned by backend controllers. */
export interface ApiError {
  message: string
}

/**
 * apiFetch wraps fetch with JWT authentication and JSON parsing.
 * Attaches the Supabase access token as a Bearer header on every request.
 * On 401, signs out locally and throws so the router guard redirects to login.
 */
export async function apiFetch<T>(path: string, init: RequestInit = {}): Promise<T> {
  // Get the current Supabase session to extract the access token.
  const { data } = await supabase.auth.getSession()

  const headers = new Headers(init.headers)
  headers.set('Content-Type', 'application/json')

  // Attach the Supabase JWT if the user is authenticated.
  const token = data.session?.access_token
  if (token) headers.set('Authorization', `Bearer ${token}`)

  const res = await fetch(`${baseUrl}${path}`, { ...init, headers })

  // On 401, the token has expired or the profile was not found – sign out.
  if (res.status === 401) {
    await supabase.auth.signOut()
    throw new Error('Session expired. Please log in again.')
  }

  // Attempt JSON parse; fall back to empty object for 204 No Content.
  const json = await res.json().catch(() => ({}))

  // Surface backend error message to the caller.
  if (!res.ok) throw new Error((json as ApiError).message || res.statusText)

  return json as T
}
