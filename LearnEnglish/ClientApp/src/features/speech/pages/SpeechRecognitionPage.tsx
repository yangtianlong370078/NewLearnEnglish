import { useState, useRef, useCallback } from 'react';
import { Mic, Square, RotateCcw, Volume2 } from 'lucide-react';
import { useMutation } from '@tanstack/react-query';
import { Button, Card, Input, Badge, Progress } from '@/components/ui';
import { toast } from '@/components/ui/Toast';
import { recognizeSpeech } from '../api';
import { isApiSuccess } from '@/lib/utils';

export default function SpeechRecognitionPage() {
  const [word, setWord] = useState('');
  const [recording, setRecording] = useState(false);
  const [audioBlob, setAudioBlob] = useState<Blob | null>(null);
  const [result, setResult] = useState<{ matched: boolean; score: number } | null>(null);
  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const chunksRef = useRef<Blob[]>([]);

  const startRecording = useCallback(async () => {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      const mediaRecorder = new MediaRecorder(stream);
      mediaRecorderRef.current = mediaRecorder;
      chunksRef.current = [];

      mediaRecorder.ondataavailable = (e) => {
        if (e.data.size > 0) chunksRef.current.push(e.data);
      };

      mediaRecorder.onstop = () => {
        const blob = new Blob(chunksRef.current, { type: 'audio/wav' });
        setAudioBlob(blob);
        stream.getTracks().forEach((t) => t.stop());
      };

      mediaRecorder.start();
      setRecording(true);
      setResult(null);
    } catch {
      toast.error('无法访问麦克风', '请检查浏览器权限设置');
    }
  }, []);

  const stopRecording = useCallback(() => {
    if (mediaRecorderRef.current?.state === 'recording') {
      mediaRecorderRef.current.stop();
      setRecording(false);
    }
  }, []);

  const recognizeMutation = useMutation({
    mutationFn: () => {
      if (!audioBlob || !word.trim()) throw new Error('缺少数据');
      return recognizeSpeech(audioBlob, word.trim());
    },
    onSuccess: (res) => {
      if (isApiSuccess(res)) {
        setResult({ matched: res.result ?? false, score: res.scoring ?? 0 });
      } else {
        toast.error('识别失败', res.msg);
      }
    },
  });

  const playWord = () => {
    if (!word.trim()) return;
    const utterance = new SpeechSynthesisUtterance(word.trim());
    utterance.lang = 'en-US';
    utterance.rate = 0.85;
    speechSynthesis.speak(utterance);
  };

  const reset = () => {
    setAudioBlob(null);
    setResult(null);
  };

  const scoreColor = (score: number) =>
    score >= 80 ? 'text-[var(--color-success)]' : score >= 60 ? 'text-[var(--color-warning)]' : 'text-[var(--color-error)]';

  return (
    <div className="max-w-xl mx-auto space-y-6 animate-fade-in">
      <h1 className="text-2xl font-bold text-[var(--color-content)]">语音识别</h1>

      {/* 输入单词 */}
      <Card>
        <label className="block text-sm font-medium text-[var(--color-content)] mb-2">目标单词</label>
        <div className="flex gap-2">
          <Input
            placeholder="输入要练习的单词"
            value={word}
            onChange={(e) => setWord(e.target.value)}
            className="flex-1"
          />
          <Button variant="ghost" size="sm" icon={<Volume2 className="w-4 h-4" />} onClick={playWord} disabled={!word.trim()}>
            试听
          </Button>
        </div>
      </Card>

      {/* 录音区域 */}
      <Card className="text-center py-10">
        <div className="mb-6">
          {recording ? (
            <div className="flex items-center justify-center gap-1 h-16">
              {Array.from({ length: 7 }).map((_, i) => (
                <div
                  key={i}
                  className="w-1.5 bg-[var(--color-error)] rounded-full animate-waveform"
                  style={{ height: '100%', animationDelay: `${i * 0.1}s` }}
                />
              ))}
            </div>
          ) : audioBlob ? (
            <Badge color="green" className="text-base px-4 py-2">录音完成</Badge>
          ) : (
            <p className="text-[var(--color-content-secondary)]">点击下方按钮开始录音</p>
          )}
        </div>

        <div className="flex justify-center gap-3">
          {!recording ? (
            <>
              <Button
                variant="gradient"
                size="lg"
                icon={<Mic className="w-5 h-5" />}
                onClick={startRecording}
                disabled={!word.trim()}
              >
                开始录音
              </Button>
              {audioBlob && (
                <>
                  <Button variant="ghost" size="lg" icon={<RotateCcw className="w-5 h-5" />} onClick={reset}>
                    重录
                  </Button>
                  <Button
                    size="lg"
                    onClick={() => recognizeMutation.mutate()}
                    loading={recognizeMutation.isPending}
                  >
                    提交识别
                  </Button>
                </>
              )}
            </>
          ) : (
            <Button variant="danger" size="lg" icon={<Square className="w-4 h-4" />} onClick={stopRecording}>
              停止录音
            </Button>
          )}
        </div>
      </Card>

      {/* 结果展示 */}
      {result && (
        <Card className="animate-scale-in">
          <div className="text-center">
            <h3 className="text-lg font-semibold text-[var(--color-content)] mb-4">识别结果</h3>
            <div className={`text-5xl font-bold mb-2 ${scoreColor(result.score)}`}>
              {result.score}
            </div>
            <Progress value={result.score} className="max-w-xs mx-auto mb-3" />
            <Badge color={result.matched ? 'green' : 'red'}>
              {result.matched ? '发音正确 ✓' : '发音需改进'}
            </Badge>
          </div>
        </Card>
      )}
    </div>
  );
}
