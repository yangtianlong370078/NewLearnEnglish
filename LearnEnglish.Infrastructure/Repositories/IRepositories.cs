using LearnEnglish.Application.Dtos.Course;
using LearnEnglish.Application.Dtos.Word;
using LearnEnglish.Domain.Entities;

namespace LearnEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// 用户 Repository 接口
    /// </summary>
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByNameAsync(string name);
        Task<User?> GetByLoginIdAsync(string loginId);
        Task<int> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task UpdatePasswordAsync(int userId, string passwordHash, int passwordVersion);
        Task UpdateCourseAsync(int userId, int courseId);
    }

    /// <summary>
    /// 分类 Repository 接口
    /// </summary>
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);
        Task<IEnumerable<Category>> GetAllAsync();
    }

    /// <summary>
    /// 课程 Repository 接口
    /// </summary>
    public interface ICourseRepository
    {
        Task<Course?> GetByIdAsync(int id);
        Task<IEnumerable<Course>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Course>> GetAllAsync();
        Task<int> CreateAsync(Course course);
        Task UpdateNameAsync(int id, string name);
        Task DeleteAsync(int id);
        Task DeleteContentByCourseIdAsync(int courseId);
        Task DeleteLearnByCourseIdAsync(int courseId);
        /// <summary>
        /// 获取分类课程列表（含用户课程关联信息）
        /// </summary>
        Task<IEnumerable<CategoryDto>> GetCategoriesWithCoursesAsync(int userId, int type, bool onlyMy);
        /// <summary>
        /// 按课程统计已完成/未完成单词数
        /// </summary>
        Task<IEnumerable<CourseCountDto>> GetDoneCountsAsync(int userId, bool onlyMyCourse);
        Task<IEnumerable<CourseCountDto>> GetUndoneCountsAsync(int userId, bool onlyMyCourse);
    }

    /// <summary>
    /// 用户课程关联 Repository 接口
    /// </summary>
    public interface IMyCourseRepository
    {
        Task<int> CreateAsync(MyCourse myCourse);
        Task DeleteByUserAndCourseAsync(int userId, int courseId);
        Task DeleteByCourseIdAsync(int courseId);
        Task<bool> ExistsAsync(int userId, int courseId);
    }

    /// <summary>
    /// 课程内容 Repository 接口
    /// </summary>
    public interface ICourseContentRepository
    {
        Task<IEnumerable<CourseContent>> GetByCourseIdAsync(int courseId);
        Task<CourseContent?> GetByCourseAndLexiconAsync(int courseId, int lexiconId);
        Task<int> CreateAsync(CourseContent content);
        Task BulkInsertAsync(IEnumerable<CourseContent> contents);
        Task<int> GetCountByCourseIdAsync(int courseId);
        Task DeleteAsync(int id);
        Task DeleteByCourseIdAsync(int courseId);
    }

    /// <summary>
    /// 单词 Repository 接口
    /// </summary>
    public interface ILexiconRepository
    {
        Task<Lexicon?> GetByIdAsync(int id);
        Task<Lexicon?> GetByEnAsync(string en);
        Task<IEnumerable<Lexicon>> GetByIdsAsync(IEnumerable<int> ids);
        Task<int> CreateAsync(Lexicon lexicon);
        Task UpdateAsync(Lexicon lexicon);
        Task UpdateEnCnAsync(int id, int userId, string en, string cn);
        Task<IEnumerable<Lexicon>> GetWithoutAudioAsync(int limit);
    }

    /// <summary>
    /// 学习记录 Repository 接口
    /// </summary>
    public interface ILearnRepository
    {
        Task<Learn?> GetByIdAsync(int id);
        Task<IEnumerable<Learn>> GetByUserIdAndCourseContentIdsAsync(int userId, IEnumerable<int> courseContentIds);
        Task<int> CreateAsync(Learn learn);
        Task UpdateStatusAsync(int id, int status);
        Task UpdateCollectAsync(int id, int isCollect);
        Task UpdateNumberAsync(int id, int number);
        Task DeleteByCourseContentIdAsync(int courseContentId);
        Task DeleteByCourseContentIdsAsync(IEnumerable<int> courseContentIds);
    }

    /// <summary>
    /// 用户自定义单词 Repository 接口
    /// </summary>
    public interface IMyLexiconRepository
    {
        Task<MyLexicon?> GetByUserAndLexiconAsync(int userId, int lexiconId);
        Task<int> CreateOrUpdateAsync(MyLexicon myLexicon);
        Task UpdateCnAsync(int userId, int lexiconId, string cn);
        Task SetCollectAsync(int userId, int lexiconId, int isCollect);
        Task UpdateStatusAsync(int userId, int lexiconId, int status);
        Task UpsertNumberAsync(int userId, int lexiconId, string numberField, int number, int status);

        Task UpsertOrInsertNumberAsync(int userId, int lexiconId, int status);
        Task BatchUpsertNumbersAsync(int userId, Dictionary<int, int> lexiconNumbers, string numberField);
        Task<int> GetFavoriteCountAsync(int userId);
        /// <summary>
        /// 校准：查询需要状态变更的单词，返回 (lexiconId, currentStatus, newStatus)
        /// </summary>
        Task<IEnumerable<(int LexiconId, int OldStatus, int NewStatus)>> GetCalibrationChangesAsync(int userId);
        /// <summary>
        /// 批量更新状态（校准结果）
        /// </summary>
        Task ApplyCalibrationAsync(int userId, IEnumerable<(int LexiconId, int NewStatus)> changes);
        /// <summary>
        /// 考试提交后重置错题的指定维度次数（用于 InsertExamnswer）
        /// </summary>
        Task ResetExamProficiencyAsync(int examId, int type, string numberField);
    }

    /// <summary>
    /// 考试 Repository 接口
    /// </summary>
    public interface IExamRepository
    {
        Task<Exam?> GetByIdAsync(int id);
        Task<IEnumerable<Exam>> GetPagedByUserIdAsync(int userId, int pageIndex, int pageSize);
        Task<IEnumerable<Exam>> SearchPagedByUserIdAsync(int userId, string? name, int pageIndex, int pageSize);
        Task<int> CreateAsync(Exam exam);
        Task UpdateCountAsync(int examId, int count);
        Task DeleteAsync(int id);
        Task<int> GetCountByUserIdAsync(int userId);
        Task<int> SearchCountByUserIdAsync(int userId, string? name);
    }

    /// <summary>
    /// 考试详情 Repository 接口
    /// </summary>
    public interface IExamDetailRepository
    {
        Task<IEnumerable<ExamDetail>> GetByExamIdAsync(int examId);
        Task<ExamDetail?> GetByIdAsync(int id);
        Task BulkInsertAsync(IEnumerable<ExamDetail> details);
        Task DeleteByExamIdAsync(int examId);
        /// <summary>
        /// 选取考试单词（从已掌握的 mylexicon 中选取今日未考过的单词）
        /// </summary>
        Task<IEnumerable<ExamDetail>> SelectWordsForExamAsync(int userId, int count);
        /// <summary>
        /// 查询考试内容详情（联表查询 examdetail+lexicon+examnswer+mylexicon）
        /// </summary>
        Task<IEnumerable<Application.Dtos.Exam.ExamContentQueryDto>> GetExamContentWithDetailsAsync(int userId, int examId, int type);
    }

    /// <summary>
    /// 考试答案 Repository 接口
    /// </summary>
    public interface IExamAnswerRepository
    {
        Task<IEnumerable<ExamAnswer>> GetByExamIdAsync(int examId);
        Task<IEnumerable<ExamAnswer>> GetByExamIdAndTypeAsync(int examId, int type);
        Task<Dictionary<int, int>> CountByExamIdGroupByTypeAsync(int examId);
        Task<int> CreateAsync(ExamAnswer answer);
        Task BulkInsertAsync(IEnumerable<ExamAnswer> answers);
        Task DeleteByExamIdAsync(int examId);
        Task DeleteByExamIdAndTypeAsync(int examId, int type);
    }

    /// <summary>
    /// 考试记录 Repository 接口
    /// </summary>
    public interface IExamRecordRepository
    {
        Task<IEnumerable<ExamRecord>> GetByExamIdAsync(int examId);
        Task<ExamRecord?> GetByExamIdAndTypeAsync(int examId, int type);
        Task<int> CreateAsync(ExamRecord record);
        Task DeleteByExamIdAsync(int examId);
        Task DeleteByExamIdAndTypeAsync(int examId, int type);
    }

    /// <summary>
    /// 学习统计 Repository 接口
    /// </summary>
    public interface IMyLearnRepository
    {
        Task<int> GetCountByUserIdSinceDateAsync(int userId, DateTime startDate);
        Task<IEnumerable<(DateTime Date, int Count)>> GetDailyCountByUserIdAsync(int userId, DateTime startDate);
        Task<IEnumerable<(DateTime Date, int Count)>> GetDailyCountByUserIdAndDatesAsync(int userId, IEnumerable<DateTime> dates);
        Task InsertIgnoreAsync(MyLearn myLearn);
        Task DeleteByUserAndLexiconAsync(int userId, int lexiconId);

        Task<DateTime?> QueryByUserAndLexiconAsync(int userId, int lexiconId);
        Task BulkInsertIgnoreAsync(IEnumerable<MyLearn> records);
        Task BulkDeleteByUserAndLexiconIdsAsync(int userId, IEnumerable<int> lexiconIds);
    }

    /// <summary>
    /// 学习任务 Repository 接口
    /// </summary>
    public interface ILearnTaskRepository
    {
        Task<LearnTask?> GetByUserIdAndMonthAsync(int userId, int year, int month);
        Task<IEnumerable<LearnTask>> GetByUserIdSinceDateAsync(int userId, DateTime startDate);
        Task<int> CreateAsync(LearnTask task);
        Task UpdateAsync(LearnTask task);
    }

    /// <summary>
    /// 单词列表查询 Repository 接口（复杂联表查询专用）
    /// </summary>
    public interface IWordQueryRepository
    {
        /// <summary>
        /// 获取课程单词分页列表
        /// </summary>
        Task<(IEnumerable<ShowTranslateDto> items, int total, int newCount, int learningCount, int masteredCount)> GetCourseWordsPagedAsync(
            int userId, int courseId, int status, string? name, int pageIndex, int pageSize, string orderBy);

        /// <summary>
        /// 获取收藏单词分页列表
        /// </summary>
        Task<(IEnumerable<ShowTranslateDto> items, int total)> GetFavoriteWordsPagedAsync(
            int userId, string? name, int pageIndex, int pageSize);

        /// <summary>
        /// 检查单词是否存在于课程中
        /// </summary>
        Task<(bool exists, int isEnAudio, int isUsAudio)> CheckWordExistsAsync(int courseId, string en);
    }
}
