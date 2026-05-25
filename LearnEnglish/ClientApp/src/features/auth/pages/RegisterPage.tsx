import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { BookOpen } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { toast } from '@/components/ui/Toast';
import { register as registerApi } from '../api';
import { isApiSuccess } from '@/lib/utils';

interface RegisterForm {
  loginID: string;
  password: string;
  confirmPassword: string;
  phone: string;
  name: string;
}

export default function RegisterPage() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);

  const { register, handleSubmit, formState: { errors }, watch } = useForm<RegisterForm>();
  const password = watch('password');

  const onSubmit = async (data: RegisterForm) => {
    setLoading(true);
    try {
      const res = await registerApi(data.loginID, data.password, data.phone, data.name);
      if (isApiSuccess(res)) {
        toast.success('注册成功', '请使用新账号登录');
        navigate('/login', { replace: true });
      } else {
        toast.error('注册失败', res.msg || '请检查输入信息');
      }
    } catch {
      toast.error('注册失败', '网络错误，请稍后重试');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 via-violet-50 to-pink-50 dark:from-gray-900 dark:via-gray-900 dark:to-gray-900 p-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <div className="w-16 h-16 bg-gradient-to-br from-blue-500 to-violet-600 rounded-2xl flex items-center justify-center mx-auto mb-4 shadow-glow-blue">
            <BookOpen className="w-8 h-8 text-white" />
          </div>
          <h1 className="text-3xl font-bold text-[var(--color-content)]">注册账号</h1>
        </div>

        <div className="bg-[var(--card-bg)] rounded-3xl shadow-modal border border-[var(--color-border)] p-8">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-[var(--color-content)] mb-1.5">姓名</label>
              <Input placeholder="请输入姓名" {...register('name', { required: '请输入姓名' })} error={errors.name?.message} />
            </div>
            <div>
              <label className="block text-sm font-medium text-[var(--color-content)] mb-1.5">账号</label>
              <Input placeholder="请输入账号" {...register('loginID', { required: '请输入账号' })} error={errors.loginID?.message} />
            </div>
            <div>
              <label className="block text-sm font-medium text-[var(--color-content)] mb-1.5">手机号</label>
              <Input placeholder="请输入手机号" {...register('phone', { required: '请输入手机号' })} error={errors.phone?.message} />
            </div>
            <div>
              <label className="block text-sm font-medium text-[var(--color-content)] mb-1.5">密码</label>
              <Input type="password" placeholder="请输入密码" {...register('password', { required: '请输入密码', minLength: { value: 6, message: '密码至少6位' } })} error={errors.password?.message} />
            </div>
            <div>
              <label className="block text-sm font-medium text-[var(--color-content)] mb-1.5">确认密码</label>
              <Input
                type="password"
                placeholder="请再次输入密码"
                {...register('confirmPassword', { required: '请确认密码', validate: (v) => v === password || '密码不一致' })}
                error={errors.confirmPassword?.message}
              />
            </div>
            <Button type="submit" variant="gradient" size="lg" className="w-full" loading={loading}>
              注册
            </Button>
          </form>
          <div className="mt-6 text-center">
            <span className="text-sm text-[var(--color-content-tertiary)]">已有账号？</span>
            <Link to="/login" className="text-sm font-medium text-blue-600 hover:text-blue-700 ml-1">去登录</Link>
          </div>
        </div>
      </div>
    </div>
  );
}
