using Vruchtgebruik.Api.Interfaces;

namespace Vruchtgebruik.Api.Context
{
    public class CorrelationContext : ICorrelationContext
    {
        public Guid CorrelationId { get; set; }
    }
}
