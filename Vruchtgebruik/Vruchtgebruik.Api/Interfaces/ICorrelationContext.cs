namespace Vruchtgebruik.Api.Interfaces
{
    public interface ICorrelationContext
    {
        Guid CorrelationId { get; set; }
    }
}
