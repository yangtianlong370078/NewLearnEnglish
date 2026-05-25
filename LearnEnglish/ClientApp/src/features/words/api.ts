import api from '@/lib/api';
import type { ApiResponse, Word } from '@/types';

// 使用新的 JSON API
export async function getWordList(params: {
  kc: number;
  zt?: number;
  tp?: number;
  name?: string;
  index?: number;
  pageSize?: number;
}) {
  const query = new URLSearchParams();
  query.set('kc', String(params.kc));
  if (params.zt !== undefined) query.set('zt', String(params.zt));
  if (params.tp !== undefined) query.set('tp', String(params.tp));
  if (params.name) query.set('name', params.name);
  query.set('index', String(params.index ?? 1));
  query.set('pageSize', String(params.pageSize ?? 30));
  const res = await api.get<{ success: boolean; data: Word[]; total: number; pageIndex: number; pageSize: number; brs: number; wlj: number; yzw: number }>(`/word/wordlistjson?${query.toString()}`);
  return res.data;
}

// 使用新的 JSON API
export async function getCollectWordList(params: {
  kc: number;
  zt?: number;
  tp?: number;
  name?: string;
  index?: number;
}) {
  const query = new URLSearchParams();
  query.set('kc', String(params.kc));
  if (params.zt !== undefined) query.set('zt', String(params.zt));
  if (params.tp !== undefined) query.set('tp', String(params.tp));
  if (params.name) query.set('name', params.name);
  query.set('index', String(params.index ?? 1));
  const res = await api.get<{ success: boolean; data: Word[]; total: number; pageIndex: number; pageSize: number }>(`/word/collectwordlistjson?${query.toString()}`);
  return res.data;
}

export async function checkWordExist(kc: number, en: string) {
  const res = await api.get<ApiResponse>(`/word/wordexist?kc=${kc}&en=${encodeURIComponent(en)}`);
  return res.data;
}

export async function setWordStatus(zt: number, dqzt: number, lexiconId: number) {
  const res = await api.post<ApiResponse>(`/word/szzt?zt=${zt}&dqzt=${dqzt}&lexiconId=${lexiconId}`);
  return res.data;
}

export async function updateWordPracticeCount(data: string) {
  const res = await api.post<ApiResponse>(`/word/updcnov2?data=${encodeURIComponent(data)}`);
  return res.data;
}

export async function editWord(id: number, en: string, cn: string) {
  const res = await api.post<ApiResponse>(`/word/updc?id=${id}&en=${encodeURIComponent(en)}&cn=${encodeURIComponent(cn)}`);
  return res.data;
}

export async function deleteWord(coursecontentId: number) {
  const res = await api.post<ApiResponse>(`/word/deletedc?coursecontentId=${coursecontentId}`);
  return res.data;
}

export async function setCollect(lexiconId: number, isCollect: boolean) {
  const res = await api.post<ApiResponse>(`/word/setcollect?lexiconId=${lexiconId}&isCollect=${isCollect}`);
  return res.data;
}

export async function calibrateWords() {
  const res = await api.post<ApiResponse>('/word/calibration');
  return res.data;
}
