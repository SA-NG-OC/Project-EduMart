namespace courses_buynsell_api.Middlewares;

public class ValidationErrorMiddleware
{
    private readonly RequestDelegate _next;

    public ValidationErrorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next(context);

        // Nếu ModelState lỗi nhưng chưa trả response
        if (context.Response.StatusCode == 400 &&
            context.Items.ContainsKey("ValidationErrors"))
        {
            var errors = context.Items["ValidationErrors"];
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Validation failed.",
                errors
            });
        }
    }
}
