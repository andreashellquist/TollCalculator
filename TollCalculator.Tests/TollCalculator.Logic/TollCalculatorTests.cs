using Moq;
using TollCalculator.Logic;
using TollCalculator.Logic.Models;

namespace TollCalculator.Tests.TollCalculator.Logic;

public class TollCalculatorTests
{
    [Fact]
    public void TollCalculator_WhenVehicleTypeIsIncludedInFreeVehicleTypes_Returns_0()
    {
        //arrange
        var vehicle = new Vehicle { VehicleType = VehicleType.Motorbike };
        var passingDate = new DateTime(2013, 03, 11, 8, 4, 0); //Monday

        var tollCalculatorMoq = new Mock<ITollFeeDateValidationService>();
        tollCalculatorMoq.Setup(x =>
            x.IsTollFreeDate(passingDate)).Returns(false);

        //act
        var tollCalculator = new global::TollCalculator.Logic.TollCalculator(tollCalculatorMoq.Object);
        var tollFee = tollCalculator.GetTollFee(vehicle, passingDate);

        //assert
        Assert.Equal(0, tollFee);
    }


    [Fact]
    public void TollCalculator_WhenDayIsWeekend_Returns_0()
    {
        //arrange
        var vehicle = new Vehicle { VehicleType = VehicleType.Car };
        var passingDate = new DateTime(2013, 03, 10, 8, 4, 0); //Sunday

        var tollCalculatorMoq = new Mock<ITollFeeDateValidationService>();
        tollCalculatorMoq.Setup(x =>
            x.IsTollFreeDate(passingDate)).Returns(true);

        //act
        var tollCalculator = new global::TollCalculator.Logic.TollCalculator(tollCalculatorMoq.Object);
        var tollFee = tollCalculator.GetTollFee(vehicle, passingDate);

        //assert
        Assert.Equal(0, tollFee);
    }

    [Theory]
    [InlineData(6, 0, 8)]
    [InlineData(6, 30, 13)]
    [InlineData(7, 0, 18)]
    [InlineData(8, 0, 13)]
    [InlineData(8, 30, 8)]
    [InlineData(15, 0, 13)]
    [InlineData(15, 30, 18)]
    [InlineData(17, 0, 13)]
    [InlineData(18, 0, 8)]
    [InlineData(18, 30, 0)]
    public void TollCalculatorReturns_ForEachTimeSlot_CorrectFeeIsRegistered(int hour, int minute, int expectedFee)
    {
        //arrange
        var vehicle = new Vehicle { VehicleType = VehicleType.Car };
        var passingDate = new DateTime(2013, 03, 11, hour, minute, 0); //Monday

        var tollCalculatorMoq = new Mock<ITollFeeDateValidationService>();
        tollCalculatorMoq.Setup(x =>
            x.IsTollFreeDate(passingDate)).Returns(false);

        //act
        var tollCalculator = new global::TollCalculator.Logic.TollCalculator(tollCalculatorMoq.Object);
        var tollFee = tollCalculator.GetTollFee(vehicle, passingDate);

        //assert
        Assert.Equal(expectedFee, tollFee);
    }

    [Fact]
    public void TollCalculator_WhenPassagesArrayIsEmpty_ShouldReturn_0()
    {
        //arrange
        var vehicle = new Vehicle { VehicleType = VehicleType.Car };
        var passingDates = new DateTime[] { };

        var tollCalculatorMoq = new Mock<ITollFeeDateValidationService>();
        tollCalculatorMoq.Setup(x =>
            x.IsTollFreeDate(It.IsAny<DateTime>())).Returns(false);

        //act
        var tollCalculator = new global::TollCalculator.Logic.TollCalculator(tollCalculatorMoq.Object);
        var tollFee = tollCalculator.GetTotalTollFeeForMultiplePassings(vehicle, passingDates);

        //assert
        Assert.Equal(0, tollFee);
    }

    [Fact]
    public void TollCalculator_WhenMultipleCongestionFeesAreRegisteredWithinAnHour_HighestFeeIsReturned()
    {
        //arrange
        var vehicle = new Vehicle { VehicleType = VehicleType.Car };
        var passingDates = new DateTime[]
        {
            new(2013, 03, 11, 6, 10, 0),
            new(2013, 03, 11, 6, 40, 0),
            new(2013, 03, 11, 7, 09, 0)
        };

        var tollCalculatorMoq = new Mock<ITollFeeDateValidationService>();
        tollCalculatorMoq.Setup(x =>
            x.IsTollFreeDate(It.IsAny<DateTime>())).Returns(false);

        //act
        var tollCalculator = new global::TollCalculator.Logic.TollCalculator(tollCalculatorMoq.Object);
        var tollFee = tollCalculator.GetTotalTollFeeForMultiplePassings(vehicle, passingDates);

        //assert
        Assert.Equal(18, tollFee);
    }

    [Fact]
    public void TollCalculator_WhenMultipleCongestionFeesIsRegisteredDuringDay_AggregationIsCorrect()
    {
        //arrange
        var vehicle = new Vehicle { VehicleType = VehicleType.Car };
        var passingDates = new DateTime[]
        {
            new(2013, 03, 11, 6, 30, 0),
            new(2013, 03, 11, 7, 31, 0),
            new(2013, 03, 11, 15, 00, 0),
        };
        var tollCalculatorMoq = new Mock<ITollFeeDateValidationService>();
        tollCalculatorMoq.Setup(x =>
            x.IsTollFreeDate(It.IsAny<DateTime>())).Returns(false);

        //act
        var tollCalculator = new global::TollCalculator.Logic.TollCalculator(tollCalculatorMoq.Object);
        var tollFee = tollCalculator.GetTotalTollFeeForMultiplePassings(vehicle, passingDates);

        //assert
        Assert.Equal(44, tollFee);
    }

    [Fact]
    public void TollCalculator_WhenDailyCongestionFeesExceedsMaximumDailyFee_MaximumDailyFeeIsReturned()
    {
        //arrange
        var vehicle = new Vehicle { VehicleType = VehicleType.Car };
        var passingDates = new DateTime[]
        {
            new(2013, 03, 11, 6, 30, 0),
            new(2013, 03, 11, 7, 31, 0),
            new(2013, 03, 11, 15, 00, 0),
            new(2013, 03, 11, 16, 01, 0)
        };

        var tollCalculatorMoq = new Mock<ITollFeeDateValidationService>();
        tollCalculatorMoq.Setup(x =>
            x.IsTollFreeDate(It.IsAny<DateTime>())).Returns(false);

        //act
        var tollCalculator = new global::TollCalculator.Logic.TollCalculator(tollCalculatorMoq.Object);
        var tollFee = tollCalculator.GetTotalTollFeeForMultiplePassings(vehicle, passingDates);

        //assert
        Assert.Equal(60, tollFee);
    }

    [Fact]
    public void TollCalculator_WhenCongestionFeesIsFromMultipleDays_DailyFeesIsAggregated()
    {
        //arrange
        var vehicle = new Vehicle { VehicleType = VehicleType.Car };
        var passingDates = new DateTime[]
        {
            new(2013, 03, 11, 6, 30, 0),
            new(2013, 03, 11, 7, 31, 0),
            new(2013, 03, 11, 15, 00, 0),
            new(2013, 03, 12, 6, 30, 0),
            new(2013, 03, 12, 7, 31, 0),
            new(2013, 03, 12, 15, 00, 0),
        };

        var tollCalculatorMoq = new Mock<ITollFeeDateValidationService>();
        tollCalculatorMoq.Setup(x =>
            x.IsTollFreeDate(It.IsAny<DateTime>())).Returns(false);

        //act
        var tollCalculator = new global::TollCalculator.Logic.TollCalculator(tollCalculatorMoq.Object);
        var tollFee = tollCalculator.GetTotalTollFeeForMultiplePassings(vehicle, passingDates);

        //assert
        Assert.Equal(88, tollFee);
    }
}