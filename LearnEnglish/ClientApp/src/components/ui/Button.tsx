import { forwardRef, type ButtonHTMLAttributes, type ReactNode } from 'react';
import { cn } from '@/lib/utils';
import { Loader2 } from 'lucide-react';

type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger' | 'gradient' | 'icon';
type ButtonSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  size?: ButtonSize;
  loading?: boolean;
  icon?: ReactNode;
}

const variantStyles: Record<ButtonVariant, string> = {
  primary:
    'bg-blue-600 text-white font-medium hover:bg-blue-700 hover:shadow-btn hover:-translate-y-0.5 active:translate-y-0 active:shadow-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2',
  secondary:
    'bg-blue-50 text-blue-600 font-medium hover:bg-blue-100 dark:bg-blue-900/30 dark:text-blue-400 dark:hover:bg-blue-900/50',
  ghost:
    'bg-transparent text-blue-600 font-medium hover:bg-blue-50 dark:text-blue-400 dark:hover:bg-blue-900/20',
  danger:
    'bg-rose-600 text-white font-medium hover:bg-rose-700 hover:shadow-lg active:bg-rose-800',
  gradient:
    'bg-gradient-to-r from-blue-600 to-violet-600 text-white font-semibold hover:shadow-2xl hover:shadow-blue-500/25 hover:-translate-y-1 active:translate-y-0',
  icon:
    'bg-transparent text-content-secondary hover:bg-surface-tertiary hover:text-content rounded-xl',
};

const sizeStyles: Record<ButtonSize, string> = {
  xs: 'h-7 px-2.5 text-xs rounded-md',
  sm: 'h-8 px-3 text-sm rounded-lg',
  md: 'h-10 px-5 text-sm rounded-xl',
  lg: 'h-12 px-7 text-base rounded-xl',
  xl: 'h-14 px-10 text-lg rounded-2xl',
};

const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ variant = 'primary', size = 'md', loading, icon, children, className, disabled, ...props }, ref) => {
    return (
      <button
        ref={ref}
        className={cn(
          'inline-flex items-center justify-center gap-2 transition-all duration-200 ease-out',
          'focus-visible:outline-none disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:translate-y-0 disabled:hover:shadow-none',
          variantStyles[variant],
          variant !== 'icon' && sizeStyles[size],
          variant === 'icon' && 'w-10 h-10 p-0',
          className
        )}
        disabled={disabled || loading}
        {...props}
      >
        {loading && <Loader2 className="w-4 h-4 animate-spin" />}
        {!loading && icon}
        {children}
      </button>
    );
  }
);

Button.displayName = 'Button';
export default Button;
