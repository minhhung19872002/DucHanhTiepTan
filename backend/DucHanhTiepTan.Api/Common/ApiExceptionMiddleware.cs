using System.Net;
using System.Text.Json;

namespace DucHanhTiepTan.Api.Common;

/// <summary>Thrown by services for business-rule violations; mapped to HTTP 400 with a Vietnamese message.</summary>
public class ApiException : Exception
{
    public int StatusCode { get; }
    public ApiException(string message, int statusCode = (int)HttpStatusCode.BadRequest) : base(message) => StatusCode = statusCode;
}

public class NotFoundException : ApiException
{
    public NotFoundException(string message = "Không tìm thấy dữ liệu") : base(message, (int)HttpStatusCode.NotFound) { }
}

public class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next; _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = ex.Message }));
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update failed");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            var detail = ex.InnerException?.Message ?? ex.Message;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Không thể lưu dữ liệu: " + detail }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = ex.Message }));
        }
    }
}
