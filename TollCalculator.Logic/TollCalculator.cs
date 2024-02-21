using System.Linq.Expressions;
using TollCalculator.Logic.Models;

namespace TollCalculator.Logic;

public interface ITollCalculator
{
    int GetTollFee(Vehicle vehicle, DateTime[] dates);
}

public class TollCalculator : ITollCalculator
{
    /**
 * Calculate the total toll fee for one day
 *
 * @param vehicle - the vehicle
 * @param dates   - date and time of all passes on one day
 * @return - the total toll fee for that day
 */
    public int GetTollFee(Vehicle vehicle, DateTime[] dates)
    {
        var totalFee = 0;

        var datesGroupedByDate = GetDatesGroupedByDate(dates);

        foreach (var dateGroup in datesGroupedByDate)
        {
            var totalFeeForDateGroup = 0;
            var datesInGroup = dateGroup.Select(x => x).ToArray();
            var datesGroupedByHourInterval = GetDatesGroupedByHourInterval(datesInGroup);

            foreach (var dateGroupByHourInterval in datesGroupedByHourInterval)
            {
                var intervalStart = dateGroupByHourInterval.Key;
                var datesInHourIntervalGroup = dateGroupByHourInterval.Value;

                if (!datesInHourIntervalGroup.Any())
                {
                    totalFeeForDateGroup += GetTollFee(vehicle, dateGroupByHourInterval.Key);
                    continue;
                }

                var highestFeeInHourGroup = GetTollFee(vehicle, intervalStart);
                foreach (var date in datesInHourIntervalGroup)
                {
                    var nextFee = GetTollFee(vehicle, date);

                    if (nextFee >= highestFeeInHourGroup)
                    {
                        highestFeeInHourGroup = nextFee;
                    }
                }

                totalFeeForDateGroup += highestFeeInHourGroup;
            }

            if (totalFeeForDateGroup > 60)
            {
                totalFeeForDateGroup = 60;
            }

            totalFee += totalFeeForDateGroup;
        }

        return totalFee;
    }

    private IGrouping<DateTime, DateTime>[] GetDatesGroupedByDate(DateTime[] dates) =>
        dates.GroupBy(x => x.Date).ToArray();

    private Dictionary<DateTime, List<DateTime>> GetDatesGroupedByHourInterval(DateTime[] dates)
    {
        var datesSortedAscending = dates.OrderBy(x => x.Millisecond).ToArray();

        var groupedDates = new Dictionary<DateTime, List<DateTime>>
            { { datesSortedAscending.First(), new List<DateTime>() } };

        var currentIntervalStartTime = datesSortedAscending.First();
        foreach (var date in datesSortedAscending.Skip(1))
        {
            if (IsDateWithinOneHourFromIntervalStart(currentIntervalStartTime, date))
            {
                groupedDates[currentIntervalStartTime].Add(date);
            }
            else
            {
                groupedDates.Add(date, new List<DateTime>());
                currentIntervalStartTime = date;
            }
        }

        return groupedDates;
    }

    private bool IsDateWithinOneHourFromIntervalStart(DateTime intervalStart, DateTime currentDateTime)
    {
        var diffInMinutes = currentDateTime.Subtract(intervalStart).TotalMinutes;
        return diffInMinutes <= 60;
    }

    private bool IsTollFreeVehicle(Vehicle vehicle)
    {
        if (vehicle == null) return false;
        var vehicleType = vehicle.GetVehicleType();
        return vehicleType.Equals(TollFreeVehicles.Motorbike.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Tractor.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Emergency.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Diplomat.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Foreign.ToString()) ||
               vehicleType.Equals(TollFreeVehicles.Military.ToString());
    }

    public int GetTollFee(Vehicle vehicle, DateTime date)
    {
        if (IsTollFreeDate(date) || IsTollFreeVehicle(vehicle)) return 0;

        var hour = date.Hour;
        var minute = date.Minute;

        if (hour == 6 && minute >= 0 && minute <= 29) return 8;
        else if (hour == 6 && minute >= 30 && minute <= 59) return 13;
        else if (hour == 7 && minute >= 0 && minute <= 59) return 18;
        else if (hour == 8 && minute >= 0 && minute <= 29) return 13;
        else if (hour >= 8 && hour <= 14 && minute >= 30 && minute <= 59) return 8;
        else if (hour == 15 && minute >= 0 && minute <= 29) return 13;
        else if (hour == 15 && minute >= 0 || hour == 16 && minute <= 59) return 18;
        else if (hour == 17 && minute >= 0 && minute <= 59) return 13;
        else if (hour == 18 && minute >= 0 && minute <= 29) return 8;
        else return 0;
    }

    private bool IsTollFreeDate(DateTime date)
    {
        var year = date.Year;
        var month = date.Month;
        var day = date.Day;

        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return true;

        if (year == 2013)
        {
            if (month == 1 && day == 1 ||
                month == 3 && (day == 28 || day == 29) ||
                month == 4 && (day == 1 || day == 30) ||
                month == 5 && (day == 1 || day == 8 || day == 9) ||
                month == 6 && (day == 5 || day == 6 || day == 21) ||
                month == 7 ||
                month == 11 && day == 1 ||
                month == 12 && (day == 24 || day == 25 || day == 26 || day == 31))
            {
                return true;
            }
        }

        return false;
    }

    private enum TollFreeVehicles
    {
        Motorbike = 0,
        Tractor = 1,
        Emergency = 2,
        Diplomat = 3,
        Foreign = 4,
        Military = 5
    }
}