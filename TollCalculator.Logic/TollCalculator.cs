using TollCalculator.Logic.Models;

namespace TollCalculator.Logic;

public interface ITollCalculator
{
    int GetTollFee(Vehicle vehicle, DateTime[] passages);
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
    public int GetTollFee(Vehicle vehicle, DateTime[] passages)
    {
        var totalFee = 0;

        var dateGroupedPassages = GetPassagesGroupedByDate(passages);

        foreach (var dateGroup in dateGroupedPassages)
        {
            var totalFeeForCurrentSixtyMinuteIntervalPassageGroup = 0;
            var passagesInDateGroup = dateGroup.Select(x => x).ToArray();
            var sixtyMinuteIntervalPassageGroups = GetPassagesGroupedBySixtyMinuteInterval(passagesInDateGroup);

            foreach (var sixtyMinuteIntervalPassageGroup in sixtyMinuteIntervalPassageGroups)
            {
                var intervalStart = sixtyMinuteIntervalPassageGroup.Key;
                var passagesInSixtyMinuteIntervalGroup = sixtyMinuteIntervalPassageGroup.Value;

                if (!passagesInSixtyMinuteIntervalGroup.Any())
                {
                    totalFeeForCurrentSixtyMinuteIntervalPassageGroup += GetTollFee(vehicle, sixtyMinuteIntervalPassageGroup.Key);
                    continue;
                }

                var highestFeeInSixtyMinuteGroup = GetTollFee(vehicle, intervalStart);
                foreach (var passage in passagesInSixtyMinuteIntervalGroup)
                {
                    var nextFee = GetTollFee(vehicle, passage);

                    if (nextFee >= highestFeeInSixtyMinuteGroup)
                    {
                        highestFeeInSixtyMinuteGroup = nextFee;
                    }
                }

                totalFeeForCurrentSixtyMinuteIntervalPassageGroup += highestFeeInSixtyMinuteGroup;
            }

            if (totalFeeForCurrentSixtyMinuteIntervalPassageGroup > 60)
            {
                totalFeeForCurrentSixtyMinuteIntervalPassageGroup = 60;
            }

            totalFee += totalFeeForCurrentSixtyMinuteIntervalPassageGroup;
        }

        return totalFee;
    }

    private IGrouping<DateTime, DateTime>[] GetPassagesGroupedByDate(DateTime[] passages) =>
        passages.GroupBy(x => x.Date).ToArray();

    private Dictionary<DateTime, List<DateTime>> GetPassagesGroupedBySixtyMinuteInterval(DateTime[] passages)
    {
        var passagesSortedAscending = passages.OrderBy(x => x.Millisecond).ToArray();

        var groupedPassages = new Dictionary<DateTime, List<DateTime>>();

        DateTime currentIntervalStartTime = default;
        foreach (var passage in passagesSortedAscending)
        {
            if (currentIntervalStartTime == default)
            {
                currentIntervalStartTime = passage;
                groupedPassages.Add(passage, new List<DateTime>());
            }
            
            if (IsPassageWithinOneHourFromIntervalStart(currentIntervalStartTime, passage))
            {
                groupedPassages[currentIntervalStartTime].Add(passage);
            }
            else
            {
                groupedPassages.Add(passage, new List<DateTime>());
                currentIntervalStartTime = passage;
            }
        }

        return groupedPassages;
    }

    private bool IsPassageWithinOneHourFromIntervalStart(DateTime intervalStart, DateTime currentDateTime)
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

    public int GetTollFee(Vehicle vehicle, DateTime passage)
    {
        if (IsTollFreeDate(passage) || IsTollFreeVehicle(vehicle)) return 0;

        var hour = passage.Hour;
        var minute = passage.Minute;

        if (hour == 6 && minute >= 0 && minute <= 29) return 8;
        if (hour == 6 && minute >= 30 && minute <= 59) return 13;
        if (hour == 7 && minute >= 0 && minute <= 59) return 18;
        if (hour == 8 && minute >= 0 && minute <= 29) return 13;
        if (hour >= 8 && hour <= 14 && minute >= 30 && minute <= 59) return 8;
        if (hour == 15 && minute >= 0 && minute <= 29) return 13;
        if (hour == 15 && minute >= 0 || hour == 16 && minute <= 59) return 18;
        if (hour == 17 && minute >= 0 && minute <= 59) return 13;
        if (hour == 18 && minute >= 0 && minute <= 29) return 8;
        return 0;
    }

    private bool IsTollFreeDate(DateTime passage)
    {
        var year = passage.Year;
        var month = passage.Month;
        var day = passage.Day;

        if (passage.DayOfWeek == DayOfWeek.Saturday || passage.DayOfWeek == DayOfWeek.Sunday) return true;

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