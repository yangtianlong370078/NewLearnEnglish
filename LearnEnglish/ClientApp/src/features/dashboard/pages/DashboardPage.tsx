import { useQuery } from '@tanstack/react-query';
import { BookOpen, CheckCircle, Flame, Trophy } from 'lucide-react';
import { AreaChart, Area, XAxis, YAxis, Tooltip, ResponsiveContainer } from 'recharts';
import { StatCard, Card, Skeleton } from '@/components/ui';
import { useAuthStore } from '@/stores/authStore';
import { getUserInfo } from '../api';
import { queryLearnCount, getMonthlyStats } from '@/features/statistics/api';
import { formatNumber } from '@/lib/utils';
import type { StatisticsLearnGroup } from '@/types';

export default function DashboardPage() {
  const user = useAuthStore((s) => s.user);

  const { data: userInfo } = useQuery({ queryKey: ['userInfo'], queryFn: getUserInfo });
  const { data: learnCount } = useQuery({ queryKey: ['learnCount'], queryFn: queryLearnCount });
  const { data: monthlyStats } = useQuery({ queryKey: ['monthlyStats'], queryFn: getMonthlyStats });

  // 后端返回 StatisticsLearnGroup[]，每组是一个月的数据
  const chartData = Array.isArray(monthlyStats) ? monthlyStats.map((g: StatisticsLearnGroup) => ({
    name: `${g.year}-${String(g.month).padStart(2, '0')}`,
    words: g.totalcount,
  })) : [];

  return (
    <div className="space-y-6 animate-fade-in">
      {/* 欢迎区 */}
      <div>
        <h1 className="text-2xl lg:text-3xl font-bold text-[var(--color-content)]">
          欢迎回来，{user?.name || '同学'} 👋
        </h1>
        <p className="text-[var(--color-content-secondary)] mt-1">
          {userInfo?.Day !== undefined
            ? `账号有效期还剩 ${userInfo.Day} 天`
            : '今天也要加油学习哦！'}
        </p>
      </div>

      {/* 指标卡片 */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          icon={<BookOpen className="w-6 h-6 text-blue-500" />}
          iconBg="bg-blue-50 dark:bg-blue-900/30"
          label="已学单词"
          value={learnCount?.count ? formatNumber(learnCount.count) : '0'}
          trend={learnCount?.bzcount ? `本周+${learnCount.bzcount}` : undefined}
          trendUp
        />
        <StatCard
          icon={<CheckCircle className="w-6 h-6 text-emerald-500" />}
          iconBg="bg-emerald-50 dark:bg-emerald-900/30"
          label="本周学习"
          value={learnCount?.bzcount ?? 0}
        />
        <StatCard
          icon={<Flame className="w-6 h-6 text-amber-500" />}
          iconBg="bg-amber-50 dark:bg-amber-900/30"
          label="账号剩余"
          value={userInfo?.Day !== undefined ? `${userInfo.Day}天` : '--'}
        />
        <StatCard
          icon={<Trophy className="w-6 h-6 text-violet-500" />}
          iconBg="bg-violet-50 dark:bg-violet-900/30"
          label="学习者"
          value={learnCount?.username || user?.name || '--'}
        />
      </div>

      {/* 学习趋势图 */}
      <Card>
        <div className="flex items-center justify-between mb-6">
          <div>
            <h3 className="font-semibold text-[var(--color-content)]">学习趋势</h3>
            <p className="text-sm text-[var(--color-content-tertiary)] mt-0.5">月度统计</p>
          </div>
        </div>
        <div className="h-64">
          {chartData.length > 0 ? (
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={chartData}>
                <defs>
                  <linearGradient id="gradient-area" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor="#3b82f6" stopOpacity={0.2} />
                    <stop offset="100%" stopColor="#3b82f6" stopOpacity={0} />
                  </linearGradient>
                </defs>
                <XAxis dataKey="name" stroke="var(--color-content-tertiary)" fontSize={12} tickLine={false} axisLine={false} />
                <YAxis stroke="var(--color-content-tertiary)" fontSize={12} tickLine={false} axisLine={false} />
                <Tooltip
                  contentStyle={{
                    background: 'var(--card-bg)',
                    border: '1px solid var(--color-border)',
                    borderRadius: '12px',
                    boxShadow: '0 4px 12px rgba(0,0,0,0.1)',
                  }}
                />
                <Area type="monotone" dataKey="words" stroke="#3b82f6" strokeWidth={2} fill="url(#gradient-area)" />
              </AreaChart>
            </ResponsiveContainer>
          ) : (
            <div className="h-full flex items-center justify-center">
              <Skeleton className="w-full h-48" />
            </div>
          )}
        </div>
      </Card>
    </div>
  );
}
