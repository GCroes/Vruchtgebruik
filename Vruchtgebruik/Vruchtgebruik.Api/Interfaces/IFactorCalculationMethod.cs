using Vruchtgebruik.Api.Models;

namespace Vruchtgebruik.Api.Interfaces
{
    public interface IFactorCalculationMethod
    {
        CalculationResponse Calculate(CalculationRequest req, Guid correlationId);
        string Name { get; }
    }
}
