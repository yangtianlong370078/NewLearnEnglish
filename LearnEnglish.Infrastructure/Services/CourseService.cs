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
        private readonly ICurrentUserService _currentUserService;

        public CourseService(
            ICourseRepository courseRepository,
            IMyCourseRepository myCourseRepository,
            ICourseContentRepository courseContentRepository,
            ILexiconRepository lexiconRepository,
            IMyLexiconRepository myLexiconRepository,
            ILogger<CourseService> logger,
            ICurrentUserService currentUserService)
        {
            _courseRepository = courseRepository;
            _myCourseRepository = myCourseRepository;
            _courseContentRepository = courseContentRepository;
            _lexiconRepository = lexiconRepository;
            _myLexiconRepository = myLexiconRepository;
            _logger = logger;
            _currentUserService = currentUserService;
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
        public async Task<List<NotAddCategoryInfoDto>> GetCategoryListAsync(int userId, int type)
        {
            var categories = await _courseRepository.GetCategoriesWithCoursesAsync(userId, type, onlyMy: false);
            var allCounts = await _courseRepository.GetCourseCountsAsync(categories.Select(a=>a.CourseId).ToList());
            return BuildCategoryList(categories, allCounts);
        }


        /// <summary>
        /// 构建分类信息列表
        /// </summary>
        private static List<NotAddCategoryInfoDto> BuildCategoryList(
            IEnumerable<CategoryDto> categories,
            IEnumerable<CourseCountDto> allCounts)
        {
            var allDict = allCounts.ToDictionary(x => x.CourseId, x => x.Count);
            return categories
                .GroupBy(x => x.Id)
                .Select(group =>
                {
                    var first = group.First();
                    var info = new NotAddCategoryInfoDto
                    {
                        Id = group.Key,
                        Name = first.Name,
                    };

                    if (!(group.Count() == 1 && first.CourseId == 0))
                    {
                        foreach (var item in group)
                        {
                            var wc = allDict.GetValueOrDefault(item.CourseId, 0);
                            var courseInfo = new NotAddCourseInfoDto
                            {
                                CourseId = item.CourseId,
                                CourseName = item.CourseName,
                                WordsCount = wc,
                            };
                            info.CourseInfos.Add(courseInfo);
                        }
                    }
                    return info;
                }).ToList();

            
        }

        /// <inheritdoc/>
        public async Task<MyCategoryInfoDto> GetMyCategoryContentAsync(int userId, int type)
        {
            var categories = await _courseRepository.GetCategoriesWithCoursesAsync(userId, type, onlyMy: true);
            var doneCounts = await _courseRepository.GetDoneCountsAsync(userId, 3);
            var notDoneCounts = await _courseRepository.GetDoneCountsAsync(userId, 2);
            var allCounts = await _courseRepository.GetUndoneCountsAsync(userId);


            var collectCount = await _myLexiconRepository.GetFavoriteCountAsync(userId);

            var courseId = _currentUserService.GetValidUser().CourseId;

            var allInfos = BuildCategoryInfoList(categories, notDoneCounts, doneCounts, allCounts, userId, courseId);

            var lastCourseId = categories.FirstOrDefault()?.LastCourseId ?? 0;

            var result = new MyCategoryInfoDto
            {
                CategoryInfos = allInfos.Item1.Where(a => a.Id != 9).ToList(),
                MyCategoryInfos = allInfos.Item1.Where(a => a.Id == 9).ToList(),
                LastCourse = allInfos.Item1.SelectMany(a=>a.CourseInfos).Where(a=>a.CourseId== lastCourseId).FirstOrDefault()??new CourseInfoDto(),
            };

            // 在"我的"分类中插入强化学习区
            if (type == 1)
            {
                result.NewWord = allInfos.Item2;
                if (result.MyCategoryInfos.Count > 0)
                {
                    result.StrengthenWord = new CourseInfoDto
                    {
                        CourseId = -100,
                        CourseName = "强化学习区",
                        DoneCount = collectCount.DoneCount,
                        NotDoneCount = collectCount.NotDoneCount,
                        NotLearned = collectCount.NotLearned,
                        WordsCount = collectCount.DoneCount + collectCount.NotDoneCount + collectCount.NotLearned,
                        Percentage = "0.00"
                    };
                }
            }
            return result;
        }

        /// <inheritdoc/>
        public async Task<(List<CourseInfoDto> data, (int NotLearned, int NotDoneCount, int DoneCount))> GetMyCoursesProgressAsync(int userId, int courseId)
        {
            var doneCounts = await _courseRepository.GetDoneCountsAsync(userId, 3);
            var undoneCounts = await _courseRepository.GetUndoneCountsAsync(userId);
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
                if(courseId==0)
                {
                    // 新增课程
                    var newCourse = new Course
                    {
                        Name = name,
                        UserId = userId,
                        CategoryId = 9, // 自定义分类
                        CreateDate = DateTime.Now
                    };
                    courseId = await _courseRepository.CreateAsync(newCourse);

                    // 自动加入用户课程列表
                    await _myCourseRepository.CreateAsync(new MyCourse
                    {
                        CourseId = courseId,
                        UserId = userId,
                        CreateDate = DateTime.Now
                    });

                    return courseId;
                }
                else
                {
                    // 更新课程名称
                    await _courseRepository.UpdateNameAsync(courseId, name);
                    return courseId;
                }
            }
            else
            {
               
            }

            return courseId;
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
        private static (List<CategoryInfoDto>, CourseInfoDto) BuildCategoryInfoList(
            IEnumerable<CategoryDto> categories,
            IEnumerable<CourseCountDto> notDoneCounts,
            IEnumerable<CourseCountDto> doneCounts,
            IEnumerable<CourseCountDto> allCounts,
            int userId, int courseId)
        {
            var notDoneDict = notDoneCounts.ToDictionary(x => x.CourseId, x => x.Count);
            var doneDict = doneCounts.ToDictionary(x => x.CourseId, x => x.Count);
            var allDict = allCounts.ToDictionary(x => x.CourseId, x => x.Count);

            CourseInfoDto newWord = new CourseInfoDto();
            var data = categories
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
                            var nodc = notDoneDict.GetValueOrDefault(item.CourseId, 0);
                            var wc = allDict.GetValueOrDefault(item.CourseId, 0);
                            var dc = doneDict.GetValueOrDefault(item.CourseId, 0);

                            var courseInfo = new CourseInfoDto
                            {
                                CourseId = item.CourseId,
                                CourseName = item.CourseName,
                                IsMyCourse = item.IsMyCourse,
                                WordsCount = wc,
                                DoneCount = dc,
                                NotDoneCount = nodc,
                                NotLearned = wc - nodc - dc,
                                Percentage = wc > 0 ? ((double)(dc + nodc) / wc * 100).ToString("0.00") : "0.00"
                            };

                            if (courseId == item.CourseId)
                            {
                                newWord = courseInfo;
                            }
                            else
                            {
                                info.CourseInfos.Add(courseInfo);
                                if (dc > 0) info.IsLearn = true;
                            }

                        }
                    }
                    return info;
                }).ToList();


            return (data, newWord);
        }
    }
}
