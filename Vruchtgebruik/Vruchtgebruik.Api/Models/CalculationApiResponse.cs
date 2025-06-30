namespace Vruchtgebruik.Api.Models
{
    /// <summary>
    /// Represents the standard API response for calculation operations,
    /// containing both the correlation ID for tracing and the calculation result payload.
    /// </summary>
    public class CalculationApiResponse
    {
        /// <summary>
        /// The unique correlation identifier for the request, useful for end-to-end tracing and logging.
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// The calculation result payload returned by the API.
        /// </summary>
        public CalculationResponse Response { get; set; }
    }


}
