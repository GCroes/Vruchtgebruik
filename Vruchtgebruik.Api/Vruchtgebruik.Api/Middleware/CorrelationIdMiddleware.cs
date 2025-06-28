using Vruchtgebruik.Api.Interfaces;

namespace Vruchtgebruik.Api.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

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
