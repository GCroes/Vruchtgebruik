using Microsoft.AspNetCore.Mvc;

namespace Vruchtgebruik.Api.Middleware
{
    /// <summary>
    /// Provides a handler for generating a standardized ProblemDetails response
    /// for HTTP 404 (Not Found) errors, including logging and correlation information.
    /// </summary>
    internal static class NotFoundHandler
    {
        /// <summary>
        /// Handles HTTP 404 responses by logging the missing resource and returning a
        /// ProblemDetails object with correlation and trace identifiers.
        /// Intended for use as middleware after endpoint routing.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Handle(HttpContext context)
        {
            // Only handle 404s
            if (context.Response.StatusCode == StatusCodes.Status404NotFound && !context.Response.HasStarted)
            {
                var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault();
                var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("NotFoundHandler");

                logger.LogWarning("404 Not Found: Path={Path}, CorrelationId={CorrelationId}, TraceId={TraceId}",
                    context.Request.Path, correlationId, context.TraceIdentifier);

                var problem = new ProblemDetails
                {
                    Title = "Resource Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = $"No endpoint found for path {context.Request.Path}",
                    Instance = context.Request.Path
                };
                problem.Extensions["correlationId"] = correlationId;
                problem.Extensions["traceId"] = context.TraceIdentifier;

                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(problem);
            }
        }
    }

}
