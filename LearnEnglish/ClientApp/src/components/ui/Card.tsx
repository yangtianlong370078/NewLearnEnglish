import { type ReactNode } from 'react';
import { cn } from '@/lib/utils';

interface CardProps {
  children: ReactNode;
  className?: string;
  hoverable?: boolean;
  onClick?: () => void;
}

export default function Card({ children, className, hoverable = false, onClick }: CardProps) {
  return (
    <div
      className={cn(
        'bg-[var(--card-bg)] rounded-2xl border border-[var(--color-border)] shadow-card p-6',
        'transition-all duration-300',
        hoverable && 'hover:shadow-card-hover hover:-translate-y-1 cursor-pointer',
        className
      )}
      onClick={onClick}
    >
      {children}
    </div>
  );
}

interface StatCardProps {
  icon: ReactNode;
  iconBg?: string;
  label: string;
  value: string | number;
  trend?: string;
  trendUp?: boolean;
}

export function StatCard({ icon, iconBg = 'bg-blue-50 dark:bg-blue-900/30', label, value, trend, trendUp }: StatCardProps) {
  return (
    <Card hoverable className="group">
      <div className="flex items-center justify-between mb-4">
        <div className={cn('w-12 h-12 rounded-xl flex items-center justify-center group-hover:scale-110 transition-transform duration-300', iconBg)}>
          {icon}
        </div>
        {trend && (
          <span className={cn(
            'text-xs font-medium px-2 py-0.5 rounded-full',
            trendUp ? 'text-emerald-500 bg-emerald-50 dark:bg-emerald-900/30' : 'text-rose-500 bg-rose-50 dark:bg-rose-900/30'
          )}>
            {trend}
          </span>
        )}
      </div>
      <p className="text-3xl font-bold text-[var(--color-content)] mb-1">{value}</p>
      <p className="text-sm text-[var(--color-content-secondary)]">{label}</p>
    </Card>
  );
}
