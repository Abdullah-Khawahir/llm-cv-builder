namespace WebAPI.Exceptions;

public sealed class AppException(string errorMessage, int statusCode, Exception? innerException) : Exception(errorMessage, innerException)
{
    public int StatusCode { get; } = statusCode;
    public string ErrorMessage { get; } = errorMessage;


    public static AppException BadRequest(string message, Exception? innerException = default) =>
        new(message, StatusCodes.Status400BadRequest, innerException);

    public static AppException Unauthorized(string message = "Unauthorized", Exception? innerException = default) =>
        new(message, StatusCodes.Status401Unauthorized, innerException);

    public static AppException Forbidden(string message = "Forbidden", Exception? innerException = default) =>
        new(message, StatusCodes.Status403Forbidden, innerException);

    public static AppException NotFound(string message = "Resource not found", Exception? innerException = default) =>
        new(message, StatusCodes.Status404NotFound, innerException);

    public static AppException Conflict(string message, Exception? innerException = default) =>
        new(message, StatusCodes.Status409Conflict, innerException);

    public static AppException UnprocessableEntity(string message, Exception? innerException = default) =>
        new(message, StatusCodes.Status422UnprocessableEntity, innerException);

    public static AppException TooManyRequests(string message = "Too many requests", Exception? innerException = default) =>
        new(message, StatusCodes.Status429TooManyRequests, innerException);

    public static AppException PdfGenerationFailed(string message = "Failed to generate PDF", Exception? innerException = default) =>
        new(message, StatusCodes.Status500InternalServerError, innerException);

    public static AppException NoContentToGenerate(string message = "No content to generate", Exception? innerException = default) =>
        new(message, StatusCodes.Status204NoContent, innerException);
    public static AppException Internal(string message = "Internal server error", Exception? innerException = default) =>
        new(message, StatusCodes.Status500InternalServerError, innerException);

    public static AppException ServiceUnavailable(string message = "Service Not avaliable", Exception? innerException = default) =>
        new(message, StatusCodes.Status503ServiceUnavailable, innerException);
}
