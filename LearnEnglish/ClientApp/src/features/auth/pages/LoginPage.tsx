import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { BookOpen, Eye, EyeOff } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { toast } from '@/components/ui/Toast';
import { useAuthStore } from '@/stores/authStore';
import { loginByPassword } from '../api';
import { isApiSuccess } from '@/lib/utils';

interface LoginForm {
  loginID: string;
  password: string;
}

export default function LoginPage() {
  const navigate = useNavigate();
  const login = useAuthStore((s) => s.login);
  const [showPwd, setShowPwd] = useState(false);
  const [loading, setLoading] = useState(false);

  const { register, handleSubmit, formState: { errors } } = useForm<LoginForm>();

  const onSubmit = async (data: LoginForm) => {
    setLoading(true);
    try {
      const res = await loginByPassword(data.loginID, data.password);
      if (isApiSuccess(res) && res.token) {
        login(res.token, res.user);
        toast.success('登录成功', `欢迎回来，${res.user.name}`);
        navigate('/', { replace: true });
      } else {
        toast.error('登录失败', res.msg || '账号或密码错误');
      }
    } catch {
      toast.error('登录失败', '网络错误，请稍后重试');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 via-violet-50 to-pink-50 dark:from-gray-900 dark:via-gray-900 dark:to-gray-900 p-4">
      <div className="w-full max-w-md">
        {/* Logo */}
        <div className="text-center mb-8">
          <div className="w-16 h-16 bg-gradient-to-br from-blue-500 to-violet-600 rounded-2xl flex items-center justify-center mx-auto mb-4 shadow-glow-blue">
            <BookOpen className="w-8 h-8 text-white" />
          </div>
          <h1 className="text-3xl font-bold text-[var(--color-content)]">学英文</h1>
          <p className="text-sm text-[var(--color-content-tertiary)] mt-2">每天进步一点点</p>
        </div>

        {/* 登录表单 */}
        <div className="bg-[var(--card-bg)] rounded-3xl shadow-modal border border-[var(--color-border)] p-8">
          <h2 className="text-xl font-bold text-[var(--color-content)] mb-6">登录</h2>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
            <div>
              <label className="block text-sm font-medium text-[var(--color-content)] mb-1.5">账号</label>
              <Input
                placeholder="请输入账号"
                {...register('loginID', { required: '请输入账号' })}
                error={errors.loginID?.message}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-[var(--color-content)] mb-1.5">密码</label>
              <div className="relative">
                <Input
                  type={showPwd ? 'text' : 'password'}
                  placeholder="请输入密码"
                  {...register('password', { required: '请输入密码' })}
                  error={errors.password?.message}
                  icon={
                    <button type="button" onClick={() => setShowPwd(!showPwd)} className="text-[var(--color-content-tertiary)]">
                      {showPwd ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                    </button>
                  }
                />
              </div>
            </div>
            <Button type="submit" variant="gradient" size="lg" className="w-full" loading={loading}>
              登录
            </Button>
          </form>

          <div className="mt-6 text-center">
            <span className="text-sm text-[var(--color-content-tertiary)]">还没有账号？</span>
            <Link to="/register" className="text-sm font-medium text-blue-600 hover:text-blue-700 ml-1">
              立即注册
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
