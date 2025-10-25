using Microsoft.EntityFrameworkCore;
using courses_buynsell_api.Entities;
using courses_buynsell_api.Data;
using courses_buynsell_api.Config;
using courses_buynsell_api.Interfaces;
using courses_buynsell_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Nạp .env TRƯỚC
Env.Load();

// 🔹 Thêm Environment Variables VÀO Configuration
builder.Configuration.AddEnvironmentVariables();


// 🔹 Kết nối PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("DB_CONNECTION")));

// 🔹 Cấu hình JWT
var jwtSettings = new JwtSettings
{
    Key = Environment.GetEnvironmentVariable("JWT_KEY")!,
    Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")!,
    Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")!,
    ExpiryMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES")!)
};
builder.Services.Configure<JwtSettings>(opt =>
{
    opt.Key = jwtSettings.Key;
    opt.Issuer = jwtSettings.Issuer;
    opt.Audience = jwtSettings.Audience;
    opt.ExpiryMinutes = jwtSettings.ExpiryMinutes;
});

// 🔹 Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });

// 🔹 Services
builder.Services.AddScoped<IAuthService, AuthService>();

// 🔹 Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
