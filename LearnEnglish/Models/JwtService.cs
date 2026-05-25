using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LearnEnglish.Models
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        
        public string GenerateJwtToken(User userInfo)
        {
            var claims = new[]
            {
            new Claim("username", userInfo.name), // 自定义参数名和值
            new Claim("userid", userInfo.id.ToString()),
            new Claim("courseId", userInfo.courseId.ToString()),
            new Claim("age", userInfo.age.ToString()),
            new Claim("loginid", userInfo.loginid.ToString()),
            new Claim("phone", userInfo.phone.ToString()),
            new Claim("status", userInfo.status.ToString()),
            new Claim("startdate", userInfo.startdate.ToString()),
            new Claim("enddate", userInfo.enddate.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, userInfo.name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, userInfo.id.ToString()),
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("Jwt:Issuer").Value,
                audience: _configuration.GetSection("Jwt:Audience").Value,
                claims: claims,
               // expires: DateTime.Now.AddHours(1), // 示例：有效期1小时
                 expires: DateTime.Now.AddYears(1), // 示例：有效期1小时
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
