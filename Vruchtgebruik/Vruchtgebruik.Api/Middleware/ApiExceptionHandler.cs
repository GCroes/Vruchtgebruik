using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Vruchtgebruik.Api.Models;

namespace Vruchtgebruik.Api.Middleware
{
    /// <summary>
    /// Provides a global exception handler for unhandled exceptions in the ASP.NET Core middleware pipeline.
    /// Logs the error and returns a standard API error response with trace and correlation IDs.
    /// </summary>
    public static class ApiExceptionHandler
    {
        /// <summary>
        /// Handles unhandled exceptions by logging the error and writing a standardized JSON error response.
        /// This method is intended to be used with <c>app.UseExceptionHandler()</c> in the request pipeline.
        /// </summary>
        /// <param name="context">The current HTTP context in which the exception occurred.</param>
        /// <returns>A completed <see cref="Task"/> when the error response has been written.</returns>
        internal static async Task Handle(HttpContext context)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("ApiExceptionHandler");

            var traceId = context.TraceIdentifier;
            string correlationId = context.Request.Headers["X-Correlation-Id"];

            logger.LogError(exceptionHandlerPathFeature?.Error,
                "Unhandled exception: {Message} | TraceId={TraceId} | CorrelationId={CorrelationId}",
                exceptionHandlerPathFeature?.Error?.Message, traceId, correlationId);

            var apiError = new ApiErrorResponse
            {
                Error = "An unexpected error occurred.",
                TraceId = traceId,
                CorrelationId = string.IsNullOrEmpty(correlationId) ? null : correlationId
            };

            await context.Response.WriteAsJsonAsync(apiError);
        }
    }

}
