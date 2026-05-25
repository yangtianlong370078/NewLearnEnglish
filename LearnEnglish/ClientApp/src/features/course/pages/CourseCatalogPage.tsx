import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, BookOpen } from 'lucide-react';
import { Card, Button, EmptyState } from '@/components/ui';
import { toast } from '@/components/ui/Toast';
import { getMyCourses, addCourse } from '../api';
import { isApiSuccess } from '@/lib/utils';
import type { Course } from '@/types';

export default function CourseCatalogPage() {
  const queryClient = useQueryClient();
  const { data, isLoading } = useQuery({ queryKey: ['allCourses', 0], queryFn: () => getMyCourses(0) });

  const courses: Course[] = data?.data ?? [];

  const addMutation = useMutation({
    mutationFn: (courseId: number) => addCourse(courseId),
    onSuccess: (res) => {
      if (isApiSuccess(res)) {
        toast.success('已加入学习');
        queryClient.invalidateQueries({ queryKey: ['allCourses'] });
      } else {
        toast.error('加入失败', res.msg);
      }
    },
  });

  if (isLoading) {
    return <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 animate-pulse">
      {Array.from({length: 6}).map((_, i) => <div key={i} className="h-40 rounded-2xl bg-[var(--color-surface-tertiary)]" />)}
    </div>;
  }

  return (
    <div className="space-y-6 animate-fade-in">
      <h1 className="text-2xl font-bold text-[var(--color-content)]">课程目录</h1>

      {courses.length === 0 ? (
        <EmptyState title="暂无课程" description="还没有可用的课程" />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {courses.map((course) => (
            <Card key={course.courseId} hoverable className="group">
              <div className="flex items-start justify-between mb-4">
                <div className="w-12 h-12 rounded-xl bg-indigo-50 dark:bg-indigo-900/30 flex items-center justify-center group-hover:scale-110 transition-transform">
                  <BookOpen className="w-6 h-6 text-indigo-500" />
                </div>
              </div>
              <h3 className="text-lg font-semibold text-[var(--color-content)] mb-2">{course.courseName}</h3>
              <p className="text-sm text-[var(--color-content-tertiary)] mb-4">
                {course.WordsCount ?? 0} 个单词
              </p>
              <Button
                variant="secondary"
                size="sm"
                className="w-full"
                icon={<Plus className="w-4 h-4" />}
                onClick={() => addMutation.mutate(course.courseId)}
                loading={addMutation.isPending}
              >
                加入学习
              </Button>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
