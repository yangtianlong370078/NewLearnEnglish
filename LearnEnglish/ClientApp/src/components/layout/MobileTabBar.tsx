import { NavLink, useLocation } from 'react-router-dom';
import { Home, BookOpen, PenTool, Mic, User } from 'lucide-react';
import { cn } from '@/lib/utils';

const tabs = [
  { to: '/', icon: Home, label: '首页' },
  { to: '/courses', icon: BookOpen, label: '课程' },
  { to: '/words', icon: PenTool, label: '学习', center: true },
  { to: '/speech', icon: Mic, label: '口语' },
  { to: '/profile', icon: User, label: '我的' },
];

export default function MobileTabBar() {
  const location = useLocation();
  const isAuthPage = ['/login', '/register', '/wx-login'].includes(location.pathname);
  if (isAuthPage) return null;

  return (
    <nav className="fixed bottom-0 inset-x-0 z-50 lg:hidden bg-[var(--card-bg)] border-t border-[var(--color-border)] safe-area-bottom">
      <div className="flex items-center justify-around h-16 px-2">
        {tabs.map((tab) => (
          <NavLink
            key={tab.to}
            to={tab.to}
            end={tab.to === '/'}
            className={({ isActive }) => cn(
              'flex flex-col items-center gap-0.5 min-w-[48px]',
              tab.center && 'relative -top-3'
            )}
          >
            {({ isActive }) => (
              <>
                <div className={cn(
                  'flex items-center justify-center transition-all',
                  tab.center
                    ? 'w-14 h-14 rounded-full bg-gradient-to-r from-blue-500 to-violet-600 shadow-lg shadow-blue-500/30 text-white'
                    : cn('w-10 h-10 rounded-xl', isActive ? 'text-blue-600 dark:text-blue-400' : 'text-[var(--color-content-tertiary)]')
                )}>
                  <tab.icon className={cn('w-5 h-5', tab.center && 'w-6 h-6')} />
                </div>
                <span className={cn(
                  'text-[10px] font-medium',
                  tab.center ? 'text-blue-600 dark:text-blue-400 mt-1' : isActive ? 'text-blue-600 dark:text-blue-400' : 'text-[var(--color-content-tertiary)]'
                )}>
                  {tab.label}
                </span>
              </>
            )}
          </NavLink>
        ))}
      </div>
    </nav>
  );
}
