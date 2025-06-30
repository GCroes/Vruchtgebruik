using Vruchtgebruik.Api.Controllers;
using Vruchtgebruik.Api.Interfaces;

namespace Vruchtgebruik.Api.Factories
{
    /// <summary>
    /// Factory for resolving and returning an <see cref="IFactorCalculationMethod"/> implementation
    /// based on the provided method name. Supports multiple calculation strategies.
    /// </summary>
    public class FactorCalculationMethodFactory : IFactorCalculationMethodFactory
    {
        private readonly Dictionary<string, IFactorCalculationMethod> _strategies;
        private readonly ILogger<CalculateController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FactorCalculationMethodFactory"/> class.
        /// </summary>
        /// <param name="strategies">
        /// The available calculation strategies, each implementing <see cref="IFactorCalculationMethod"/>.
        /// </param>
        /// <param name="logger">
        /// The logger used for logging method selection and warnings.
        /// </param>
        public FactorCalculationMethodFactory(IEnumerable<IFactorCalculationMethod> strategies, ILogger<CalculateController> logger)
        {
            _strategies = strategies.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
            _logger = logger;
        }

        /// <summary>
        /// Returns the calculation strategy corresponding to the given method name.
        /// Logs the selection or a warning if the method is unknown.
        /// </summary>
        /// <param name="methodName">The name of the factor calculation method to resolve.</param>
        /// <param name="correlationId">The correlation ID for tracing/logging purposes.</param>
        /// <returns>
        /// The matching <see cref="IFactorCalculationMethod"/> implementation.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if no strategy is found for the specified method name.
        /// </exception>
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
