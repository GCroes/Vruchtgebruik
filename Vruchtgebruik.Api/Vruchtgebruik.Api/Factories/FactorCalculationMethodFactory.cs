using Vruchtgebruik.Api.Controllers;
using Vruchtgebruik.Api.Interfaces;

namespace Vruchtgebruik.Api.Factories
{
    public class FactorCalculationMethodFactory: IFactorCalculationMethodFactory
    {
        private readonly Dictionary<string, IFactorCalculationMethod> _strategies;
        private readonly ILogger<CalculateController> _logger;

        public FactorCalculationMethodFactory(IEnumerable<IFactorCalculationMethod> strategies, ILogger<CalculateController> logger)
        {
            _strategies = strategies.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
            _logger = logger;
        }

        public IFactorCalculationMethod GetStrategy(string methodName, Guid correlationId)
        {
            if (!_strategies.TryGetValue(methodName, out var strategy))
            {
                _logger.LogWarning("CorrelationId:{CorrelationId} - Unknown factor method requested: {Method}", correlationId, methodName);

                throw new ArgumentException("Unknown factor method");
            }

            _logger.LogInformation("CorrelationId:{CorrelationId} - Factor method selected: {Method}", correlationId, methodName);

            return strategy;
        }

    }
}
