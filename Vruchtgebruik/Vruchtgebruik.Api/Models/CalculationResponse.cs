namespace Vruchtgebruik.Api.Models
{
    /// <summary>
    /// Represents the result of a usage value calculation, including the input asset value, 
    /// the factor used, and the calculated usage value.
    /// </summary>
    public class CalculationResponse
    {
        /// <summary>
        /// The asset value provided in the original calculation request.
        /// </summary>
        public decimal AssetValue { get; set; }

        /// <summary>
        /// The factor that was applied during the calculation.
        /// </summary>
        public decimal UsedFactor { get; set; }

        /// <summary>
        /// The final calculated usage value (rounded to the nearest integer).
        /// </summary>
        public decimal UsageValue { get; set; }
    }

}
