namespace Vruchtgebruik.Api.Interfaces
{
    /// <summary>
    /// Abstraction for storing and accessing the correlation ID within the application scope.
    /// Used to enable tracing and logging of requests across middleware and services.
    /// </summary>
    public interface ICorrelationContext
    {
        /// <summary>
        /// Gets or sets the correlation ID for the current request.
        /// </summary>
        Guid CorrelationId { get; set; }
    }
}
