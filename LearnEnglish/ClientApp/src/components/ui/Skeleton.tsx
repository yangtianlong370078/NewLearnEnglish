import { cn } from '@/lib/utils';

interface SkeletonProps {
  className?: string;
}

export default function Skeleton({ className }: SkeletonProps) {
  return (
    <div className={cn(
      'rounded-xl animate-shimmer bg-[length:200%_100%] bg-gradient-to-r from-[var(--color-surface-tertiary)] via-[var(--color-surface-secondary)] to-[var(--color-surface-tertiary)]',
      className
    )} />
  );
}

export function WordCardSkeleton() {
  return (
    <div className="bg-[var(--card-bg)] rounded-2xl border border-[var(--color-border)] p-5 space-y-3">
      <Skeleton className="h-5 w-24" />
      <Skeleton className="h-4 w-16" />
      <Skeleton className="h-4 w-full" />
      <div className="flex justify-between pt-3 border-t border-[var(--color-border-secondary)]">
        <Skeleton className="h-5 w-5 rounded-lg" />
        <Skeleton className="h-5 w-5 rounded-lg" />
      </div>
    </div>
  );
}

export function TableSkeleton({ rows = 5 }: { rows?: number }) {
  return (
    <div className="space-y-3">
      {Array.from({ length: rows }).map((_, i) => (
        <Skeleton key={i} className="h-14 w-full" />
      ))}
    </div>
  );
}
