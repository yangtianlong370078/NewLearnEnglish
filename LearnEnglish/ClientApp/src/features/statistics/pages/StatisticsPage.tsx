import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { BarChart3, Target, Calendar, Save } from 'lucide-react';
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { Button, Card, Input } from '@/components/ui';
import { StatCard } from '@/components/ui/Card';
import { toast } from '@/components/ui/Toast';
import { queryLearnCount, getMonthlyStats, saveLearningTask } from '../api';
import { isApiSuccess } from '@/lib/utils';

export default function StatisticsPage() {
  const queryClient = useQueryClient();

  const { data: learnData } = useQuery({
    queryKey: ['learnCount'],
    queryFn: queryLearnCount,
  });

  const { data: monthlyData } = useQuery({
    queryKey: ['monthlyStats'],
    queryFn: getMonthlyStats,
  });

  const stats = learnData ?? {};
  // getMonthlyStats 直接返回 StatisticsLearnGroup[]
  const chartData: Array<{ month: string; count: number }> = Array.isArray(monthlyData)
    ? monthlyData.map((g: { year: number; month: number; totalcount: number }) => ({
        month: `${g.year}-${String(g.month).padStart(2, '0')}`,
        count: g.totalcount,
      }))
    : [];

  // 学习任务表单
  const [taskCount, setTaskCount] = useState(20);
  const [taskDate, setTaskDate] = useState(() => new Date().toISOString().slice(0, 10));

  const taskMutation = useMutation({
    mutationFn: () =>
      saveLearningTask({
        id: 0,
        count: taskCount,
        date: taskDate,
        type: 1,
        weekend: new Date(taskDate).getDay() === 0 || new Date(taskDate).getDay() === 6 ? 1 : 0,
      }),
    onSuccess: (res) => {
      if (isApiSuccess(res)) {
        toast.success('学习任务已保存');
        queryClient.invalidateQueries({ queryKey: ['learnCount'] });
      } else {
        toast.error('保存失败', res.msg);
      }
    },
  });

  return (
    <div className="space-y-6 animate-fade-in">
      <h1 className="text-2xl font-bold text-[var(--color-content)]">学习统计</h1>

      {/* 统计卡片 */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <StatCard
          title="总学习数"
          value={stats.count ?? 0}
          icon={<BarChart3 className="w-5 h-5" />}
          color="primary"
        />
        <StatCard
          title="本周学习数"
          value={stats.bzcount ?? 0}
          icon={<Target className="w-5 h-5" />}
          color="success"
        />
        <StatCard
          title="用户"
          value={stats.username ?? '-'}
          icon={<Calendar className="w-5 h-5" />}
          color="secondary"
        />
      </div>

      {/* 月度趋势图 */}
      <Card>
        <h2 className="text-lg font-semibold text-[var(--color-content)] mb-4">学习趋势</h2>
        {chartData.length > 0 ? (
          <ResponsiveContainer width="100%" height={300}>
            <AreaChart data={chartData}>
              <defs>
                <linearGradient id="statsGradient" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="var(--color-primary)" stopOpacity={0.3} />
                  <stop offset="95%" stopColor="var(--color-primary)" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" />
              <XAxis dataKey="month" stroke="var(--color-content-tertiary)" fontSize={12} />
              <YAxis stroke="var(--color-content-tertiary)" fontSize={12} />
              <Tooltip
                contentStyle={{
                  backgroundColor: 'var(--color-surface)',
                  border: '1px solid var(--color-border)',
                  borderRadius: '12px',
                }}
              />
              <Area
                type="monotone"
                dataKey="count"
                stroke="var(--color-primary)"
                fill="url(#statsGradient)"
                strokeWidth={2}
                name="学习数量"
              />
            </AreaChart>
          </ResponsiveContainer>
        ) : (
          <div className="h-64 flex items-center justify-center text-[var(--color-content-secondary)]">
            暂无数据
          </div>
        )}
      </Card>

      {/* 设定学习任务 */}
      <Card>
        <h2 className="text-lg font-semibold text-[var(--color-content)] mb-4">设定学习任务</h2>
        <div className="grid gap-4 sm:grid-cols-3 items-end">
          <div>
            <label className="block text-sm font-medium text-[var(--color-content)] mb-1.5">每日目标</label>
            <Input
              type="number"
              value={String(taskCount)}
              onChange={(e) => setTaskCount(Number(e.target.value))}
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-[var(--color-content)] mb-1.5">日期</label>
            <Input
              type="date"
              value={taskDate}
              onChange={(e) => setTaskDate(e.target.value)}
            />
          </div>
          <Button
            icon={<Save className="w-4 h-4" />}
            loading={taskMutation.isPending}
            onClick={() => taskMutation.mutate()}
          >
            保存任务
          </Button>
        </div>
      </Card>
    </div>
  );
}
