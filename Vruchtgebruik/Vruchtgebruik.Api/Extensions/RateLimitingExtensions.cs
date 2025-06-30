using System.Text.Json;
using System.Threading.RateLimiting;

namespace Vruchtgebruik.Api.Extensions
{
    public static class RateLimitingExtensions
    {
        /// <summary>
        /// Adds global rate limiting with a custom 429 response to the service collection.
        /// </summary>
        /// <param name="services">The IServiceCollection.</param>
        /// <returns>The IServiceCollection for chaining.</returns>
        public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromSeconds(10),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 2
                        }));

                options.RejectionStatusCode = 429;

                options.OnRejected = async (context, token) =>
                {
                    var traceId = context.HttpContext.TraceIdentifier;
                    var correlationId = context.HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault();

                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";
                    context.HttpContext.Response.Headers["Retry-After"] = "10";

                    var result = new
                    {
                        error = "Too many requests. Please wait and try again.",
                        traceId,
                        correlationId
                    };
                    await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(result), token);
                };
            });
            return services;
        }
    }
}
