namespace Vruchtgebruik.Api.Interfaces
{
    /// <summary>
    /// Factory for resolving the appropriate <see cref="IFactorCalculationMethod"/> implementation
    /// based on a given method name and correlation ID.
    /// </summary>
    public interface IFactorCalculationMethodFactory
    {
        /// <summary>
        /// Returns the calculation strategy matching the specified method name.
        /// </summary>
        /// <param name="methodName">The name of the desired factor calculation method.</param>
        /// <param name="correlationId">The correlation ID for tracing/logging (can be used in implementation).</param>
        /// <returns>An implementation of <see cref="IFactorCalculationMethod"/>.</returns>
        IFactorCalculationMethod GetStrategy(string methodName, Guid correlationId);
    }
}
