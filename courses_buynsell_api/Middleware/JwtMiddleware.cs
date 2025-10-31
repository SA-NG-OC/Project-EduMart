using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using courses_buynsell_api.Config;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using courses_buynsell_api.Data;

namespace courses_buynsell_api.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtSettings _jwtSettings;

        public JwtMiddleware(RequestDelegate next, IOptions<JwtSettings> jwtSettings)
        {
            _next = next;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task Invoke(HttpContext context, AppDbContext dbContext)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                AttachUserToContext(context, dbContext, token);

            await _next(context);
        }

        private void AttachUserToContext(HttpContext context, AppDbContext dbContext, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userIdStr = jwtToken.Claims.First(x => x.Type == "id").Value;
                if (int.TryParse(userIdStr, out var userId))
                {
                    // Lưu int (không còn kiểu string)
                    context.Items["UserId"] = userId;
                    Console.WriteLine("Authenticated User ID: " + userId);
                }
                else
                {
                    // Nếu không parse được, bỏ qua (không throw)
                    Console.WriteLine("Failed to parse user id claim: " + userIdStr);
                }

                // Gắn user vào context để controller có thể lấy ra
                context.Items["UserId"] = userId;
                Console.WriteLine("Authenticated User ID: " + userId);
            }
            catch
            {
                throw new UnauthorizedAccessException("Invalid Token");
            }
        }
    }
}