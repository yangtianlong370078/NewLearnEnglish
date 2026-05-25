import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { ArrowLeft } from 'lucide-react';
import { Button, Table } from '@/components/ui';
import { getWordList } from '../api';

export default function WordTablePage() {
  const { courseId } = useParams<{ courseId: string }>();
  const navigate = useNavigate();

  const { data, isLoading } = useQuery({
    queryKey: ['wordTable', courseId],
    queryFn: () => getWordList({ kc: Number(courseId), pageSize: 100 }),
    enabled: !!courseId,
  });

  const words = data?.data ?? [];

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate(-1)} icon={<ArrowLeft className="w-4 h-4" />}>
          返回
        </Button>
        <h1 className="text-2xl font-bold text-[var(--color-content)]">单词表格</h1>
      </div>

      <Table
        columns={[
          { key: 'en', title: '单词', render: (w) => <span className="font-semibold word-display">{w.en}</span> },
          { key: 'Value', title: '音标', render: (w) => <span className="phonetic">{w.Value || '--'}</span> },
          { key: 'cn', title: '释义' },
          {
            key: 'Zt', title: '状态',
            render: (w) => (
              <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${
                w.Zt === 2 ? 'bg-emerald-50 text-emerald-600 dark:bg-emerald-900/30 dark:text-emerald-400' :
                w.Zt === 1 ? 'bg-blue-50 text-blue-600 dark:bg-blue-900/30 dark:text-blue-400' :
                'bg-gray-100 text-gray-500 dark:bg-gray-800 dark:text-gray-400'
              }`}>
                {w.Zt === 2 ? '熟悉' : w.Zt === 1 ? '已学' : '未学'}
              </span>
            ),
          },
          {
            key: 'actions', title: '操作',
            render: (w) => (
              <Button variant="ghost" size="xs" onClick={() => navigate(`/words/${encodeURIComponent(w.en)}`)}>
                详情
              </Button>
            ),
          },
        ]}
        data={words}
        rowKey={(w) => w.Id || w.LexiconId}
        emptyText={isLoading ? '加载中...' : '暂无数据'}
      />
    </div>
  );
}
