import { forwardRef, type InputHTMLAttributes, type ReactNode } from 'react';
import { cn } from '@/lib/utils';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  icon?: ReactNode;
  error?: string;
}

const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ icon, error, className, ...props }, ref) => {
    return (
      <div className="relative">
        <input
          ref={ref}
          className={cn(
            'w-full h-11 px-4 bg-[var(--input-bg)] border border-[var(--input-border)] rounded-xl',
            'text-[var(--color-content)] text-sm placeholder:text-[var(--color-content-tertiary)]',
            'transition-all duration-200',
            'focus:outline-none focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10',
            'hover:border-[var(--color-content-tertiary)]',
            error && 'border-rose-500 focus:border-rose-500 focus:ring-rose-500/10',
            icon && 'pr-10',
            className
          )}
          {...props}
        />
        {icon && (
          <div className="absolute right-3.5 top-1/2 -translate-y-1/2 text-[var(--color-content-tertiary)]">
            {icon}
          </div>
        )}
        {error && <p className="mt-1.5 text-xs text-rose-500">{error}</p>}
      </div>
    );
  }
);

Input.displayName = 'Input';
export default Input;
