<script setup lang="ts">
/**
 * LoginBackdrop.vue – Professional Background with Asset Loading & Pure CSS/SVG Fallback
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/LOGIN.md
 * Tokens: Canvas #f8f9ff, Primary #1d68f0, Slate-900 #0f172a, Violet #7c3aed
 */
import { ref } from 'vue';

// Local asset bundled and hashed by Vite
import loginBgAsset from '@/assets/login-background.jpg';

const imageFailed = ref(false);

function handleImageError() {
  imageFailed.value = true;
}
</script>

<template>
  <div
    class="absolute inset-0 w-full h-full overflow-hidden select-none pointer-events-none"
    role="img"
    aria-label="Abstract architectural talent network background"
  >
    <!-- 1. Primary Layer: High-res bundled photographic asset -->
    <div
      v-if="!imageFailed"
      class="absolute inset-0 w-full h-full bg-cover bg-center transition-transform duration-1000 ease-out motion-safe:scale-105 motion-reduce:scale-100"
      :style="{ backgroundImage: `url(${loginBgAsset})` }"
    >
      <!-- Hidden image element to detect load error -->
      <img
        :src="loginBgAsset"
        alt=""
        class="hidden"
        @error="handleImageError"
      />
    </div>

    <!-- 2. Resilient Pure CSS + SVG Fallback Layer (rendered if asset fails to load) -->
    <div
      v-else
      class="absolute inset-0 w-full h-full bg-slate-950 overflow-hidden"
    >
      <!-- Layered ambient radial gradients -->
      <div
        class="absolute -top-[20%] -left-[10%] w-[70vw] h-[70vw] rounded-full opacity-30 blur-3xl pointer-events-none"
        style="background: radial-gradient(circle, #1d68f0 0%, rgba(29,104,240,0) 70%);"
      />
      <div
        class="absolute -bottom-[20%] -right-[10%] w-[60vw] h-[60vw] rounded-full opacity-20 blur-3xl pointer-events-none"
        style="background: radial-gradient(circle, #7c3aed 0%, rgba(124,58,237,0) 70%);"
      />

      <!-- Geometric SVG constellation network -->
      <svg
        class="absolute inset-0 w-full h-full opacity-25"
        xmlns="http://www.w3.org/2000/svg"
        width="100%"
        height="100%"
      >
        <defs>
          <pattern
            id="login-grid-pattern"
            width="80"
            height="80"
            patternUnits="userSpaceOnUse"
          >
            <circle cx="40" cy="40" r="1.5" fill="#94a3b8" opacity="0.6" />
            <path
              d="M 40 40 L 80 80 M 40 40 L 0 80"
              stroke="#64748b"
              stroke-width="0.75"
              stroke-dasharray="2 4"
              opacity="0.4"
            />
          </pattern>
        </defs>
        <rect width="100%" height="100%" fill="url(#login-grid-pattern)" />
      </svg>
    </div>

    <!-- 3. Darkening Gradient Overlay for WCAG AA Contrast -->
    <!-- Ensures light glass card and crisp text read clearly on desktop and mobile -->
    <div
      class="absolute inset-0 w-full h-full"
      style="background: linear-gradient(115deg, rgba(15, 23, 42, 0.78) 0%, rgba(15, 23, 42, 0.45) 55%, rgba(15, 23, 42, 0.82) 100%);"
    />

    <!-- Subtle vignette effect around the borders -->
    <div
      class="absolute inset-0 w-full h-full shadow-[inset_0_0_120px_rgba(15,23,42,0.8)]"
    />
  </div>
</template>
