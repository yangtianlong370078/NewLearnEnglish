import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Trash2, PlayCircle, Search } from 'lucide-react';
import { Button, Card, Input, Modal, Badge, EmptyState } from '@/components/ui';
import { toast } from '@/components/ui/Toast';
import { getExamList, createExam, deleteExam } from '../api';
import { isApiSuccess, formatDate } from '@/lib/utils';
import type { Exam } from '@/types';

export default function ExamListPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [examCount, setExamCount] = useState(20);
  const [limitTime, setLimitTime] = useState(30);

  const { data, isLoading } = useQuery({
    queryKey: ['examList', page, search],
    queryFn: () => getExamList(page, search || undefined),
  });

  const exams: Exam[] = data?.data ?? [];
  const total = data?.total ?? 0;
  const pageSize = data?.pageSize ?? 7;

  const createMutation = useMutation({
    mutationFn: () => createExam(examCount, limitTime),
    onSuccess: (res) => {
      if (isApiSuccess(res)) {
        toast.success('测验创建成功');
        setShowCreate(false);
        queryClient.invalidateQueries({ queryKey: ['examList'] });
      } else {
        toast.error('创建失败', res.msg);
      }
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteExam(id),
    onSuccess: (res) => {
      if (isApiSuccess(res)) {
        toast.success('已删除');
        queryClient.invalidateQueries({ queryKey: ['examList'] });
      }
    },
  });

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-[var(--color-content)]">测验列表</h1>
        <Button icon={<Plus className="w-4 h-4" />} onClick={() => setShowCreate(true)}>创建测验</Button>
      </div>

      <div className="max-w-sm">
        <Input
          placeholder="搜索..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
          icon={<Search className="w-4 h-4" />}
        />
      </div>

      {isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {[1, 2, 3].map((i) => (
            <Card key={i} className="animate-pulse h-40" />
          ))}
        </div>
      ) : exams.length === 0 ? (
        <EmptyState title="暂无测验" description="创建一个新测验开始练习吧" actionLabel="创建测验" onAction={() => setShowCreate(true)} />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {exams.map((exam) => {
            const allCompleted = exam.zyCompleted && exam.yzCompleted;
            return (
              <Card key={exam.Id} hoverable className="flex flex-col justify-between">
                <div>
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-lg font-semibold text-[var(--color-content)]">{exam.Name || `测验 #${exam.Id}`}</span>
                    {allCompleted ? <Badge color="green">已完成</Badge> : <Badge color="yellow">进行中</Badge>}
                  </div>
                  <div className="text-sm text-[var(--color-content-secondary)] space-y-1">
                    <p>题目数：{exam.examcount}</p>
                    <p>听写：{exam.zyCompleted ? `${exam.zyScore.toFixed(0)}分` : '未完成'}</p>
                    <p>阅读：{exam.yzCompleted ? `${exam.yzScore.toFixed(0)}分` : '未完成'}</p>
                    <p>{formatDate(exam.createtime)}</p>
                  </div>
                </div>
                <div className="flex gap-2 mt-4">
                  <Button variant="primary" size="sm" className="flex-1" icon={<PlayCircle className="w-4 h-4" />} onClick={() => navigate(`/exam/${exam.Id}`)}>
                    {allCompleted ? '查看' : '继续'}
                  </Button>
                  <Button variant="danger" size="sm" icon={<Trash2 className="w-4 h-4" />} onClick={() => deleteMutation.mutate(exam.Id)} loading={deleteMutation.isPending} />
                </div>
              </Card>
            );
          })}
        </div>
      )}

      {/* 分页 */}
      <div className="flex justify-center gap-2">
        <Button variant="ghost" size="sm" onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1}>上一页</Button>
        <span className="flex items-center px-3 text-sm text-[var(--color-content-secondary)]">第 {page} 页</span>
        <Button variant="ghost" size="sm" onClick={() => setPage((p) => p + 1)} disabled={exams.length < pageSize}>下一页</Button>
      </div>

      {/* 创建测验弹窗 */}
      <Modal
        open={showCreate}
        onClose={() => setShowCreate(false)}
        title="创建新测验"
        footer={
          <>
            <Button variant="ghost" onClick={() => setShowCreate(false)}>取消</Button>
            <Button loading={createMutation.isPending} onClick={() => createMutation.mutate()}>创建</Button>
          </>
        }
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-[var(--color-content)] mb-1.5">题目数量</label>
            <Input type="number" value={String(examCount)} onChange={(e) => setExamCount(Number(e.target.value))} />
          </div>
          <div>
            <label className="block text-sm font-medium text-[var(--color-content)] mb-1.5">限时（分钟）</label>
            <Input type="number" value={String(limitTime)} onChange={(e) => setLimitTime(Number(e.target.value))} />
          </div>
        </div>
      </Modal>
    </div>
  );
}
