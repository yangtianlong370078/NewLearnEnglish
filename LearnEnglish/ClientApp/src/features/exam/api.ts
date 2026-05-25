import api from '@/lib/api';
import type { ApiResponse, Exam, ExamContentItem } from '@/types';

export async function createExam(examcount: number, limittime: number) {
  const res = await api.post<ApiResponse & { examid?: number }>(`/examv2/saveexam?examcount=${examcount}&limittime=${limittime}`);
  return res.data;
}

export async function getExam(examid: number) {
  const res = await api.get(`/examv2/getexam?examid=${examid}`);
  return res.data;
}

// 使用新的 JSON API
export async function getExamList(index: number = 1, word?: string) {
  const query = new URLSearchParams({ index: String(index) });
  if (word) query.set('word', word);
  const res = await api.get<{ success: boolean; data: Exam[]; total: number; pageIndex: number; pageSize: number }>(`/examv2/examlistjson?${query.toString()}`);
  return res.data;
}

// 使用新的 JSON API
export async function getExamContent(id: number, type?: number) {
  const query = new URLSearchParams({ id: String(id) });
  if (type !== undefined) query.set('type', String(type));
  const res = await api.get<{ success: boolean; data: ExamContentItem[]; limittime: number; score: number }>(`/examv2/examcontentlistjson?${query.toString()}`);
  return res.data;
}

export async function submitExamAnswer(data: string, examid: number, type: number, score: number) {
  const res = await api.post<ApiResponse>(
    `/examv2/insertexamnswer?data=${encodeURIComponent(data)}&examid=${examid}&type=${type}&score=${score}`
  );
  return res.data;
}

export async function restartExam(examid: number, type: number) {
  const res = await api.post<ApiResponse>(`/examv2/restartexam?examid=${examid}&type=${type}`);
  return res.data;
}

export async function deleteExam(examid: number) {
  const res = await api.post<ApiResponse>(`/examv2/deleteexam?examid=${examid}`);
  return res.data;
}
