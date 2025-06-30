using FluentAssertions;
using Vruchtgebruik.Api.Models;
using Vruchtgebruik.Api.Validators;

namespace Vruchtgebruik.UnitTests
{
    public class CalculationRequestValidatorTests
    {
        [Theory]
        [InlineData(1000, 35, "male", "EenLeven", true)]
        [InlineData(-5, 35, "male", "EenLeven", false)]   // Invalid asset value
        [InlineData(1000, -1, "male", "EenLeven", false)] // Invalid age
        [InlineData(1000, 35, "", "EenLeven", false)]    // Empty sex
        [InlineData(1000, 35, "female", "", false)]       // Empty factor method
        public void CalculationRequestValidator_ValidatesCorrectly(int assetValue, int age, string sex, string method, bool isValid)
        {
            var validator = new CalculationRequestValidator();
            var request = new CalculationRequest
            {
                AssetValue = assetValue,
                Age = age,
                Sex = sex,
                FactorMethod = method
            };

            var result = validator.Validate(request);

            result.IsValid.Should().Be(isValid);
        }
    }
}
