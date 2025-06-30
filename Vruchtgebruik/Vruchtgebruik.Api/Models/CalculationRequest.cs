namespace Vruchtgebruik.Api.Models
{
    /// <summary>
    /// Represents a calculation request for determining the usage value based on Dutch tax rules.
    /// </summary>
    public class CalculationRequest
    {
        /// <summary>
        /// The value of the asset to be used in the calculation.
        /// </summary>
        public int AssetValue { get; set; }

        /// <summary>
        /// The age of the person (in years) for which the calculation applies.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// The sex of the person ("male" or "female").
        /// Used to adjust age-based factor rules.
        /// </summary>
        public string Sex { get; set; } = string.Empty;

        /// <summary>
        /// The name of the factor calculation method to use (e.g., "EenLeven", "Vruchtgebruik").
        /// </summary>
        public string FactorMethod { get; set; } = string.Empty;
    }

}
