import api from '@/lib/api';

export async function getUserInfo() {
  const res = await api.get('/home/getuiserinfo');
  return res.data;
}

export async function updateLearnRecord(data: string) {
  const res = await api.post(`/home/updcno?data=${encodeURIComponent(data)}`);
  return res.data;
}
