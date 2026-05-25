import { cn } from '@/lib/utils';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import type { ReactNode } from 'react';

interface Column<T> {
  key: string;
  title: string;
  render?: (item: T, index: number) => ReactNode;
  className?: string;
}

interface TableProps<T> {
  columns: Column<T>[];
  data: T[];
  rowKey: (item: T) => string | number;
  page?: number;
  totalPages?: number;
  onPageChange?: (page: number) => void;
  className?: string;
  emptyText?: string;
}

export default function Table<T>({
  columns, data, rowKey, page, totalPages, onPageChange, className, emptyText = '暂无数据',
}: TableProps<T>) {
  return (
    <div className={cn('bg-[var(--card-bg)] rounded-2xl border border-[var(--color-border)] shadow-card overflow-hidden', className)}>
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead>
            <tr className="border-b border-[var(--color-border)]">
              {columns.map((col) => (
                <th key={col.key} className={cn(
                  'text-left text-xs font-semibold uppercase tracking-wider text-[var(--color-content-tertiary)] px-6 py-4',
                  col.className
                )}>
                  {col.title}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-[var(--color-border-secondary)]">
            {data.length === 0 ? (
              <tr>
                <td colSpan={columns.length} className="px-6 py-16 text-center text-[var(--color-content-tertiary)]">
                  {emptyText}
                </td>
              </tr>
            ) : (
              data.map((item, idx) => (
                <tr key={rowKey(item)} className="hover:bg-[var(--color-surface-secondary)] transition-colors">
                  {columns.map((col) => (
                    <td key={col.key} className={cn('px-6 py-4 text-sm', col.className)}>
                      {col.render ? col.render(item, idx) : (item as Record<string, unknown>)[col.key] as ReactNode}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
      {totalPages && totalPages > 1 && onPageChange && (
        <div className="flex items-center justify-between px-6 py-4 border-t border-[var(--color-border)]">
          <span className="text-sm text-[var(--color-content-tertiary)]">
            第 {page} / {totalPages} 页
          </span>
          <div className="flex gap-2">
            <button
              onClick={() => onPageChange((page ?? 1) - 1)}
              disabled={page === 1}
              className="w-8 h-8 flex items-center justify-center rounded-lg border border-[var(--color-border)] hover:bg-[var(--color-surface-secondary)] disabled:opacity-50 transition-colors"
            >
              <ChevronLeft className="w-4 h-4" />
            </button>
            <button
              onClick={() => onPageChange((page ?? 1) + 1)}
              disabled={page === totalPages}
              className="w-8 h-8 flex items-center justify-center rounded-lg border border-[var(--color-border)] hover:bg-[var(--color-surface-secondary)] disabled:opacity-50 transition-colors"
            >
              <ChevronRight className="w-4 h-4" />
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
