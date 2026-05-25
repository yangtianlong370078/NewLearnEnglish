import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Search, Volume2, Heart } from 'lucide-react';
import { Input, Card, Button, EmptyState, Badge } from '@/components/ui';
import { WordCardSkeleton } from '@/components/ui/Skeleton';
import { toast } from '@/components/ui/Toast';
import { useAuthStore } from '@/stores/authStore';
import { getWordList, setCollect } from '../api';
import { isApiSuccess } from '@/lib/utils';
import type { Word } from '@/types';

export default function WordListPage() {
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const courseId = user?.courseId ?? 0;

  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState<number | undefined>(undefined);

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['wordList', courseId, page, statusFilter, search],
    queryFn: () => getWordList({ kc: courseId, index: page, zt: statusFilter, name: search || undefined }),
    enabled: courseId > 0,
  });

  // 解析后端返回
  const words: Word[] = data?.data ?? [];
  const total = data?.total ?? 0;
  const pageSize = data?.pageSize ?? 30;
  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  const handleCollect = async (lexiconId: number, current: boolean) => {
    const res = await setCollect(lexiconId, !current);
    if (isApiSuccess(res)) {
      toast.success(current ? '已取消收藏' : '已收藏');
      refetch();
    }
  };

  const statusTabs = [
    { label: '全部', value: undefined },
    { label: '未学', value: 0 },
    { label: '已学', value: 1 },
    { label: '熟悉', value: 2 },
  ];

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-[var(--color-content)]">单词学习</h1>
        <Button variant="secondary" size="sm" onClick={() => navigate(`/words/table/${courseId}`)}>
          表格视图
        </Button>
      </div>

      {/* 搜索和筛选 */}
      <div className="flex flex-col sm:flex-row gap-4">
        <div className="flex-1">
          <Input
            placeholder="搜索单词..."
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            icon={<Search className="w-4 h-4" />}
          />
        </div>
        <div className="flex gap-2">
          {statusTabs.map((tab) => (
            <button
              key={tab.label}
              onClick={() => { setStatusFilter(tab.value); setPage(1); }}
              className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
                statusFilter === tab.value
                  ? 'bg-blue-50 text-blue-600 dark:bg-blue-900/30 dark:text-blue-400'
                  : 'text-[var(--color-content-tertiary)] hover:bg-[var(--color-surface-tertiary)]'
              }`}
            >
              {tab.label}
            </button>
          ))}
        </div>
      </div>

      {/* 单词网格 */}
      {isLoading ? (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-4">
          {Array.from({ length: 12 }).map((_, i) => <WordCardSkeleton key={i} />)}
        </div>
      ) : words.length === 0 ? (
        <EmptyState title="暂无单词" description="当前课程还没有单词，去添加一些吧" actionLabel="去课程管理" onAction={() => navigate('/courses')} />
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-4">
          {words.map((word) => (
            <Card
              key={word.Id || word.LexiconId}
              hoverable
              className="group relative overflow-hidden p-0"
              onClick={() => navigate(`/words/${encodeURIComponent(word.en)}`)}
            >
              {/* 顶部渐变装饰 */}
              <div className="h-1 bg-gradient-to-r from-blue-500 to-violet-500 transform origin-left scale-x-0 group-hover:scale-x-100 transition-transform duration-500" />
              <div className="p-5">
                <div className="flex items-start justify-between mb-1">
                  <h3 className="text-lg font-bold text-[var(--color-content)] word-display">{word.en}</h3>
                  <Badge variant={word.Zt === 2 ? 'success' : word.Zt === 1 ? 'default' : 'warning'}>
                    {word.Zt === 2 ? '熟悉' : word.Zt === 1 ? '已学' : '未学'}
                  </Badge>
                </div>
                <p className="text-sm text-[var(--color-content-secondary)] line-clamp-2">{word.cn}</p>
                <div className="flex items-center justify-between mt-4 pt-3 border-t border-[var(--color-border-secondary)]">
                  <button
                    onClick={(e) => { e.stopPropagation(); }}
                    className="text-[var(--color-content-tertiary)] hover:text-blue-500 transition-colors"
                  >
                    <Volume2 className="w-5 h-5" />
                  </button>
                  <button
                    onClick={(e) => { e.stopPropagation(); handleCollect(word.LexiconId, word.isCollect); }}
                    className={`transition-colors ${word.isCollect ? 'text-rose-500' : 'text-[var(--color-content-tertiary)] hover:text-rose-500'}`}
                  >
                    <Heart className="w-5 h-5" fill={word.isCollect ? 'currentColor' : 'none'} />
                  </button>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}

      {/* 分页 */}
      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-2 pt-4">
          <Button variant="ghost" size="sm" onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page === 1}>上一页</Button>
          <span className="text-sm text-[var(--color-content-tertiary)]">第 {page} / {totalPages} 页</span>
          <Button variant="ghost" size="sm" onClick={() => setPage(p => Math.min(totalPages, p + 1))} disabled={page === totalPages}>下一页</Button>
        </div>
      )}
    </div>
  );
}
