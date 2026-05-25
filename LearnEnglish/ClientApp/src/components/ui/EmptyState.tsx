import { type ReactNode } from 'react';
import { Inbox } from 'lucide-react';
import Button from './Button';

interface EmptyStateProps {
  icon?: ReactNode;
  title: string;
  description?: string;
  actionLabel?: string;
  onAction?: () => void;
}

export default function EmptyState({ icon, title, description, actionLabel, onAction }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-20 px-6">
      <div className="w-20 h-20 rounded-full bg-[var(--color-surface-tertiary)] flex items-center justify-center mb-6">
        {icon || <Inbox className="w-10 h-10 text-[var(--color-content-tertiary)]" />}
      </div>
      <h3 className="text-lg font-semibold text-[var(--color-content)] mb-2">{title}</h3>
      {description && (
        <p className="text-sm text-[var(--color-content-tertiary)] text-center max-w-sm mb-6">{description}</p>
      )}
      {actionLabel && onAction && (
        <Button variant="gradient" size="lg" onClick={onAction}>{actionLabel}</Button>
      )}
    </div>
  );
}
