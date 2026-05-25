import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Plus } from 'lucide-react';
import { Button, Card, Input, Modal } from '@/components/ui';
import { toast } from '@/components/ui/Toast';
import { saveCourseContent } from '../api';
import { isApiSuccess } from '@/lib/utils';

export default function CourseDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const courseId = Number(id);

  const [showAddModal, setShowAddModal] = useState(false);
  const [newEn, setNewEn] = useState('');
  const [newCn, setNewCn] = useState('');

  const addWordMutation = useMutation({
    mutationFn: () => saveCourseContent(courseId, newEn, newCn),
    onSuccess: (res) => {
      if (isApiSuccess(res)) {
        toast.success('单词已添加');
        setNewEn('');
        setNewCn('');
        setShowAddModal(false);
        queryClient.invalidateQueries({ queryKey: ['wordList'] });
      } else {
        toast.error('添加失败', res.msg);
      }
    },
  });

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" onClick={() => navigate(-1)} icon={<ArrowLeft className="w-4 h-4" />}>返回</Button>
          <h1 className="text-2xl font-bold text-[var(--color-content)]">课程详情</h1>
        </div>
        <div className="flex gap-2">
          <Button variant="secondary" size="sm" icon={<Plus className="w-4 h-4" />} onClick={() => setShowAddModal(true)}>
            添加单词
          </Button>
          <Button variant="primary" size="sm" onClick={() => navigate(`/words/table/${courseId}`)}>
            查看单词
          </Button>
        </div>
      </div>

      <Card>
        <p className="text-[var(--color-content-secondary)]">
          选择"查看单词"来浏览课程中的所有单词，或者"添加单词"手动添加新单词。
        </p>
      </Card>

      {/* 添加单词弹窗 */}
      <Modal
        open={showAddModal}
        onClose={() => setShowAddModal(false)}
        title="添加单词"
        footer={
          <>
            <Button variant="ghost" onClick={() => setShowAddModal(false)}>取消</Button>
            <Button loading={addWordMutation.isPending} onClick={() => addWordMutation.mutate()}>添加</Button>
          </>
        }
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-[var(--color-content)] mb-1.5">英文</label>
            <Input placeholder="输入英文单词" value={newEn} onChange={(e) => setNewEn(e.target.value)} />
          </div>
          <div>
            <label className="block text-sm font-medium text-[var(--color-content)] mb-1.5">中文释义</label>
            <Input placeholder="输入中文释义" value={newCn} onChange={(e) => setNewCn(e.target.value)} />
          </div>
        </div>
      </Modal>
    </div>
  );
}
