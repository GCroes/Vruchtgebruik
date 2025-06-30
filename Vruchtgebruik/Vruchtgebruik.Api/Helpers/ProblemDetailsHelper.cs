using Microsoft.AspNetCore.Mvc;

namespace Vruchtgebruik.Api.Helpers
{
    public static class ProblemDetailsHelper
    {
        /// <summary>
        /// Creates a ProblemDetails or ValidationProblemDetails with standard extensions.
        /// </summary>
        public static ProblemDetails CreateProblemDetails(
            HttpContext httpContext,
            int statusCode,
            string title,
            string detail = null,
            string errorCode = null,
            string supportUrl = null,
            IDictionary<string, string[]> errors = null,
            string instance = null,
            string correlationId = null)
        {
            ProblemDetails pd = errors == null
                ? new ProblemDetails()
                : new ValidationProblemDetails(errors);

            pd.Title = title;
            pd.Status = statusCode;
            pd.Detail = detail;
            pd.Instance = instance ?? httpContext.Request.Path;

            // Standard extensions
            pd.Extensions["traceId"] = httpContext.TraceIdentifier;
            pd.Extensions["timestamp"] = DateTimeOffset.UtcNow;

            if (!string.IsNullOrEmpty(correlationId))
                pd.Extensions["correlationId"] = correlationId;
            if (!string.IsNullOrEmpty(errorCode))
                pd.Extensions["errorCode"] = errorCode;
            if (!string.IsNullOrEmpty(supportUrl))
                pd.Extensions["supportUrl"] = supportUrl;

            return pd;
        }
    }
}
