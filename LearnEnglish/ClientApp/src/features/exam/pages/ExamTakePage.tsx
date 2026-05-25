import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { Clock, ChevronLeft, ChevronRight, CheckCircle2 } from 'lucide-react';
import { Button, Card, Progress, Badge, Input } from '@/components/ui';
import { toast } from '@/components/ui/Toast';
import { getExamContent, submitExamAnswer, getExam } from '../api';
import { isApiSuccess } from '@/lib/utils';
import type { ExamContentItem } from '@/types';

export default function ExamTakePage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const examId = Number(id);
  const [examType, setExamType] = useState(1); // 1=听写, 2=阅读, 3=翻译
  const [current, setCurrent] = useState(0);
  const [answers, setAnswers] = useState<Record<number, string>>({});
  const [timeLeft, setTimeLeft] = useState(0);
  const [finished, setFinished] = useState(false);
  const [showAnswer, setShowAnswer] = useState(false);

  const { data: examInfo } = useQuery({
    queryKey: ['exam', examId],
    queryFn: () => getExam(examId),
  });

  const { data: contentData, isLoading } = useQuery({
    queryKey: ['examContent', examId, examType],
    queryFn: () => getExamContent(examId, examType),
  });

  const questions: ExamContentItem[] = contentData?.data ?? [];
  const limitTime = contentData?.limittime ?? examInfo?.data?.limittime ?? 30;
  const examScore = contentData?.score ?? 0;

  // 如果有 score，说明已经考完
  useEffect(() => {
    if (examScore > 0) setFinished(true);
  }, [examScore]);

  // 初始化倒计时
  useEffect(() => {
    if (limitTime > 0 && !finished) setTimeLeft(limitTime * 60);
  }, [limitTime, finished]);

  // 倒计时
  useEffect(() => {
    if (timeLeft <= 0 || finished) return;
    const timer = setInterval(() => {
      setTimeLeft((t) => {
        if (t <= 1) { clearInterval(timer); handleSubmit(); return 0; }
        return t - 1;
      });
    }, 1000);
    return () => clearInterval(timer);
  }, [timeLeft > 0, finished]);

  const formatTime = (seconds: number) => {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
  };

  const submitMutation = useMutation({
    mutationFn: (params: { data: string; score: number }) =>
      submitExamAnswer(params.data, examId, examType, params.score),
    onSuccess: (res) => {
      if (isApiSuccess(res)) {
        setFinished(true);
        toast.success('提交成功');
        navigate(`/exam/${examId}/result`, { replace: true });
      } else {
        toast.error('提交失败', res.msg);
      }
    },
  });

  const handleSubmit = useCallback(() => {
    let correct = 0;
    questions.forEach((q) => {
      const userAns = (answers[q.Id] ?? '').trim().toLowerCase();
      const expected = examType === 1 ? q.Cn : q.En;
      if (userAns && userAns === expected.trim().toLowerCase()) correct++;
    });
    const score = questions.length > 0 ? Math.round((correct / questions.length) * 100) : 0;
    const data = JSON.stringify(questions.map((q) => ({ id: q.Id, answer: answers[q.Id] ?? '' })));
    submitMutation.mutate({ data, score });
  }, [answers, questions, examType]);

  const q = questions[current];
  const answeredCount = Object.keys(answers).filter(k => answers[Number(k)]?.trim()).length;
  const progress = questions.length > 0 ? (answeredCount / questions.length) * 100 : 0;

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="w-8 h-8 border-4 border-[var(--color-primary)] border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  if (!questions.length) {
    return <div className="text-center py-20 text-[var(--color-content-secondary)]">暂无题目数据</div>;
  }

  return (
    <div className="max-w-3xl mx-auto space-y-6 animate-fade-in">
      {/* 顶部状态栏 */}
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-[var(--color-content)]">测验 #{examId}</h1>
        <div className="flex items-center gap-4">
          <div className="flex gap-1">
            {[{t:1,l:'听写'},{t:2,l:'阅读'},{t:3,l:'翻译'}].map(({t,l}) => (
              <button key={t} onClick={() => { setExamType(t); setCurrent(0); setAnswers({}); setFinished(false); setShowAnswer(false); }}
                className={`px-2 py-1 text-xs rounded-md ${examType === t ? 'bg-[var(--color-primary)] text-white' : 'bg-[var(--color-surface-alt)] text-[var(--color-content-secondary)]'}`}>
                {l}
              </button>
            ))}
          </div>
          {!finished && (
            <Badge color={timeLeft < 60 ? 'red' : timeLeft < 300 ? 'yellow' : 'green'}>
              <Clock className="w-3.5 h-3.5 mr-1 inline" />{formatTime(timeLeft)}
            </Badge>
          )}
          <span className="text-sm text-[var(--color-content-secondary)]">{answeredCount}/{questions.length}</span>
        </div>
      </div>

      <Progress value={progress} />

      {/* 题干 */}
      <Card className="p-8">
        <div className="mb-2 text-sm text-[var(--color-content-tertiary)]">第 {current + 1} / {questions.length} 题</div>
        <h2 className="text-2xl font-semibold text-[var(--color-content)] mb-6">
          {examType === 1 ? q.En : examType === 2 ? q.Cn : q.En}
        </h2>
        {q.Value && <p className="text-sm text-[var(--color-content-tertiary)] mb-4">提示：{q.Value}</p>}

        {finished ? (
          <div className="space-y-3">
            <span className={q.IsOk ? 'text-[var(--color-success)]' : 'text-[var(--color-error)]'}>
              {q.IsOk ? '✓ 正确' : '✗ 错误'}
            </span>
            {q.Answer && <p className="text-sm">你的答案：{q.Answer}</p>}
            <p className="text-sm">正确答案：{examType === 1 ? q.Cn : q.En}</p>
          </div>
        ) : (
          <div>
            <Input
              placeholder={examType === 1 ? '请输入中文释义...' : '请输入英文单词...'}
              value={answers[q.Id] ?? ''}
              onChange={(e) => setAnswers(prev => ({...prev, [q.Id]: e.target.value}))}
              onKeyDown={(e) => { if (e.key === 'Enter' && current < questions.length - 1) setCurrent(c => c + 1); }}
            />
            <button onClick={() => setShowAnswer(!showAnswer)}
              className="mt-2 text-xs text-[var(--color-content-tertiary)] hover:text-[var(--color-content-secondary)]">
              {showAnswer ? '隐藏答案' : '查看答案'}
            </button>
            {showAnswer && <p className="mt-1 text-sm text-[var(--color-content-secondary)]">{examType === 1 ? q.Cn : q.En}</p>}
          </div>
        )}
      </Card>

      {/* 底部导航 */}
      <div className="flex items-center justify-between">
        <Button variant="ghost" icon={<ChevronLeft className="w-4 h-4" />} onClick={() => setCurrent(c => Math.max(0, c - 1))} disabled={current === 0}>上一题</Button>
        {!finished && current === questions.length - 1 ? (
          <Button icon={<CheckCircle2 className="w-4 h-4" />} onClick={handleSubmit} loading={submitMutation.isPending}>提交测验</Button>
        ) : (
          <Button variant="secondary" onClick={() => setCurrent(c => Math.min(questions.length - 1, c + 1))}>
            下一题<ChevronRight className="w-4 h-4 ml-1" />
          </Button>
        )}
      </div>

      {/* 题号导航 */}
      <Card>
        <div className="flex flex-wrap gap-2">
          {questions.map((item, idx) => (
            <button key={item.Id} onClick={() => setCurrent(idx)}
              className={`w-9 h-9 rounded-lg text-sm font-medium transition-colors ${
                idx === current ? 'bg-[var(--color-primary)] text-white'
                  : finished ? (item.IsOk ? 'bg-[var(--color-success)]/20 text-[var(--color-success)]' : item.IsOk === false ? 'bg-[var(--color-error)]/20 text-[var(--color-error)]' : 'bg-[var(--color-surface-alt)] text-[var(--color-content-secondary)]')
                  : answers[item.Id]?.trim() ? 'bg-[var(--color-success)]/20 text-[var(--color-success)]' : 'bg-[var(--color-surface-alt)] text-[var(--color-content-secondary)]'
              }`}>
              {idx + 1}
            </button>
          ))}
        </div>
      </Card>
    </div>
  );
}
