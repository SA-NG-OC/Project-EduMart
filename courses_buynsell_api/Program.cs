using courses_buynsell_api.Config;
using courses_buynsell_api.Data;
using courses_buynsell_api.DTOs.Momo;
using courses_buynsell_api.Entities;
using courses_buynsell_api.Extensions;
using courses_buynsell_api.Hubs;
using courses_buynsell_api.Interfaces;
using courses_buynsell_api.Middlewares;
using courses_buynsell_api.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Nạp .env TRƯỚC
Env.Load();

// 🔹 Thêm Environment Variables VÀO Configuration
builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value!.Errors.Count > 0)
            .ToDictionary(
                k => k.Key,
                v => v.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var result = new
        {
            success = false,
            message = "Validation failed.",
            errors
        };

        return new BadRequestObjectResult(result);
    };
});

builder.Services.Configure<MomoOptions>(options =>
{
    options.PartnerCode = Environment.GetEnvironmentVariable("MOMO_PARTNER_CODE")!;
    options.AccessKey = Environment.GetEnvironmentVariable("MOMO_ACCESS_KEY")!;
    options.SecretKey = Environment.GetEnvironmentVariable("MOMO_SECRET_KEY")!;
    options.ApiUrl = Environment.GetEnvironmentVariable("MOMO_API_URL")!;
    options.ReturnUrl = Environment.GetEnvironmentVariable("MOMO_RETURN_URL")!;
    options.NotifyUrl = Environment.GetEnvironmentVariable("MOMO_NOTIFY_URL")!;
    options.RequestType = Environment.GetEnvironmentVariable("MOMO_REQUEST_TYPE")!;
});


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

// Đăng ký SignalR
builder.Services.AddSignalR();

// Cấu hình CORS để frontend có thể kết nối SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "http://127.0.0.1:5500",
                "http://localhost:5500"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


builder.Services.Configure<JwtSettings>(opt =>
{
    opt.Key = jwtSettings.Key;
    opt.Issuer = jwtSettings.Issuer;
    opt.Audience = jwtSettings.Audience;
    opt.ExpiryMinutes = jwtSettings.ExpiryMinutes;
});

builder.Services.Configure<CloudinaryOptions>(builder.Configuration.GetSection("Cloudinary"));

var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME")
                ?? builder.Configuration["Cloudinary:CloudName"];
var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY")
             ?? builder.Configuration["Cloudinary:ApiKey"];
var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
                ?? builder.Configuration["Cloudinary:ApiSecret"];

builder.Services.AddSingleton(provider =>
{
    var account = new CloudinaryDotNet.Account(cloudName ?? "", apiKey ?? "", apiSecret ?? "");
    return new CloudinaryDotNet.Cloudinary(account);
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
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ICourseService, CourseService>();

// Đăng ký Memory Cache
builder.Services.AddMemoryCache();

// 🔹 Controllers + Swagger
builder.Services
    .AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Error;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Vui lòng nhập token vào ô bên dưới (không cần gõ chữ 'Bearer ')",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// Sử dụng CORS
app.UseCors("AllowAll");
// thêm middleware JWT
app.UseMiddleware<JwtMiddleware>();
app.UseErrorHandling();
app.UseAuthentication();
app.UseAuthorization();
// Map SignalR Hub
app.MapHub<NotificationHub>("/notificationHub");
app.MapControllers();
app.Run();
