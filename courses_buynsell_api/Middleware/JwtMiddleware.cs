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
            var path = context.Request.Path.Value?.ToLower();

            // ⚠️ Bỏ qua xác thực cho các route public
            if (path.Contains("/auth/login") ||
                path.Contains("/auth/register") ||
                path.Contains("/auth/refresh-token") ||
                path.Contains("/auth/verify-email") ||
                path.Contains("/auth/forgot-password") ||
                path.Contains("/auth/check-otp") ||
                path.Contains("/auth/reset-password"))
            {
                await _next(context);
                return;
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                // ✅ Bọc try-catch ở đây để tự trả 401 khi token invalid
                try
                {
                    AttachUserToContext(context, dbContext, token);
                }
                catch (SecurityTokenExpiredException)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token expired");
                    return;
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid or unauthorized token");
                    return;
                }
                catch (Exception ex)
                {
                    // Bất kỳ lỗi nào khác vẫn coi như 401
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync($"Token validation failed: {ex.Message}");
                    return;
                }
            }

            await _next(context);
        }

        private void AttachUserToContext(HttpContext context, AppDbContext dbContext, string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero // Không cho phép trễ thời gian
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdStr = jwtToken.Claims.First(x => x.Type == "id").Value;

            if (int.TryParse(userIdStr, out var userId))
            {
                context.Items["UserId"] = userId;
                Console.WriteLine($"Authenticated User ID: {userId}");
            }
            else
            {
                throw new UnauthorizedAccessException("Invalid user ID in token");
            }
        }
    }
}
