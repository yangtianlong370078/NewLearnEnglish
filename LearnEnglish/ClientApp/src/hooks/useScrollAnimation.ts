import { useEffect, useRef } from 'react';

export function useScrollAnimation(threshold = 0.1, rootMargin = '0px 0px -60px 0px') {
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;

    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          el.classList.add('animate-fade-in-up');
          el.style.opacity = '1';
          observer.unobserve(el);
        }
      },
      { threshold, rootMargin }
    );

    el.style.opacity = '0';
    observer.observe(el);
    return () => observer.disconnect();
  }, [threshold, rootMargin]);

  return ref;
}
