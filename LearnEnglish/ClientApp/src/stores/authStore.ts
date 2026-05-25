import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { UserInfo } from '@/types';

interface AuthState {
  token: string | null;
  refreshToken: string | null;
  user: UserInfo | null;
  isAuthenticated: boolean;
  setTokens: (token: string, refreshToken?: string) => void;
  setUser: (user: UserInfo) => void;
  login: (token: string, user: UserInfo, refreshToken?: string) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      refreshToken: null,
      user: null,
      isAuthenticated: false,
      setTokens: (token, refreshToken) =>
        set({ token, refreshToken: refreshToken ?? null, isAuthenticated: true }),
      setUser: (user) => set({ user }),
      login: (token, user, refreshToken) =>
        set({ token, user, refreshToken: refreshToken ?? null, isAuthenticated: true }),
      logout: () =>
        set({ token: null, refreshToken: null, user: null, isAuthenticated: false }),
    }),
    { name: 'auth-storage' }
  )
);
