using LearnEnglish.Application.Interfaces;
using LearnEnglish.Infrastructure.Redis;
using LearnEnglish.Infrastructure.Repositories;
using LearnEnglish.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace LearnEnglish.WebApi.Controllers
{
    /// <summary>
    /// 听力练习 API
    /// </summary>
    [Route("api/[controller]")]
    public class HearingController : ApiControllerBase
    {
        private readonly IRedisService _redisService;
        private readonly ICourseRepository _courseRepository;
        private readonly ICurrentUserService _currentUserService;

        public HearingController(
            IRedisService redisService,
            ICourseRepository courseRepository,
            ICurrentUserService currentUserService)
        {
            _redisService = redisService;
            _courseRepository = courseRepository;
            _currentUserService = currentUserService;
        }

        private int RequireUserId() => _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("用户未登录");

        /// <summary>听力练习课程基础信息</summary>
        [HttpGet("HearingLX")]
        [Authorize]
        public async Task<IActionResult> HearingLX(int kc)
        {
            var userId = RequireUserId();
            var ml = await _redisService.GetAsync($"ml_{kc}");
            var courseName = "";

            if (kc != -100)
            {
                var course = await _courseRepository.GetByIdAsync(kc);
                if (course != null && (course.UserId == 0 || course.UserId == userId))
                {
                    courseName = course.Name ?? "";
                }
            }

            return Ok(new { success = true, courseName, HearingId = kc, HearingML = ml ?? "" });
        }

        /// <summary>查询听力内容</summary>
        [HttpGet("HearingConent")]
        [Authorize]
        public async Task<IActionResult> HearingConent(int kc, int id)
        {
            var lr = await _redisService.HashGetAsync($"lr_{kc}", $"{id}");

            if (string.IsNullOrEmpty(lr))
            {
                return Ok(new { success = false, msg = "未找到听力数据" });
            }

            byte[] dataBytes = Convert.FromBase64String(lr);
            string decodedString = Encoding.UTF8.GetString(dataBytes);
            var items = JsonConvert.DeserializeObject<List<HearingItem>>(decodedString);

            return Ok(new { success = true, list = items });
        }
    }
}
