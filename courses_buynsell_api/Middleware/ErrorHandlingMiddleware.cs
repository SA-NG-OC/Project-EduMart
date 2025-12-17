using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;
using courses_buynsell_api.Exceptions;

namespace courses_buynsell_api.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // ✅ Lưu response body gốc
        var originalBodyStream = context.Response.Body;

        try
        {
            // ✅ Sử dụng MemoryStream để buffer response
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            // ✅ Đọc response body
            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            // ✅ Reset để ghi lại
            memoryStream.Seek(0, SeekOrigin.Begin);

            // ✅ Xử lý các status code cần custom
            if (context.Response.StatusCode == (int)HttpStatusCode.BadRequest)
            {
                await HandleBadRequest(context, responseBody, originalBodyStream);
            }
            else if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                await WriteCustomErrorResponse(context, originalBodyStream, HttpStatusCode.Unauthorized,
                    "Unauthorized: You are not authorized to access this resource.");
            }
            else if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
            {
                await WriteCustomErrorResponse(context, originalBodyStream, HttpStatusCode.Forbidden,
                    "Forbidden: You do not have permission to access this resource.");
            }
            else
            {
                // ✅ Copy response gốc nếu không cần xử lý
                await memoryStream.CopyToAsync(originalBodyStream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            context.Response.Body = originalBodyStream;
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleBadRequest(HttpContext context, string responseBody, Stream originalBodyStream)
    {
        try
        {
            // ✅ Parse response gốc
            var originalResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

            object customResponse;

            // ✅ Kiểm tra xem có phải validation error không
            if (originalResponse.TryGetProperty("errors", out var errorsProperty))
            {
                // ✅ Đây là validation error từ ModelState
                customResponse = new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = JsonSerializer.Deserialize<Dictionary<string, string[]>>(errorsProperty.GetRawText())
                };
            }
            else
            {
                // ✅ BadRequest khác (từ throw BadRequestException)
                customResponse = new
                {
                    success = false,
                    message = originalResponse.TryGetProperty("message", out var msg)
                        ? msg.GetString()
                        : "Bad request.",
                    errors = (object?)null
                };
            }

            // ✅ Ghi response mới
            context.Response.Body = originalBodyStream;
            context.Response.ContentType = "application/json";
            var json = JsonSerializer.Serialize(customResponse);
            await context.Response.WriteAsync(json);
        }
        catch
        {
            // ✅ Nếu parse fail, giữ nguyên response gốc
            context.Response.Body = originalBodyStream;
            await originalBodyStream.WriteAsync(Encoding.UTF8.GetBytes(responseBody));
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            Console.WriteLine("Response has already started, cannot write custom error message.");
            return;
        }
        var statusCode = exception switch
        {
            BadRequestException => HttpStatusCode.BadRequest,
            UnauthorizedException => HttpStatusCode.Unauthorized,
            NotFoundException => HttpStatusCode.NotFound,
            KeyNotFoundException => HttpStatusCode.NotFound,
            DuplicateNameException => HttpStatusCode.Conflict,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            _ => HttpStatusCode.InternalServerError
        };

        var response = new
        {
            success = false,
            message = exception.Message,
            errors = exception.InnerException != null
                ? new { detail = exception.InnerException.Message }
                : (object?)null
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static async Task WriteCustomErrorResponse(HttpContext context, Stream originalBodyStream,
        HttpStatusCode status, string message)
    {
        context.Response.Body = originalBodyStream;
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var response = new
        {
            success = false,
            message,
            errors = (object?)null
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}