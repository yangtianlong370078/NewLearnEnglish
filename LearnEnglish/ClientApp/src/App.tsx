import { lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import AppLayout from '@/components/layout/AppLayout';

// 懒加载页面
const LoginPage = lazy(() => import('@/features/auth/pages/LoginPage'));
const RegisterPage = lazy(() => import('@/features/auth/pages/RegisterPage'));
const DashboardPage = lazy(() => import('@/features/dashboard/pages/DashboardPage'));
const ProfilePage = lazy(() => import('@/features/profile/pages/ProfilePage'));
const CourseCatalogPage = lazy(() => import('@/features/course/pages/CourseCatalogPage'));
const MyCoursesPage = lazy(() => import('@/features/course/pages/MyCoursesPage'));
const CourseDetailPage = lazy(() => import('@/features/course/pages/CourseDetailPage'));
const WordListPage = lazy(() => import('@/features/words/pages/WordListPage'));
const WordTablePage = lazy(() => import('@/features/words/pages/WordTablePage'));
const WordDetailPage = lazy(() => import('@/features/words/pages/WordDetailPage'));
const ExamListPage = lazy(() => import('@/features/exam/pages/ExamListPage'));
const ExamTakePage = lazy(() => import('@/features/exam/pages/ExamTakePage'));
const ExamResultPage = lazy(() => import('@/features/exam/pages/ExamResultPage'));
const HearingPracticePage = lazy(() => import('@/features/hearing/pages/HearingPracticePage'));
const SpeechRecognitionPage = lazy(() => import('@/features/speech/pages/SpeechRecognitionPage'));
const StatisticsPage = lazy(() => import('@/features/statistics/pages/StatisticsPage'));

function PageLoader() {
  return (
    <div className="flex items-center justify-center h-64">
      <div className="w-8 h-8 border-4 border-[var(--color-primary)] border-t-transparent rounded-full animate-spin" />
    </div>
  );
}

export default function App() {
  return (
    <BrowserRouter basename="/spa">
      <Suspense fallback={<PageLoader />}>
        <Routes>
          {/* 公开路由 */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* 需认证路由 */}
          <Route element={<AppLayout />}>
            <Route index element={<DashboardPage />} />
            <Route path="profile" element={<ProfilePage />} />
            <Route path="courses" element={<CourseCatalogPage />} />
            <Route path="courses/mine" element={<MyCoursesPage />} />
            <Route path="courses/:id" element={<CourseDetailPage />} />
            <Route path="words" element={<WordListPage />} />
            <Route path="words/table/:courseId" element={<WordTablePage />} />
            <Route path="words/:word" element={<WordDetailPage />} />
            <Route path="exams" element={<ExamListPage />} />
            <Route path="exams/:id" element={<ExamTakePage />} />
            <Route path="exams/:id/result" element={<ExamResultPage />} />
            <Route path="hearing" element={<HearingPracticePage />} />
            <Route path="speech" element={<SpeechRecognitionPage />} />
            <Route path="statistics" element={<StatisticsPage />} />
          </Route>

          {/* 兜底 */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Suspense>
    </BrowserRouter>
  );
}
