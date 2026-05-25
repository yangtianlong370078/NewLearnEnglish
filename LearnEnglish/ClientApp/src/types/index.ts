// 通用类型定义

export interface ApiResponse<T = unknown> {
  success?: boolean;
  succss?: boolean; // 兼容后端拼写错误
  msg?: string;
  data?: T;
}

export function isSuccess(res: ApiResponse): boolean {
  return res.success === true || res.succss === true;
}

export interface UserInfo {
  id: number;
  courseId: number;
  name: string;
  age: number;
  loginid: string;
  phone: string;
  status: number;
  startdate: string;
  enddate: string;
}

export interface LoginResponse extends ApiResponse {
  token: string;
  refreshToken?: string;
  user: UserInfo;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

// 单词 —— 与后端 WordListJson 返回对齐
export interface Word {
  Id: number;
  LexiconId: number;
  CourseContentId: number;
  en: string;
  cn: string;
  Name: string;
  Value: string;
  Zt: number; // 0未学 1已学 2熟悉
  isCollect: boolean;
  IsEnAudio: number;
  IsUsAudio: number;
  NumberSum: number;
  ZyNumber: number;
  YzNumber: number;
  TxNumber: number;
  FyNumber: number;
}

// 课程 —— 与后端 MyCategorys 返回的 courseInfo 对齐
export interface Course {
  courseId: number;
  courseName: string;
  WordsCount: number;
  DoneCount: number;
  Percentage: string; // "85.00" 格式
}

// 考试列表项 —— 与后端 ExamListJson 返回对齐
export interface Exam {
  Id: number;
  Name: string;
  examcount: number;
  createtime: string;
  txScore: number;
  yzScore: number;
  zyScore: number;
  txCompleted: boolean;
  yzCompleted: boolean;
  zyCompleted: boolean;
}

// 考试内容项 —— 与后端 ExamContentListJson 返回对齐
export interface ExamContentItem {
  Id: number;
  LexiconId: number;
  En: string;
  Cn: string;
  ExamId: number;
  Type: number;
  IsOk: boolean | null;
  Answer: string;
  Name: string;
  Value: string;
  IsEnAudio: number;
  IsUsAudio: number;
}

// 听力内容项 —— 与后端 HearingConent 返回的 HearingItem 对齐
export interface HearingContent {
  StartTime: number;
  EndTime: number;
  Value: string;
  ValueCN: string;
}

export interface StatData {
  bzcount: number; // 本周学习数
  count: number;   // 总学习数
  username: string;
}

// 后端 StatisticsLearnCountTwo 返回的月度统计组
export interface StatisticsLearnGroup {
  date: string;
  year: number;
  month: number;
  totalcount: number;
  statisticsLearns: Array<{ date: string; count: number; Year: number; Month: number; Day: number }>;
  task: { id: number; userid: number; startdate: string; count: number; weekend: number };
}

export interface LearningTask {
  id: number;
  count: number;
  date: string;
  type: number;
  weekend: number;
}
