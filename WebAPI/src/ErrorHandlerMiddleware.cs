using WebAPI;
using WebAPI.Exceptions;
namespace WebAPI;
public sealed class ErrorHandlerMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (AppException ex)
        {

            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                ex.StatusCode,
                ex.ErrorMessage,
            };
            await context.Response.WriteAsJsonAsync(response).ConfigureAwait(false);

        }

    }
}


