/**
 * useToast.ts – Lightweight Reactive Toast Notification Manager
 * Source of Truth: docs/DESIGN.md & .claude/skills/ats-orchestrator/reference/DESIGN_TOKENS.md
 */
import { ref } from 'vue';

export interface ToastItem {
  id: string;
  title: string;
  description?: string;
  type?: 'success' | 'danger' | 'warning' | 'info' | 'ai';
  duration?: number;
}

const toasts = ref<ToastItem[]>([]);

export function useToast() {
  function toast(item: Omit<ToastItem, 'id'>) {
    const id = Math.random().toString(36).slice(2, 9);
    const duration = item.duration ?? 4000;
    const toastItem: ToastItem = { ...item, id };

    toasts.value.push(toastItem);

    if (duration > 0) {
      setTimeout(() => {
        dismiss(id);
      }, duration);
    }
    return id;
  }

  function dismiss(id: string) {
    toasts.value = toasts.value.filter((t) => t.id !== id);
  }

  function success(title: string, description?: string) {
    return toast({ title, description, type: 'success' });
  }

  function error(title: string, description?: string) {
    return toast({ title, description, type: 'danger' });
  }

  function warning(title: string, description?: string) {
    return toast({ title, description, type: 'warning' });
  }

  function info(title: string, description?: string) {
    return toast({ title, description, type: 'info' });
  }

  function ai(title: string, description?: string) {
    return toast({ title, description, type: 'ai' });
  }

  return {
    toasts,
    toast,
    dismiss,
    success,
    error,
    warning,
    info,
    ai,
  };
}
