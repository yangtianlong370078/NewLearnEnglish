using LearnEnglish.Application.Dtos.Course;
using LearnEnglish.Application.Interfaces;
using LearnEnglish.Domain.Entities;
using LearnEnglish.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace LearnEnglish.Infrastructure.Services
{
    /// <summary>
    /// 课程管理服务实现
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMyCourseRepository _myCourseRepository;
        private readonly ICourseContentRepository _courseContentRepository;
        private readonly ILexiconRepository _lexiconRepository;
        private readonly IMyLexiconRepository _myLexiconRepository;
        private readonly ILogger<CourseService> _logger;

        public CourseService(
            ICourseRepository courseRepository,
            IMyCourseRepository myCourseRepository,
            ICourseContentRepository courseContentRepository,
            ILexiconRepository lexiconRepository,
            IMyLexiconRepository myLexiconRepository,
            ILogger<CourseService> logger)
        {
            _courseRepository = courseRepository;
            _myCourseRepository = myCourseRepository;
            _courseContentRepository = courseContentRepository;
            _lexiconRepository = lexiconRepository;
            _myLexiconRepository = myLexiconRepository;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> InsertMyCourseAsync(int userId, int courseId)
        {
            if (await _myCourseRepository.ExistsAsync(userId, courseId))
                return false;

            var result = await _myCourseRepository.CreateAsync(new MyCourse
            {
                CourseId = courseId,
                UserId = userId,
                CreateDate = DateTime.Now
            });
            return result > 0;
        }

        /// <inheritdoc/>
        public async Task<List<CategoryInfoDto>> GetCategoryListAsync(int userId, int type)
        {
            var categories = await _courseRepository.GetCategoriesWithCoursesAsync(userId, type, onlyMy: false);
            var doneCounts = await _courseRepository.GetDoneCountsAsync(userId, onlyMyCourse: false);
            var undoneCounts = await _courseRepository.GetUndoneCountsAsync(userId, onlyMyCourse: false);

            return BuildCategoryInfoList(categories, doneCounts, undoneCounts, userId);
        }

        /// <inheritdoc/>
        public async Task<MyCategoryInfoDto> GetMyCategoryContentAsync(int userId, int type)
        {
            var categories = await _courseRepository.GetCategoriesWithCoursesAsync(userId, type, onlyMy: true);
            var doneCounts = await _courseRepository.GetDoneCountsAsync(userId, onlyMyCourse: false);
            var undoneCounts = await _courseRepository.GetUndoneCountsAsync(userId, onlyMyCourse: false);
            var collectCount = await _myLexiconRepository.GetFavoriteCountAsync(userId);

            var allInfos = BuildCategoryInfoList(categories, doneCounts, undoneCounts, userId);

            var result = new MyCategoryInfoDto
            {
                CategoryInfos = allInfos.Where(a => a.Id != 9).ToList(),
                MyCategoryInfos = allInfos.Where(a => a.Id == 9).ToList()
            };

            // 在"我的"分类中插入强化学习区
            if (type == 1 && result.MyCategoryInfos.Count > 0)
            {
                result.MyCategoryInfos.First().CourseInfos.Insert(
                    Math.Min(1, result.MyCategoryInfos.First().CourseInfos.Count),
                    new CourseInfoDto
                    {
                        CourseId = -100,
                        CourseName = "强化学习区",
                        WordsCount = collectCount,
                        DoneCount = 0,
                        Percentage = "0.00"
                    });
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<(List<CourseInfoDto> data, int collectCount)> GetMyCoursesProgressAsync(int userId, int courseId)
        {
            var doneCounts = await _courseRepository.GetDoneCountsAsync(userId, onlyMyCourse: true);
            var undoneCounts = await _courseRepository.GetUndoneCountsAsync(userId, onlyMyCourse: true);
            var collectCount = await _myLexiconRepository.GetFavoriteCountAsync(userId);

            var doneDict = doneCounts.ToDictionary(x => x.CourseId, x => x.Count);
            var undoneDict = undoneCounts.ToDictionary(x => x.CourseId, x => x.Count);
            var allIds = doneDict.Keys.Union(undoneDict.Keys).OrderBy(id => id).ToList();

            // 如果指定了 courseId，只返回该课程
            if (courseId > 0) allIds = allIds.Where(id => id == courseId).ToList();

            var result = allIds.Select(id =>
            {
                var wc = undoneDict.GetValueOrDefault(id, 0);
                var dc = doneDict.GetValueOrDefault(id, 0);
                var total = wc + dc;
                return new CourseInfoDto
                {
                    CourseId = id,
                    WordsCount = wc,
                    DoneCount = dc,
                    Percentage = total > 0 ? ((double)dc / total * 100).ToString("0.00") : "0.00"
                };
            }).ToList();

            return (result, collectCount);
        }

        /// <inheritdoc/>
        public async Task<(int id, string courseName, bool isEditable)> GetCourseInfoAsync(int userId, int courseId)
        {
            if (courseId == -100)
                return (-100, "强化学习区", false);

            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                return (courseId, string.Empty, false);

            var isEditable = course.UserId == userId && course.CategoryId == 9;
            return (course.Id, course.Name, isEditable);
        }

        /// <inheritdoc/>
        public async Task<int> SaveCourseAsync(int userId, int courseId, string name, int type)
        {
            if (type == 1)
            {
                // 新增课程
                var newCourse = new Course
                {
                    Name = name,
                    UserId = userId,
                    CategoryId = 9, // 自定义分类
                    CreateDate = DateTime.Now
                };
                var newId = await _courseRepository.CreateAsync(newCourse);

                // 自动加入用户课程列表
                await _myCourseRepository.CreateAsync(new MyCourse
                {
                    CourseId = newId,
                    UserId = userId,
                    CreateDate = DateTime.Now
                });

                return newId;
            }
            else
            {
                // 更新课程名称
                await _courseRepository.UpdateNameAsync(courseId, name);
                return courseId;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteCourseAsync(int userId, int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null) return;

            // 删除用户课程关联
            await _myCourseRepository.DeleteByCourseIdAsync(courseId);

            // 如果是自己创建的课程，级联删除所有内容
            if (course.UserId == userId)
            {
                await _courseRepository.DeleteLearnByCourseIdAsync(courseId);
                await _courseContentRepository.DeleteByCourseIdAsync(courseId);
                await _courseRepository.DeleteAsync(courseId);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SaveWordToCourseAsync(int userId, int courseId, string en, string cn)
        {
            // 查找单词是否存在
            var lexicon = await _lexiconRepository.GetByEnAsync(en);
            int lexiconId;

            if (lexicon == null)
            {
                // 不存在则创建
                lexiconId = await _lexiconRepository.CreateAsync(new Lexicon
                {
                    En = en,
                    Cn = cn,
                    UserId = userId
                });
            }
            else
            {
                lexiconId = lexicon.Id;
            }

            // 检查课程内容是否已关联
            var existing = await _courseContentRepository.GetByCourseAndLexiconAsync(courseId, lexiconId);
            if (existing != null) return false;

            // 添加关联
            await _courseContentRepository.CreateAsync(new CourseContent
            {
                CourseId = courseId,
                LexiconId = lexiconId,
                CreateDate = DateTime.Now
            });

            return true;
        }

        /// <summary>
        /// 构建分类信息列表
        /// </summary>
        private static List<CategoryInfoDto> BuildCategoryInfoList(
            IEnumerable<CategoryDto> categories,
            IEnumerable<CourseCountDto> doneCounts,
            IEnumerable<CourseCountDto> undoneCounts,
            int userId)
        {
            var doneDict = doneCounts.ToDictionary(x => x.CourseId, x => x.Count);
            var undoneDict = undoneCounts.ToDictionary(x => x.CourseId, x => x.Count);

            return categories
                .GroupBy(x => x.Id)
                .Select(group =>
                {
                    var first = group.First();
                    var info = new CategoryInfoDto
                    {
                        Id = group.Key,
                        Name = first.Name,
                        IsMy = first.UserId == userId,
                    };

                    if (!(group.Count() == 1 && first.CourseId == 0))
                    {
                        foreach (var item in group)
                        {
                            var wc = undoneDict.GetValueOrDefault(item.CourseId, 0);
                            var dc = doneDict.GetValueOrDefault(item.CourseId, 0);
                            var total = wc + dc;
                            var courseInfo = new CourseInfoDto
                            {
                                CourseId = item.CourseId,
                                CourseName = item.CourseName,
                                IsMyCourse = item.IsMyCourse,
                                WordsCount = wc,
                                DoneCount = dc,
                                Percentage = total > 0 ? ((double)dc / total * 100).ToString("0.00") : "0.00"
                            };
                            if (dc > 0) info.IsLearn = true;
                            info.CourseInfos.Add(courseInfo);
                        }
                    }

                    return info;
                }).ToList();
        }
    }
}
