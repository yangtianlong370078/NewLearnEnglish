import api from '@/lib/api';
import type { ApiResponse, Course } from '@/types';

// 后端返回 { data: courseInfo[], success }
export async function getMyCourses(type: number = 1) {
  const res = await api.get<{ data: Course[]; success: boolean }>(`/course/mycategorys?id=${type}`);
  return res.data;
}

export async function addCourse(courseId: number) {
  const res = await api.post<ApiResponse>(`/course/insertmycourse?setcourseId=${courseId}`);
  return res.data;
}

export async function saveCourse(setcourseId: number, name: string, type: number) {
  const res = await api.post<ApiResponse>(`/course/savecourse?setcourseId=${setcourseId}&insercoursename=${encodeURIComponent(name)}&type=${type}`);
  return res.data;
}

export async function deleteCourse(courseId: number) {
  const res = await api.post<ApiResponse>(`/course/deletecourse?setcourseId=${courseId}`);
  return res.data;
}

export async function saveCourseContent(courseId: number, en: string, cn: string) {
  const res = await api.post<ApiResponse>(`/course/savecoursecontent?courseId=${courseId}&en=${encodeURIComponent(en)}&cn=${encodeURIComponent(cn)}`);
  return res.data;
}
