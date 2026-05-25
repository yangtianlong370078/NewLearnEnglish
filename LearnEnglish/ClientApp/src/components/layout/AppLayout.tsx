import { Outlet, Navigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '@/stores/authStore';
import Navbar from './Navbar';
import Sidebar from './Sidebar';
import MobileTabBar from './MobileTabBar';

export default function AppLayout() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const location = useLocation();

  const publicPaths = ['/login', '/register', '/wx-login'];
  const isPublicPage = publicPaths.includes(location.pathname);

  if (!isAuthenticated && !isPublicPage) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // 认证页面使用简洁布局
  if (isPublicPage) {
    return <Outlet />;
  }

  return (
    <div className="min-h-screen bg-[var(--color-surface)]">
      <Navbar />
      <Sidebar />
      {/* 主内容区 */}
      <main className="pt-16 lg:pt-[72px] lg:pl-[260px] pb-20 lg:pb-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <Outlet />
        </div>
      </main>
      <MobileTabBar />
    </div>
  );
}
