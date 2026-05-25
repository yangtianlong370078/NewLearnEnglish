import { NavLink } from 'react-router-dom';
import { Home, BookOpen, Headphones, PenTool, Mic, Globe, FolderOpen, BarChart3, Settings, X } from 'lucide-react';
import { cn } from '@/lib/utils';
import { useAuthStore } from '@/stores/authStore';
import { useUIStore } from '@/stores/uiStore';
import Progress from '@/components/ui/Progress';

const menuGroups = [
  {
    items: [
      { to: '/', icon: Home, label: '学习首页' },
      { to: '/words', icon: BookOpen, label: '单词学习' },
      { to: '/hearing', icon: Headphones, label: '听力训练' },
      { to: '/exams', icon: PenTool, label: '智能考试' },
      { to: '/speech', icon: Mic, label: '口语评测' },
      { to: '/translate', icon: Globe, label: '翻译查词' },
    ],
  },
  {
    title: '管理',
    items: [
      { to: '/courses', icon: FolderOpen, label: '我的课程' },
      { to: '/statistics', icon: BarChart3, label: '学习统计' },
      { to: '/profile', icon: Settings, label: '设置' },
    ],
  },
];

function SidebarContent() {
  const user = useAuthStore((s) => s.user);

  return (
    <>
      {/* 用户信息 */}
      {user && (
        <div className="p-5 border-b border-[var(--color-border)]">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-full ring-2 ring-blue-500/20 bg-gradient-to-br from-blue-400 to-violet-500 flex items-center justify-center text-white font-semibold">
              {user.name?.charAt(0) || 'U'}
            </div>
            <div>
              <p className="font-semibold text-sm text-[var(--color-content)]">{user.name}</p>
              <p className="text-xs text-[var(--color-content-tertiary)]">每天进步一点点</p>
            </div>
          </div>
        </div>
      )}

      {/* 导航菜单 */}
      <nav className="flex-1 p-3 space-y-1 overflow-y-auto">
        {menuGroups.map((group, gi) => (
          <div key={gi}>
            {group.title && (
              <div className="pt-4 pb-2">
                <p className="text-xs font-medium text-[var(--color-content-tertiary)] uppercase tracking-wider px-3">
                  {group.title}
                </p>
              </div>
            )}
            {group.items.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.to === '/'}
                className={({ isActive }) => cn(
                  'flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all duration-200',
                  isActive
                    ? 'bg-blue-50 text-blue-600 dark:bg-blue-900/30 dark:text-blue-400'
                    : 'text-[var(--color-content-secondary)] hover:bg-[var(--color-surface-tertiary)] hover:text-[var(--color-content)]'
                )}
              >
                <item.icon className="w-5 h-5" />
                {item.label}
              </NavLink>
            ))}
          </div>
        ))}
      </nav>

      {/* 底部今日目标 */}
      <div className="p-4 border-t border-[var(--color-border)]">
        <div className="bg-gradient-to-r from-blue-50 to-violet-50 dark:from-blue-900/20 dark:to-violet-900/20 rounded-xl p-4">
          <p className="text-sm font-semibold mb-1 text-[var(--color-content)]">今日目标</p>
          <div className="flex items-center gap-2 mb-2">
            <Progress value={68} className="flex-1" />
            <span className="text-xs font-medium text-[var(--color-content)]">68%</span>
          </div>
          <p className="text-xs text-[var(--color-content-tertiary)]">继续加油！</p>
        </div>
      </div>
    </>
  );
}

export default function Sidebar() {
  const mobileSidebarOpen = useUIStore((s) => s.mobileSidebarOpen);
  const closeMobileSidebar = useUIStore((s) => s.closeMobileSidebar);

  return (
    <>
      {/* 桌面端固定侧边栏 */}
      <aside className="fixed left-0 top-[72px] bottom-0 w-[260px] bg-[var(--color-surface)] border-r border-[var(--color-border)] hidden lg:flex flex-col z-40">
        <SidebarContent />
      </aside>

      {/* 移动端抽屉侧边栏 */}
      {mobileSidebarOpen && (
        <div className="fixed inset-0 z-50 lg:hidden">
          <div className="absolute inset-0 bg-[var(--overlay-bg)] animate-fade-in" onClick={closeMobileSidebar} />
          <aside className="absolute left-0 top-0 bottom-0 w-[280px] bg-[var(--color-surface)] flex flex-col animate-fade-in-left shadow-modal">
            <div className="flex items-center justify-between p-4 border-b border-[var(--color-border)]">
              <span className="font-bold text-[var(--color-content)]">菜单</span>
              <button onClick={closeMobileSidebar} className="w-8 h-8 rounded-lg flex items-center justify-center hover:bg-[var(--color-surface-tertiary)]">
                <X className="w-5 h-5" />
              </button>
            </div>
            <SidebarContent />
          </aside>
        </div>
      )}
    </>
  );
}
