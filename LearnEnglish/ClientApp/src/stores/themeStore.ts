import { create } from 'zustand';
import { persist } from 'zustand/middleware';

type Theme = 'light' | 'dark' | 'system';

interface ThemeState {
  theme: Theme;
  setTheme: (theme: Theme) => void;
}

function applyTheme(theme: Theme) {
  const root = document.documentElement;
  root.classList.remove('dark');
  root.removeAttribute('data-theme');

  if (theme === 'system') {
    const isDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    root.setAttribute('data-theme', isDark ? 'dark' : 'light');
    if (isDark) root.classList.add('dark');
  } else {
    root.setAttribute('data-theme', theme);
    if (theme === 'dark') root.classList.add('dark');
  }
}

export const useThemeStore = create<ThemeState>()(
  persist(
    (set) => ({
      theme: 'system',
      setTheme: (theme: Theme) => {
        applyTheme(theme);
        set({ theme });
      },
    }),
    { name: 'theme-storage' }
  )
);

// 初始化主题
const savedTheme = (JSON.parse(localStorage.getItem('theme-storage') || '{}')?.state?.theme as Theme) || 'system';
applyTheme(savedTheme);

// 监听系统主题变化
window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
  const current = useThemeStore.getState().theme;
  if (current === 'system') {
    applyTheme('system');
  }
});
