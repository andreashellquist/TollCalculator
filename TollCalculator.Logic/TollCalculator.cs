using TollCalculator.Logic.Models;

namespace TollCalculator.Logic;

public interface ITollCalculator
{
    int GetTollFee(Vehicle vehicle, DateTime passing);
    int GetTotalTollFeeForMultiplePassings(Vehicle vehicleType, DateTime[] passages);
}

public class TollCalculator : ITollCalculator
{
    /**
     * Calculate the total toll fee for passings over a period of time
     *
     * @param vehicleType - the vehicleType
     * @param passages   - date and time of all passings over a period of time
     * @return - the total toll fee for that period of time
     */
    public int GetTotalTollFeeForMultiplePassings(Vehicle vehicleType, DateTime[] passages)
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
                    totalFeeForCurrentSixtyMinuteIntervalPassageGroup +=
                        GetTollFee(vehicleType, sixtyMinuteIntervalPassageGroup.Key);
                    continue;
                }

                var highestFeeInSixtyMinuteGroup = GetTollFee(vehicleType, intervalStart);
                foreach (var passage in passagesInSixtyMinuteIntervalGroup)
                {
                    var nextFee = GetTollFee(vehicleType, passage);

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

    /**
     * Calculate the toll fee for a single passing occurence
     *
     * @param vehicleType - the vehicleType
     * @param passages   - date and time of the passing
     * @return - the toll fee for the single passing
     */
    public int GetTollFee(Vehicle vehicle, DateTime passing)
    {
        if (IsTollFreeDate(passing) || vehicle.IsTollFree()) return 0;

        var hour = passing.Hour;
        var minute = passing.Minute;

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
}