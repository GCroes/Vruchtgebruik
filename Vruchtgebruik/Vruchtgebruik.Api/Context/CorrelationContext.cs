using Vruchtgebruik.Api.Interfaces;

namespace Vruchtgebruik.Api.Context
{
    /// <summary>
    /// Provides context for the current request's correlation ID, allowing 
    /// the correlation ID to be accessed and set throughout the application's 
    /// request pipeline.
    /// </summary>
    public class CorrelationContext : ICorrelationContext
    {
        /// <summary>
        /// Gets or sets the correlation ID for the current request.
        /// This value is used to trace a logical operation across 
        /// distributed services and logs.
        /// </summary>
        public Guid CorrelationId { get; set; }
    }
}
