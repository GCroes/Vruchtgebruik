using Vruchtgebruik.Api.Interfaces;

namespace Vruchtgebruik.Api.Middleware
{
    /// <summary>
    /// ASP.NET Core middleware that manages the correlation ID for each incoming HTTP request.
    /// Ensures a correlation ID is available throughout the request pipeline and returned in the response.
    /// </summary>
    internal class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationIdMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">Logger for diagnostic and trace information.</param>
        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Middleware entry point. Reads or generates a correlation ID,
        /// stores it in the <see cref="ICorrelationContext"/>, and ensures
        /// the ID is included in the response headers for tracing across distributed systems.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <param name="correlationContext">The application-scoped context for storing the correlation ID.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context, ICorrelationContext correlationContext)
        {
            Guid correlationId;
            if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var values) &&
                Guid.TryParse(values.FirstOrDefault(), out var parsed))
            {
                correlationId = parsed;
            }
            else
            {
                correlationId = Guid.NewGuid();
            }

            correlationContext.CorrelationId = correlationId;
            context.Response.Headers["X-Correlation-Id"] = correlationId.ToString();

            await _next(context);
        }
    }

}
