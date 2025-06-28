namespace Vruchtgebruik.Api.Interfaces
{
    public interface IFactorCalculationMethodFactory
    {
        IFactorCalculationMethod GetStrategy(string methodName, Guid correlationId);
    }
}
