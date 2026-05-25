import { type ClassValue, clsx } from 'clsx';

// 简易 clsx 实现（避免额外依赖）
export function cn(...inputs: ClassValue[]): string {
  return clsx(inputs);
}

// 兼容后端 success/succss 拼写
export function isApiSuccess(res: { success?: boolean; succss?: boolean }): boolean {
  return res.success === true || res.succss === true;
}

// 格式化日期
export function formatDate(date: string | Date): string {
  const d = new Date(date);
  return d.toLocaleDateString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit' });
}

// 格式化数字
export function formatNumber(num: number): string {
  return num.toLocaleString('zh-CN');
}
