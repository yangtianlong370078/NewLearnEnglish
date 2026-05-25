import { useState } from 'react';
import { useQuery, useMutation } from '@tanstack/react-query';
import { User, Lock, Save } from 'lucide-react';
import { Button, Card, Input } from '@/components/ui';
import { toast } from '@/components/ui/Toast';
import { getUserInfo } from '@/features/dashboard/api';
import { modifyPassword } from '@/features/auth/api';
import { isApiSuccess } from '@/lib/utils';
import { useAuthStore } from '@/stores/authStore';

export default function ProfilePage() {
  const { user } = useAuthStore();
  const [oldPwd, setOldPwd] = useState('');
  const [newPwd, setNewPwd] = useState('');
  const [confirmPwd, setConfirmPwd] = useState('');

  const { data: userInfo } = useQuery({
    queryKey: ['userInfo'],
    queryFn: getUserInfo,
  });

  const info = userInfo?.data ?? userInfo ?? user;

  const pwdMutation = useMutation({
    mutationFn: () => {
      if (newPwd !== confirmPwd) throw new Error('两次密码不一致');
      if (newPwd.length < 6) throw new Error('密码至少6位');
      return modifyPassword(oldPwd, newPwd);
    },
    onSuccess: (res) => {
      if (isApiSuccess(res)) {
        toast.success('密码修改成功');
        setOldPwd('');
        setNewPwd('');
        setConfirmPwd('');
      } else {
        toast.error('修改失败', res.msg);
      }
    },
    onError: (err: Error) => {
      toast.error(err.message);
    },
  });

  return (
    <div className="max-w-2xl mx-auto space-y-6 animate-fade-in">
      <h1 className="text-2xl font-bold text-[var(--color-content)]">个人中心</h1>

      {/* 用户信息 */}
      <Card>
        <div className="flex items-center gap-4 mb-6">
          <div className="w-16 h-16 rounded-full bg-gradient-to-br from-[var(--color-primary)] to-[var(--color-secondary)] flex items-center justify-center">
            <User className="w-8 h-8 text-white" />
          </div>
          <div>
            <h2 className="text-xl font-semibold text-[var(--color-content)]">{info?.name ?? '用户'}</h2>
            <p className="text-sm text-[var(--color-content-secondary)]">{info?.loginid ?? ''}</p>
          </div>
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label className="block text-sm text-[var(--color-content-secondary)] mb-1">手机号</label>
            <div className="text-[var(--color-content)]">{info?.phone || '未设置'}</div>
          </div>
          <div>
            <label className="block text-sm text-[var(--color-content-secondary)] mb-1">年龄</label>
            <div className="text-[var(--color-content)]">{info?.age || '-'}</div>
          </div>
          <div>
            <label className="block text-sm text-[var(--color-content-secondary)] mb-1">有效期</label>
            <div className="text-[var(--color-content)]">
              {info?.startdate ? `${info.startdate} ~ ${info.enddate}` : '-'}
            </div>
          </div>
          <div>
            <label className="block text-sm text-[var(--color-content-secondary)] mb-1">状态</label>
            <div className="text-[var(--color-content)]">
              {info?.status === 1 ? '正常' : info?.status === 0 ? '禁用' : '-'}
            </div>
          </div>
        </div>
      </Card>

      {/* 修改密码 */}
      <Card>
        <h2 className="text-lg font-semibold text-[var(--color-content)] mb-4 flex items-center gap-2">
          <Lock className="w-5 h-5" /> 修改密码
        </h2>
        <div className="space-y-4 max-w-sm">
          <div>
            <label className="block text-sm font-medium text-[var(--color-content)] mb-1.5">当前密码</label>
            <Input type="password" value={oldPwd} onChange={(e) => setOldPwd(e.target.value)} placeholder="输入当前密码" />
          </div>
          <div>
            <label className="block text-sm font-medium text-[var(--color-content)] mb-1.5">新密码</label>
            <Input type="password" value={newPwd} onChange={(e) => setNewPwd(e.target.value)} placeholder="输入新密码（至少6位）" />
          </div>
          <div>
            <label className="block text-sm font-medium text-[var(--color-content)] mb-1.5">确认新密码</label>
            <Input
              type="password"
              value={confirmPwd}
              onChange={(e) => setConfirmPwd(e.target.value)}
              placeholder="再次输入新密码"
              error={confirmPwd && newPwd !== confirmPwd ? '两次密码不一致' : undefined}
            />
          </div>
          <Button
            icon={<Save className="w-4 h-4" />}
            loading={pwdMutation.isPending}
            onClick={() => pwdMutation.mutate()}
            disabled={!oldPwd || !newPwd || !confirmPwd}
          >
            保存
          </Button>
        </div>
      </Card>
    </div>
  );
}
