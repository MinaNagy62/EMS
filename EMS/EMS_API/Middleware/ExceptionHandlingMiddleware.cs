using System.Text.Json;
using EMS_Application.Common;
using EMS_Application.Exceptions;

namespace EMS_API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            NotFoundException ex => (
                StatusCodes.Status404NotFound,
                ApiResponse<object>.FailResponse(ex.Message)
            ),
            ValidationException ex => (
                StatusCodes.Status422UnprocessableEntity,
                ApiResponse<object>.FailResponse(ex.Message, ex.Errors)
            ),
            BadRequestException ex => (
                StatusCodes.Status400BadRequest,
                ApiResponse<object>.FailResponse(ex.Message)
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                ApiResponse<object>.FailResponse("An internal server error occurred.")
            )
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsJsonAsync(response, jsonOptions);
    }
}
