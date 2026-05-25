import api from '@/lib/api';

export async function getHearingContent(kc: number, id?: number) {
  const query = new URLSearchParams({ kc: String(kc) });
  if (id !== undefined) query.set('id', String(id));
  const res = await api.get(`/hearing/hearingconent?${query.toString()}`);
  return res.data;
}
