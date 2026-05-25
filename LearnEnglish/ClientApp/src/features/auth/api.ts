import api from '@/lib/api';
import type { LoginResponse, ApiResponse } from '@/types';

function toForm(data: Record<string, string | number | undefined>) {
  const fd = new URLSearchParams();
  for (const [k, v] of Object.entries(data)) {
    if (v !== undefined) fd.append(k, String(v));
  }
  return fd;
}

export async function loginByPassword(loginID: string, password: string, sb: number | string = 1) {
  const res = await api.post<LoginResponse>('/api/Login/LoginResult', toForm({ loginID, password, sb }));
  return res.data;
}

export async function register(loginID: string, password: string, phone: string, name: string) {
  const res = await api.post<ApiResponse>('/api/Login/SetRegister', toForm({ loginID, password, phone, name }));
  return res.data;
}

export async function modifyPassword(oldPwd: string, newPwd: string) {
  const res = await api.post<ApiResponse>('/api/Login/ModifyPassword', toForm({ OldPwd: oldPwd, NewPwd: newPwd }));
  return res.data;
}

export async function loginByApi(loginID: string, password: string) {
  const res = await api.post('/api/auth/login', { loginID, password });
  return res.data;
}

export async function refreshToken(refreshToken: string) {
  const res = await api.post('/api/auth/refresh', { refreshToken });
  return res.data;
}
