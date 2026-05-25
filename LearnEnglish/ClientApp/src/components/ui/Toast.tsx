import { create } from 'zustand';
import { CheckCircle, XCircle, AlertTriangle, Info, X } from 'lucide-react';
import { cn } from '@/lib/utils';

type ToastType = 'success' | 'error' | 'warning' | 'info';

interface Toast {
  id: string;
  type: ToastType;
  title: string;
  message?: string;
}

interface ToastState {
  toasts: Toast[];
  addToast: (toast: Omit<Toast, 'id'>) => void;
  removeToast: (id: string) => void;
}

const useToastStore = create<ToastState>((set) => ({
  toasts: [],
  addToast: (toast) => {
    const id = Math.random().toString(36).slice(2);
    set((s) => ({ toasts: [...s.toasts, { ...toast, id }] }));
    const duration = toast.type === 'error' ? 5000 : 3000;
    setTimeout(() => {
      set((s) => ({ toasts: s.toasts.filter((t) => t.id !== id) }));
    }, duration);
  },
  removeToast: (id) => set((s) => ({ toasts: s.toasts.filter((t) => t.id !== id) })),
}));

// 全局 toast 方法
export const toast = {
  success: (title: string, message?: string) => useToastStore.getState().addToast({ type: 'success', title, message }),
  error: (title: string, message?: string) => useToastStore.getState().addToast({ type: 'error', title, message }),
  warning: (title: string, message?: string) => useToastStore.getState().addToast({ type: 'warning', title, message }),
  info: (title: string, message?: string) => useToastStore.getState().addToast({ type: 'info', title, message }),
};

const iconMap = {
  success: CheckCircle,
  error: XCircle,
  warning: AlertTriangle,
  info: Info,
};

const bgMap = {
  success: 'bg-emerald-50 dark:bg-emerald-900/30',
  error: 'bg-rose-50 dark:bg-rose-900/30',
  warning: 'bg-amber-50 dark:bg-amber-900/30',
  info: 'bg-cyan-50 dark:bg-cyan-900/30',
};

const colorMap = {
  success: 'text-emerald-500',
  error: 'text-rose-500',
  warning: 'text-amber-500',
  info: 'text-cyan-500',
};

export function ToastContainer() {
  const toasts = useToastStore((s) => s.toasts);
  const removeToast = useToastStore((s) => s.removeToast);

  return (
    <div className="fixed top-20 right-4 z-[100] space-y-3">
      {toasts.map((t) => {
        const Icon = iconMap[t.type];
        return (
          <div
            key={t.id}
            className="flex items-center gap-3 bg-[var(--card-bg)] rounded-2xl shadow-float border border-[var(--color-border)] px-5 py-4 min-w-[320px] animate-slide-in-right"
          >
            <div className={cn('w-9 h-9 rounded-xl flex items-center justify-center shrink-0', bgMap[t.type])}>
              <Icon className={cn('w-5 h-5', colorMap[t.type])} />
            </div>
            <div className="flex-1 min-w-0">
              <p className="font-medium text-sm text-[var(--color-content)]">{t.title}</p>
              {t.message && <p className="text-xs text-[var(--color-content-tertiary)] mt-0.5 truncate">{t.message}</p>}
            </div>
            <button onClick={() => removeToast(t.id)} className="text-[var(--color-content-tertiary)] hover:text-[var(--color-content)]">
              <X className="w-4 h-4" />
            </button>
          </div>
        );
      })}
    </div>
  );
}
