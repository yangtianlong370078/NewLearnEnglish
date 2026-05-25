import { useState, useRef } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Volume2, Eye, EyeOff, ChevronLeft, ChevronRight } from 'lucide-react';
import { Button, Card, Badge } from '@/components/ui';
import { getHearingContent } from '../api';
import type { HearingContent } from '@/types';

export default function HearingPracticePage() {
  const [params] = useSearchParams();
  const kc = Number(params.get('kc') || 0);
  const [hearingId, setHearingId] = useState(0);
  const [current, setCurrent] = useState(0);
  const [showChinese, setShowChinese] = useState(false);
  const [isPlaying, setIsPlaying] = useState(false);
  const audioRef = useRef<HTMLAudioElement | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ['hearing', kc, hearingId],
    queryFn: () => getHearingContent(kc, hearingId),
    enabled: kc > 0,
  });

  // 后端返回 { success, list: HearingItem[] }
  const items: HearingContent[] = data?.list ?? [];
  const item = items[current] ?? null;

  const playTTS = (text: string) => {
    const utterance = new SpeechSynthesisUtterance(text);
    utterance.lang = 'zh-CN';
    utterance.rate = 0.85;
    utterance.onend = () => setIsPlaying(false);
    speechSynthesis.cancel();
    speechSynthesis.speak(utterance);
    setIsPlaying(true);
  };

  if (!kc) {
    return (
      <div className="text-center py-20 text-[var(--color-content-secondary)]">
        请从课程页面进入听力练习
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto space-y-6 animate-fade-in">
      <audio ref={audioRef} onEnded={() => setIsPlaying(false)} />

      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-[var(--color-content)]">听力练习</h1>
        <Badge>课程 #{kc}</Badge>
      </div>

      {isLoading ? (
        <Card className="animate-pulse h-60" />
      ) : !items.length ? (
        <Card className="text-center py-16 text-[var(--color-content-secondary)]">暂无听力数据</Card>
      ) : (
        <>
          {/* 当前字幕卡片 */}
          <Card className="text-center py-12 px-8">
            <div className="mb-2 text-sm text-[var(--color-content-tertiary)]">
              第 {current + 1} / {items.length} 句
            </div>

            {/* 波形动画 */}
            <div className="flex items-center justify-center gap-1 mb-8 h-12">
              {isPlaying ? (
                Array.from({ length: 5 }).map((_, i) => (
                  <div key={i}
                    className="w-1.5 bg-gradient-to-t from-[var(--color-primary)] to-[var(--color-secondary)] rounded-full animate-waveform"
                    style={{ height: '100%', animationDelay: `${i * 0.15}s` }} />
                ))
              ) : (
                <div className="text-[var(--color-content-tertiary)] text-sm">点击播放按钮朗读</div>
              )}
            </div>

            {/* 文本 */}
            <h2 className="text-2xl font-bold text-[var(--color-content)] mb-4 leading-relaxed">
              {item?.Value}
            </h2>

            {/* 中文释义/翻译 */}
            <div className={`transition-all duration-300 overflow-hidden ${showChinese ? 'max-h-20 opacity-100' : 'max-h-0 opacity-0'}`}>
              <p className="text-lg text-[var(--color-content-secondary)] mt-2">{item?.ValueCN}</p>
            </div>

            {item && (
              <p className="mt-4 text-xs text-[var(--color-content-tertiary)]">
                {item.StartTime.toFixed(1)}s — {item.EndTime.toFixed(1)}s
              </p>
            )}
          </Card>

          {/* 控制按钮 */}
          <div className="flex justify-center gap-3">
            <Button variant="ghost" icon={<ChevronLeft className="w-5 h-5" />}
              onClick={() => setCurrent(c => Math.max(0, c - 1))} disabled={current === 0}>
              上一句
            </Button>
            <Button variant="secondary" size="lg"
              icon={isPlaying ? <Volume2 className="w-5 h-5 animate-pulse" /> : <Volume2 className="w-5 h-5" />}
              onClick={() => item && playTTS(item.Value)}>
              {isPlaying ? '播放中...' : '朗读'}
            </Button>
            <Button variant="ghost"
              icon={showChinese ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
              onClick={() => setShowChinese(!showChinese)}>
              {showChinese ? '隐藏翻译' : '显示翻译'}
            </Button>
            <Button variant="ghost" icon={<ChevronRight className="w-5 h-5" />}
              onClick={() => setCurrent(c => Math.min(items.length - 1, c + 1))} disabled={current >= items.length - 1}>
              下一句
            </Button>
          </div>

          {/* 句子导航 */}
          <Card>
            <div className="flex flex-wrap gap-2">
              {items.map((_, idx) => (
                <button key={idx} onClick={() => setCurrent(idx)}
                  className={`w-9 h-9 rounded-lg text-sm font-medium transition-colors ${
                    idx === current
                      ? 'bg-[var(--color-primary)] text-white'
                      : 'bg-[var(--color-surface-alt)] text-[var(--color-content-secondary)]'
                  }`}>
                  {idx + 1}
                </button>
              ))}
            </div>
          </Card>

          {/* 快捷键提示 */}
          <div className="text-center text-xs text-[var(--color-content-tertiary)]">
            提示：使用浏览器 TTS 朗读
          </div>
        </>
      )}
    </div>
  );
}
