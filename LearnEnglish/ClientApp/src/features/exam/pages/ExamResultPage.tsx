import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Trophy, RotateCcw, ArrowLeft, CheckCircle, XCircle } from 'lucide-react';
import { Button, Card, Badge, Progress } from '@/components/ui';
import { toast } from '@/components/ui/Toast';
import { getExam, getExamContent, restartExam } from '../api';
import { isApiSuccess } from '@/lib/utils';
import type { ExamContentItem } from '@/types';

export default function ExamResultPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const examId = Number(id);
  const [examType, setExamType] = useState(1);

  const { data: examInfo } = useQuery({
    queryKey: ['exam', examId],
    queryFn: () => getExam(examId),
  });

  const { data: contentData, isLoading } = useQuery({
    queryKey: ['examContent', examId, examType],
    queryFn: () => getExamContent(examId, examType),
  });

  const exam = examInfo?.data ?? examInfo;
  const questions: ExamContentItem[] = contentData?.data ?? [];
  const score = contentData?.score ?? 0;

  const restartMutation = useMutation({
    mutationFn: () => restartExam(examId, examType),
    onSuccess: (res) => {
      if (isApiSuccess(res)) {
        queryClient.invalidateQueries({ queryKey: ['examContent', examId] });
        toast.success('已重置');
        navigate(`/exam/${examId}`, { replace: true });
      }
    },
  });

  const total = questions.length;
  const correctCount = questions.filter((q) => q.IsOk === true).length;
  const wrongCount = questions.filter((q) => q.IsOk === false).length;
  const unanswered = total - correctCount - wrongCount;

  const gradeColor = score >= 80 ? 'text-[var(--color-success)]' : score >= 60 ? 'text-[var(--color-warning)]' : 'text-[var(--color-error)]';

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="w-8 h-8 border-4 border-[var(--color-primary)] border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="max-w-3xl mx-auto space-y-6 animate-fade-in">
      {/* 题型切换 */}
      <div className="flex gap-1">
        {[{t:1,l:'听写'},{t:2,l:'阅读'},{t:3,l:'翻译'}].map(({t,l}) => (
          <button key={t} onClick={() => setExamType(t)}
            className={`px-3 py-1.5 text-sm rounded-md ${examType === t ? 'bg-[var(--color-primary)] text-white' : 'bg-[var(--color-surface-alt)] text-[var(--color-content-secondary)]'}`}>
            {l}
          </button>
        ))}
      </div>

      {/* 成绩卡片 */}
      <Card className="text-center py-10">
        <div className="inline-flex items-center justify-center w-20 h-20 rounded-full bg-gradient-to-br from-[var(--color-primary)] to-[var(--color-secondary)] mb-4">
          <Trophy className="w-10 h-10 text-white" />
        </div>
        <h1 className={`text-5xl font-bold ${gradeColor} mb-2`}>{score}</h1>
        <p className="text-[var(--color-content-secondary)]">
          {score >= 80 ? '太棒了！继续保持 🎉' : score >= 60 ? '还不错，再接再厉' : '需要加油哦，多练习'}
        </p>

        <div className="flex justify-center gap-8 mt-6">
          <div className="text-center">
            <div className="text-2xl font-bold text-[var(--color-success)]">{correctCount}</div>
            <div className="text-sm text-[var(--color-content-secondary)]">正确</div>
          </div>
          <div className="text-center">
            <div className="text-2xl font-bold text-[var(--color-error)]">{wrongCount}</div>
            <div className="text-sm text-[var(--color-content-secondary)]">错误</div>
          </div>
          <div className="text-center">
            <div className="text-2xl font-bold text-[var(--color-content)]">{total}</div>
            <div className="text-sm text-[var(--color-content-secondary)]">总题数</div>
          </div>
        </div>

        {total > 0 && (
          <div className="max-w-xs mx-auto mt-6">
            <Progress value={(correctCount / total) * 100} />
          </div>
        )}
      </Card>

      {/* 操作按钮 */}
      <div className="flex justify-center gap-3">
        <Button variant="ghost" icon={<ArrowLeft className="w-4 h-4" />} onClick={() => navigate('/exam')}>
          返回列表
        </Button>
        <Button icon={<RotateCcw className="w-4 h-4" />} loading={restartMutation.isPending} onClick={() => restartMutation.mutate()}>
          重新测验
        </Button>
      </div>

      {/* 详细结果 */}
      <h2 className="text-lg font-semibold text-[var(--color-content)]">答题详情</h2>
      <div className="space-y-3">
        {questions.map((q, idx) => {
          const isCorrect = q.IsOk === true;
          const isWrong = q.IsOk === false;
          return (
            <Card key={q.Id} className={`border-l-4 ${isCorrect ? 'border-l-[var(--color-success)]' : isWrong ? 'border-l-[var(--color-error)]' : 'border-l-[var(--color-border)]'}`}>
              <div className="flex items-start gap-3">
                {isCorrect ? (
                  <CheckCircle className="w-5 h-5 text-[var(--color-success)] mt-0.5 shrink-0" />
                ) : isWrong ? (
                  <XCircle className="w-5 h-5 text-[var(--color-error)] mt-0.5 shrink-0" />
                ) : (
                  <div className="w-5 h-5 rounded-full border-2 border-[var(--color-border)] mt-0.5 shrink-0" />
                )}
                <div className="flex-1 min-w-0">
                  <div className="font-medium text-[var(--color-content)]">
                    {idx + 1}. {q.En}
                  </div>
                  <div className="text-sm text-[var(--color-content-tertiary)]">{q.Cn}</div>
                  {q.Answer && (
                    <div className="mt-1 text-sm text-[var(--color-content-secondary)]">
                      你的答案：<span className={isCorrect ? 'text-[var(--color-success)]' : 'text-[var(--color-error)]'}>{q.Answer}</span>
                    </div>
                  )}
                  {isWrong && (
                    <div className="mt-1 text-sm">
                      正确答案：<Badge color="green">{examType === 1 ? q.Cn : q.En}</Badge>
                    </div>
                  )}
                </div>
              </div>
            </Card>
          );
        })}
      </div>
    </div>
  );
}
