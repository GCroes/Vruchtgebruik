namespace Vruchtgebruik.Api.Models
{
    public class CalculationRequest
    {
        public int AssetValue { get; set; }
        public int Age { get; set; }

        /// <summary>
        /// "male" or "female"
        /// </summary>
        public string Sex { get; set; } = string.Empty;
        /// <summary>
        /// Name of the factor method to use, e.g. "default", "vruchtgebruik"
        /// </summary>
        public string FactorMethod { get; set; } = string.Empty;
    }
}
