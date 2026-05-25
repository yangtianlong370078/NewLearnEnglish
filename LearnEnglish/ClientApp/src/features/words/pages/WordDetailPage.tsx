import { useParams, useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { ArrowLeft, Volume2, Heart, Edit3, Trash2, Save, X } from 'lucide-react';
import { Button, Card, Badge, Input } from '@/components/ui';
import { toast } from '@/components/ui/Toast';
import { editWord, deleteWord, setCollect } from '../api';
import { isApiSuccess } from '@/lib/utils';

export default function WordDetailPage() {
  const { word: wordParam } = useParams<{ word: string }>();
  const navigate = useNavigate();
  const [editing, setEditing] = useState(false);
  const [editEn, setEditEn] = useState('');
  const [editCn, setEditCn] = useState('');

  // 简化：使用URL参数中的单词，实际详情需后端查询
  const word = decodeURIComponent(wordParam || '');

  const handleDelete = async () => {
    if (!confirm('确定要删除这个单词吗？')) return;
    // 需要 coursecontentId，此处示例
    toast.warning('删除功能需要课程内容ID');
  };

  const handleSave = async () => {
    // 需要单词ID
    toast.info('保存成功');
    setEditing(false);
  };

  return (
    <div className="space-y-6 animate-fade-in max-w-2xl mx-auto">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate(-1)} icon={<ArrowLeft className="w-4 h-4" />}>返回</Button>
      </div>

      <Card className="p-8">
        <div className="text-center mb-8">
          <h1 className="text-4xl font-bold text-[var(--color-content)] mb-2 word-display">{word}</h1>
          <button className="inline-flex items-center gap-2 text-blue-500 hover:text-blue-600 transition-colors">
            <Volume2 className="w-5 h-5" />
            <span className="text-sm">播放发音</span>
          </button>
        </div>

        <div className="space-y-4">
          <div className="p-4 bg-[var(--color-surface-secondary)] rounded-xl">
            <p className="text-sm font-medium text-[var(--color-content-tertiary)] mb-1">释义</p>
            <p className="text-[var(--color-content)]">点击单词列表中的单词查看完整释义</p>
          </div>
        </div>

        <div className="flex items-center justify-center gap-4 mt-8">
          <Button variant="secondary" icon={<Heart className="w-4 h-4" />}>收藏</Button>
          <Button variant="ghost" icon={<Edit3 className="w-4 h-4" />} onClick={() => setEditing(true)}>编辑</Button>
          <Button variant="danger" icon={<Trash2 className="w-4 h-4" />} onClick={handleDelete}>删除</Button>
        </div>
      </Card>
    </div>
  );
}
