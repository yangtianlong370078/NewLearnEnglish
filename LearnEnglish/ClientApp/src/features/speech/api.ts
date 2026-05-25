import api from '@/lib/api';
import type { ApiResponse } from '@/types';

export async function recognizeSpeech(audioFile: Blob, word: string, type: number = 1) {
  const formData = new FormData();
  formData.append('audioFile', audioFile, 'recording.wav');
  const res = await api.post<ApiResponse & { result?: boolean; scoring?: number }>(
    `/whisper/recognize?word=${encodeURIComponent(word)}&type=${type}`,
    formData,
    { headers: { 'Content-Type': 'multipart/form-data' } }
  );
  return res.data;
}
