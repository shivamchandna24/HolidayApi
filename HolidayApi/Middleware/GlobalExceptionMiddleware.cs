using HolidayApi.Domain;
using System.Net;
using System.Text.Json;

namespace HolidayApi.Application
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<GlobalExceptionMiddleware> logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = ApiEndpoints.ApplicationJsonContentType;

            var errorResponse = new ErrorResponseDto();
            int statusCode = exception switch
            {
                ExternalServiceException => StatusCodes.Status502BadGateway,
                HttpRequestException => StatusCodes.Status503ServiceUnavailable,
                DatabaseOperationException => StatusCodes.Status500InternalServerError,
                ServiceException => StatusCodes.Status500InternalServerError,
                JsonException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
       
            };

            PopulateErrorResponse(errorResponse, exception);

            context.Response.StatusCode = statusCode;
            var result = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(result);
        }

        private void PopulateErrorResponse(ErrorResponseDto errorResponse, Exception exception)
        {
            switch (exception)
            {
                case ExternalServiceException externalServiceException:
                    logger.LogError(externalServiceException, externalServiceException.Message);
                    SetErrorResponse(errorResponse, ExceptionMessages.ExternalServiceError, externalServiceException.Message);
                    break;

                case DatabaseOperationException databaseException:
                    logger.LogError(databaseException, databaseException.Message);
                    SetErrorResponse(errorResponse, ExceptionMessages.DatabaseOperationFailed, databaseException.Message);
                    break;

                case JsonException jsonException:
                    logger.LogError(jsonException, jsonException.Message);
                    SetErrorResponse(errorResponse, ExceptionMessages.InternalServiceError, jsonException.Message);
                    break;

                case InvalidOperationException invalidOperationException:
                    logger.LogError(invalidOperationException, invalidOperationException.Message);
                    SetErrorResponse(errorResponse, ExceptionMessages.InternalServiceError, invalidOperationException.Message);
                    break;

                case ServiceException serviceException:
                    logger.LogError(serviceException, serviceException.Message);
                    SetErrorResponse(errorResponse, ExceptionMessages.InternalServiceError, serviceException.Message);
                    break;

                default:
                    logger.LogCritical(exception, ExceptionMessages.UnhandledError);
                    SetErrorResponse(errorResponse, ExceptionMessages.UnknownError, exception.Message);
                    break;
            }
        }

        private static void SetErrorResponse(ErrorResponseDto errorResponse, string message, string details)
        {
            errorResponse.Message = message;
            errorResponse.Errors = new Dictionary<string, string[]>
            {
                { "", new[] { details } }
            };
        }
    }
}
