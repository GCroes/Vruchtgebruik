using Vruchtgebruik.Api.Controllers;
using Vruchtgebruik.Api.Interfaces;
using Vruchtgebruik.Api.Models;
using Vruchtgebruik.Api.Settings;

namespace Vruchtgebruik.Api.Methods
{
    /// <summary>
    /// Implements the "EenLeven" factor calculation method for determining the usage value
    /// based on Dutch tax rules. Calculates the value using age-adjusted factors from configuration.
    /// </summary>
    public class EenLevenVruchtgebruikFactorMethod : IFactorCalculationMethod
    {
        private readonly EenLevenSettings _settings;
        private readonly AgeFactorSettings _ageFactorSettings;
        private readonly ILogger<CalculateController> _logger;

        /// <summary>
        /// Gets the name of the calculation method ("EenLeven").
        /// </summary>
        public string Name => "EenLeven";

        /// <summary>
        /// Initializes a new instance of the <see cref="EenLevenVruchtgebruikFactorMethod"/> class.
        /// </summary>
        /// <param name="settings">The factor table settings for the "EenLeven" method.</param>
        /// <param name="ageFactorSettings">Settings for age adjustment by sex.</param>
        /// <param name="logger">Logger for calculation and error events.</param>
        public EenLevenVruchtgebruikFactorMethod(
            EenLevenSettings settings,
            AgeFactorSettings ageFactorSettings,
            ILogger<CalculateController> logger)
        {
            _settings = settings;
            _ageFactorSettings = ageFactorSettings;
            _logger = logger;
        }

        /// <summary>
        /// Performs the calculation for the given request, adjusting age as per configuration,
        /// finding the correct factor row, and computing the usage value. 
        /// Logs calculation events and errors with the provided correlation ID.
        /// </summary>
        /// <param name="req">The calculation request, including asset value, age, and sex.</param>
        /// <param name="correlationId">The correlation ID for tracking and logging.</param>
        /// <returns>
        /// A <see cref="CalculationResponse"/> containing the result of the calculation.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if no applicable factor is found for the adjusted age.</exception>
        public CalculationResponse Calculate(CalculationRequest req, Guid correlationId)
        {
            try
            {
                int adjustedAge = req.Age;

                if (req.Sex?.ToLower() == "female")
                    adjustedAge -= _ageFactorSettings.FemaleAdjustment;
                else if (req.Sex?.ToLower() == "male")
                    adjustedAge -= _ageFactorSettings.MaleAdjustment;

                var version = _settings.Versions[_settings.ActiveVersion];
                var row = version.FirstOrDefault(r => adjustedAge >= r.MinAge && adjustedAge <= r.MaxAge);

                if (row == null)
                {
                    _logger.LogWarning("CorrelationId:{CorrelationId} - No factor found for age {Age} in method {Method}", correlationId, adjustedAge, Name);
                    throw new ArgumentException("No factor found for provided age");
                }

                var usageValue = req.AssetValue * 0.04m * row.Factor;

                _logger.LogInformation("CorrelationId:{CorrelationId} - Calculation success: method={Method}, assetValue={AssetValue}, adjAge={AdjAge}, usedFactor={Factor}, usageValue={UsageValue}",
                    correlationId, Name, req.AssetValue, adjustedAge, row.Factor, usageValue);

                return new CalculationResponse
                {
                    AssetValue = req.AssetValue,
                    UsedFactor = row.Factor,
                    UsageValue = Math.Round(usageValue, 0, MidpointRounding.AwayFromZero),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CorrelationId:{CorrelationId} - Exception occurred in {Method}: {Message}", correlationId, Name, ex.Message);
                throw;
            }
        }
    }

}
