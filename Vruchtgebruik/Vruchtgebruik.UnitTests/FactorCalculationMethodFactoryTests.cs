using Moq;
using FluentAssertions;
using Vruchtgebruik.Api.Factories;
using Vruchtgebruik.Api.Interfaces;
using Microsoft.Extensions.Logging;

namespace Vruchtgebruik.UnitTests
{
    public class FactorCalculationMethodFactoryTests
    {
        [Fact]
        public void GetStrategy_KnownMethod_ReturnsStrategy()
        {
            // Arrange
            var methodMock = new Mock<IFactorCalculationMethod>();
            methodMock.Setup(m => m.Name).Returns("TestMethod");
            var loggerMock = new Mock<ILogger<Vruchtgebruik.Api.Controllers.CalculateController>>();
            var factory = new FactorCalculationMethodFactory(new[] { methodMock.Object }, loggerMock.Object);

            // Act
            var strategy = factory.GetStrategy("TestMethod", Guid.NewGuid());

            // Assert
            strategy.Should().Be(methodMock.Object);
        }

        [Fact]
        public void GetStrategy_UnknownMethod_ThrowsArgumentException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Vruchtgebruik.Api.Controllers.CalculateController>>();
            var factory = new FactorCalculationMethodFactory(Array.Empty<IFactorCalculationMethod>(), loggerMock.Object);

            // Act
            Action act = () => factory.GetStrategy("Unknown", Guid.NewGuid());

            // Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}