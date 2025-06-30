using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Vruchtgebruik.Api.Extensions
{
    public static class HealthCheckExtensions
    {
        /// <summary>
        /// Maps a standardized health check endpoint with JSON output.
        /// </summary>
        /// <param name="app">The WebApplication instance.</param>
        /// <param name="path">The health check path (default: "/health").</param>
        public static void MapCustomHealthChecks(this WebApplication app, string path = "/health")
        {
            app.MapHealthChecks(path, new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        results = report.Entries.Select(e => new
                        {
                            key = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description
                        })
                    });
                    await context.Response.WriteAsync(result);
                }
            });
        }
    }
}
