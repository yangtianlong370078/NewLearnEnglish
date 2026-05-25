import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { BookOpen, Headphones } from 'lucide-react';
import { Card, Button, Progress, EmptyState } from '@/components/ui';
import { getMyCourses } from '../api';
import type { Course } from '@/types';

export default function MyCoursesPage() {
  const navigate = useNavigate();
  const [mode, setMode] = useState<'word' | 'hearing'>('word');
  const type = mode === 'word' ? 1 : 2;

  const { data, isLoading } = useQuery({
    queryKey: ['myCourses', type],
    queryFn: () => getMyCourses(type),
  });

  const courses: Course[] = data?.data ?? [];

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-[var(--color-content)]">我的课程</h1>
        <Button variant="secondary" size="sm" onClick={() => navigate('/courses/catalog')}>浏览更多</Button>
      </div>

      {/* 模式切换 */}
      <div className="flex gap-2">
        <button
          onClick={() => setMode('word')}
          className={`flex items-center gap-2 px-4 py-2 rounded-xl text-sm font-medium transition-colors ${
            mode === 'word' ? 'bg-blue-50 text-blue-600 dark:bg-blue-900/30 dark:text-blue-400' : 'text-[var(--color-content-tertiary)] hover:bg-[var(--color-surface-tertiary)]'
          }`}
        >
          <BookOpen className="w-4 h-4" /> 学单词
        </button>
        <button
          onClick={() => setMode('hearing')}
          className={`flex items-center gap-2 px-4 py-2 rounded-xl text-sm font-medium transition-colors ${
            mode === 'hearing' ? 'bg-emerald-50 text-emerald-600 dark:bg-emerald-900/30 dark:text-emerald-400' : 'text-[var(--color-content-tertiary)] hover:bg-[var(--color-surface-tertiary)]'
          }`}
        >
          <Headphones className="w-4 h-4" /> 练听力
        </button>
      </div>

      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 animate-pulse">
          {Array.from({length: 3}).map((_, i) => <div key={i} className="h-40 rounded-2xl bg-[var(--color-surface-tertiary)]" />)}
        </div>
      ) : courses.length === 0 ? (
        <EmptyState title="还没有课程" description="去课程目录添加感兴趣的课程吧" actionLabel="浏览课程" onAction={() => navigate('/courses/catalog')} />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {courses.map((course) => {
            const progress = parseFloat(course.Percentage) || 0;
            return (
              <Card
                key={course.courseId}
                hoverable
                onClick={() => navigate(mode === 'word' ? `/courses/${course.courseId}` : `/courses/${course.courseId}/hearing`)}
              >
                <h3 className="text-lg font-semibold text-[var(--color-content)] mb-2">{course.courseName}</h3>
                <p className="text-sm text-[var(--color-content-tertiary)] mb-4">
                  {course.DoneCount ?? 0} / {course.WordsCount ?? 0} 词
                </p>
                <div className="flex items-center gap-2">
                  <Progress value={progress} className="flex-1" />
                  <span className="text-xs font-medium text-[var(--color-content)]">{progress.toFixed(0)}%</span>
                </div>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
