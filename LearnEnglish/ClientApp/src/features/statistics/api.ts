import api from '@/lib/api';
import type { ApiResponse, StatisticsLearnGroup } from '@/types';

export async function queryLearnCount() {
  const res = await api.get<ApiResponse & { bzcount?: number; count?: number; username?: string }>('/statistics/querylearncount');
  return res.data;
}

// 后端返回 { success, categorys: StatisticsLearnGroup[] }
export async function getMonthlyStats(): Promise<StatisticsLearnGroup[]> {
  const res = await api.get<{ success: boolean; categorys: StatisticsLearnGroup[] }>('/statistics/statisticslearncounttwo');
  return res.data?.categorys ?? [];
}

export async function saveLearningTask(params: { id: number; count: number; date: string; type: number; weekend: number }) {
  const q = new URLSearchParams({
    id: String(params.id),
    count: String(params.count),
    date: params.date,
    type: String(params.type),
    weekend: String(params.weekend),
  });
  const res = await api.post<ApiResponse>(`/statistics/savelearntask?${q.toString()}`);
  return res.data;
}
