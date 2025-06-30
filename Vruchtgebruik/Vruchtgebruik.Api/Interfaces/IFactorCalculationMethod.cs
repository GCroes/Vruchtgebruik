using Vruchtgebruik.Api.Models;

namespace Vruchtgebruik.Api.Interfaces
{
    /// <summary>
    /// Defines a contract for implementing a factor calculation method,
    /// allowing different strategies for usage value calculation.
    /// </summary>
    public interface IFactorCalculationMethod
    {
        /// <summary>
        /// Executes the calculation logic for the provided request using this method's rules.
        /// </summary>
        /// <param name="req">The calculation request containing asset value, age, sex, and factor method.</param>
        /// <param name="correlationId">The correlation ID for tracing and logging the request.</param>
        /// <returns>
        /// A <see cref="CalculationResponse"/> containing the calculation result.
        /// </returns>
        CalculationResponse Calculate(CalculationRequest req, Guid correlationId);

        /// <summary>
        /// Gets the unique name or identifier of this factor calculation method.
        /// </summary>
        string Name { get; }
    }
}
