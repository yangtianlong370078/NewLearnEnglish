using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearnEnglish.Models.ErrorHelper
{
   
    public class UserService
    {

        private readonly IHttpContextAccessor _httpContextAccessor;


        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public User GetValidUser()
        {
            User user = null;


            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userid = httpContext.User.FindFirstValue("userid");
            var username = httpContext.User.FindFirstValue("username");
            var courseId = httpContext.User.FindFirstValue("courseId");
            var age = httpContext.User.FindFirstValue("age");
            var loginid = httpContext.User.FindFirstValue("loginid");
            var phone = httpContext.User.FindFirstValue("phone");
            var status = httpContext.User.FindFirstValue("status");
            var startdate = httpContext.User.FindFirstValue("startdate");
            var enddate = httpContext.User.FindFirstValue("enddate");


            if (userid != null)
            {
                user = new User();
                user.id = Convert.ToInt32(userid);
                user.name = Convert.ToString(username);
                user.courseId = Convert.ToInt32(courseId);
                user.age = Convert.ToInt32(age);
                user.loginid = Convert.ToString(loginid);
                user.phone = Convert.ToString(phone);
                user.status = Convert.ToInt32(status);
                user.startdate = Convert.ToDateTime(startdate);
                user.enddate = Convert.ToDateTime(enddate);
            }

            if (user == null)
            {
                throw new InvalidUserException("请重新登录");
            }

            if (user.enddate != default && user.enddate < DateTime.Now)
            {
                user.status = 2;
                throw new InvalidUserException("账户已过期");
            }
            return user;
        }
    }
}
