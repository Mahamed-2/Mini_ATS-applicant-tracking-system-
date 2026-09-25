<script setup lang="ts">
/**
 * LanguageSwitcher.vue – Bilingual Locale Switcher (EN / SV)
 * Smooth pill toggle allowing instant switching between English and Swedish.
 */
import { useI18n, type Locale } from '@/i18n';

interface Props {
  variant?: 'pill' | 'button';
  size?: 'sm' | 'md';
}

withDefaults(defineProps<Props>(), {
  variant: 'pill',
  size: 'sm',
});

const { locale, setLocale } = useI18n();

function selectLocale(lang: Locale) {
  setLocale(lang);
}
</script>

<template>
  <div
    class="inline-flex items-center p-0.5 rounded-full bg-surface-hover/80 border border-border-subtle backdrop-blur-md select-none"
    role="group"
    aria-label="Language selection"
  >
    <button
      type="button"
      :class="[
        'px-2.5 py-1 text-[11px] font-semibold rounded-full transition-all duration-150 cursor-pointer',
        locale === 'en'
          ? 'bg-primary text-white shadow-sm shadow-primary/25'
          : 'text-text-muted hover:text-text-primary'
      ]"
      :aria-pressed="locale === 'en'"
      title="Switch to English"
      @click="selectLocale('en')"
    >
      EN
    </button>
    <button
      type="button"
      :class="[
        'px-2.5 py-1 text-[11px] font-semibold rounded-full transition-all duration-150 cursor-pointer',
        locale === 'sv'
          ? 'bg-primary text-white shadow-sm shadow-primary/25'
          : 'text-text-muted hover:text-text-primary'
      ]"
      :aria-pressed="locale === 'sv'"
      title="Växla till svenska"
      @click="selectLocale('sv')"
    >
      SV
    </button>
  </div>
</template>
