import { Link, useLocation } from 'react-router-dom';
import { BookOpen, Sun, Moon, Monitor, Menu } from 'lucide-react';
import { useThemeStore } from '@/stores/themeStore';
import { useAuthStore } from '@/stores/authStore';
import { useUIStore } from '@/stores/uiStore';

export default function Navbar() {
  const { theme, setTheme } = useThemeStore();
  const user = useAuthStore((s) => s.user);
  const toggleMobileSidebar = useUIStore((s) => s.toggleMobileSidebar);
  const location = useLocation();

  const themeIcons = { light: Sun, dark: Moon, system: Monitor };
  const nextTheme = { light: 'dark', dark: 'system', system: 'light' } as const;
  const ThemeIcon = themeIcons[theme];

  const isAuthPage = ['/login', '/register', '/wx-login'].includes(location.pathname);
  if (isAuthPage) return null;

  return (
    <nav className="fixed top-0 inset-x-0 z-50 glass-nav h-16 lg:h-[72px]">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-full flex items-center justify-between">
        {/* 左侧 Logo + 移动端菜单按钮 */}
        <div className="flex items-center gap-3">
          <button onClick={toggleMobileSidebar} className="lg:hidden w-9 h-9 flex items-center justify-center rounded-xl hover:bg-[var(--color-surface-tertiary)] transition-colors">
            <Menu className="w-5 h-5 text-[var(--color-content)]" />
          </button>
          <Link to="/" className="flex items-center gap-3">
            <div className="w-9 h-9 bg-gradient-to-br from-blue-500 to-violet-600 rounded-xl flex items-center justify-center shadow-lg">
              <BookOpen className="w-5 h-5 text-white" />
            </div>
            <span className="text-lg font-bold text-[var(--color-content)] hidden sm:block">学英文</span>
          </Link>
        </div>

        {/* 右侧操作 */}
        <div className="flex items-center gap-3">
          <button
            onClick={() => setTheme(nextTheme[theme])}
            className="w-9 h-9 rounded-xl flex items-center justify-center hover:bg-[var(--color-surface-tertiary)] transition-colors"
            title={`当前: ${theme === 'light' ? '日间' : theme === 'dark' ? '夜间' : '跟随系统'}`}
          >
            <ThemeIcon className="w-5 h-5 text-[var(--color-content-secondary)]" />
          </button>

          {user && (
            <Link to="/profile" className="flex items-center gap-2">
              <div className="w-8 h-8 rounded-full bg-gradient-to-br from-blue-400 to-violet-500 flex items-center justify-center text-white text-sm font-semibold">
                {user.name?.charAt(0) || 'U'}
              </div>
              <span className="text-sm font-medium text-[var(--color-content)] hidden md:block">{user.name}</span>
            </Link>
          )}
        </div>
      </div>
    </nav>
  );
}
