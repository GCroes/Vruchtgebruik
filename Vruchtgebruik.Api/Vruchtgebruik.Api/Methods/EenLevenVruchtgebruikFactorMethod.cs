using Vruchtgebruik.Api.Controllers;
using Vruchtgebruik.Api.Interfaces;
using Vruchtgebruik.Api.Models;
using Vruchtgebruik.Api.Settings;

namespace Vruchtgebruik.Api.Methods
{
    public class EenLevenVruchtgebruikFactorMethod : IFactorCalculationMethod
    {
        private readonly EenLevenSettings _settings;
        private readonly AgeFactorSettings _ageFactorSettings;
        private readonly ILogger<CalculateController> _logger;

        public string Name => "EenLeven";

        public EenLevenVruchtgebruikFactorMethod(EenLevenSettings settings, AgeFactorSettings ageFactorSettings, ILogger<CalculateController> logger)
        {
            _settings = settings;
            _ageFactorSettings = ageFactorSettings;
            _logger = logger;
        }

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
