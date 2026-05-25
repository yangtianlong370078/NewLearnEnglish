import { type ReactNode, useEffect } from 'react';
import { X } from 'lucide-react';

interface ModalProps {
  open: boolean;
  onClose: () => void;
  title?: string;
  children: ReactNode;
  footer?: ReactNode;
}

export default function Modal({ open, onClose, title, children, footer }: ModalProps) {
  useEffect(() => {
    if (open) {
      document.body.style.overflow = 'hidden';
      const handleEsc = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose(); };
      document.addEventListener('keydown', handleEsc);
      return () => {
        document.body.style.overflow = '';
        document.removeEventListener('keydown', handleEsc);
      };
    } else {
      document.body.style.overflow = '';
    }
  }, [open, onClose]);

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-[var(--overlay-bg)] animate-fade-in" onClick={onClose} />
      <div className="relative bg-[var(--card-bg)] rounded-3xl shadow-modal w-full max-w-lg max-h-[90vh] overflow-y-auto animate-scale-in">
        {title && (
          <div className="sticky top-0 bg-[var(--card-bg)] px-8 pt-8 pb-4 border-b border-[var(--color-border)] z-10">
            <div className="flex items-center justify-between">
              <h2 className="text-xl font-bold text-[var(--color-content)]">{title}</h2>
              <button
                onClick={onClose}
                className="w-9 h-9 rounded-xl bg-[var(--color-surface-tertiary)] flex items-center justify-center hover:bg-[var(--color-border)] transition-colors"
              >
                <X className="w-4 h-4" />
              </button>
            </div>
          </div>
        )}
        <div className="px-8 py-6">{children}</div>
        {footer && (
          <div className="sticky bottom-0 bg-[var(--card-bg)] px-8 pb-8 pt-4 border-t border-[var(--color-border)] flex items-center justify-end gap-3">
            {footer}
          </div>
        )}
      </div>
    </div>
  );
}
