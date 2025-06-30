using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Vruchtgebruik.Api.Methods;
using Vruchtgebruik.Api.Models;
using Vruchtgebruik.Api.Settings;

namespace Vruchtgebruik.UnitTests
{
    public class EenLevenVruchtgebruikFactorMethodTests
    {
        private EenLevenVruchtgebruikFactorMethod CreateMethod(int femaleAdj = 5, int maleAdj = 0)
        {
            var settings = new EenLevenSettings
            {
                ActiveVersion = "2024",
                Versions = new Dictionary<string, List<EenLevenFactorRow>>
                {
                    ["2024"] = new List<EenLevenFactorRow>
                {
                    new EenLevenFactorRow { MinAge = 20, MaxAge = 29, Factor = 20 },
                    new EenLevenFactorRow { MinAge = 30, MaxAge = 39, Factor = 19 }
                }
                }
            };
            var ageSettings = new AgeFactorSettings { FemaleAdjustment = femaleAdj, MaleAdjustment = maleAdj };
            var logger = new Mock<ILogger<Api.Controllers.CalculateController>>();
            return new EenLevenVruchtgebruikFactorMethod(settings, ageSettings, logger.Object);
        }

        [Fact]
        public void Calculate_ReturnsCorrectResult_ForMale()
        {
            var method = CreateMethod();
            var req = new CalculationRequest { AssetValue = 1000, Age = 30, Sex = "male", FactorMethod = "EenLeven" };

            var result = method.Calculate(req, Guid.NewGuid());

            result.UsageValue.Should().Be(1000 * 0.04m * 19);
            result.UsedFactor.Should().Be(19);
        }

        [Fact]
        public void Calculate_AdjustsAgeForFemale()
        {
            var method = CreateMethod();
            var req = new CalculationRequest { AssetValue = 1000, Age = 35, Sex = "female", FactorMethod = "EenLeven" };

            // Female adjustment is 5: 35-5=30, so matches the 30-39 factor
            var result = method.Calculate(req, Guid.NewGuid());

            result.UsedFactor.Should().Be(19);
        }

        [Fact]
        public void Calculate_NoMatchingFactor_ThrowsArgumentException()
        {
            var method = CreateMethod();
            var req = new CalculationRequest { AssetValue = 1000, Age = 10, Sex = "male", FactorMethod = "EenLeven" };

            Action act = () => method.Calculate(req, Guid.NewGuid());

            act.Should().Throw<ArgumentException>();
        }
    }
}