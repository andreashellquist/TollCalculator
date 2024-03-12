using Microsoft.Extensions.Logging;
using Moq;
using TollCalculator.Controllers;
using TollCalculator.Dtos;
using TollCalculator.Logic;
using TollCalculator.Logic.Models;

namespace TollCalculator.Tests.TollCalculator.Api;

public class TollCalculatorControllerTests
{
    [Fact]
    public void TollCalculatorController_WhenNoPassingsIsRegisteredInDto_Returns_0_AndLogsTheCorrectReason()
    {
        //arrange
        var vehiclePassingDto = new VehiclePassingsDto
        {
            Vehicle = new Vehicle
            {
                VehicleType = VehicleType.Car
            },
            Passings = new DateTime[] { }
        };

        var tollCalculatorMoq = new Mock<ITollCalculator>();
        tollCalculatorMoq.Setup(x =>
            x.GetTotalTollFeeForMultiplePassings(vehiclePassingDto.Vehicle, vehiclePassingDto.Passings)).Returns(0);

        var loggerMoq = new Mock<ILogger<TollCalculatorController>>();

        //act
        var controller = new TollCalculatorController(tollCalculatorMoq.Object, loggerMoq.Object);
        var totalFee = controller.Get(vehiclePassingDto);

        //assert
        Assert.Equal(0, totalFee);
        loggerMoq.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "No passings registered for vehicle. Skipping calculation." && @type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    
    //More tests can be written to validate more input cases
}