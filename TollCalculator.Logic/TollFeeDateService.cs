using TollCalculator.Logic.Models;

namespace TollCalculator.Logic;

public interface ITollFeeDateService

{
    bool IsTollFreeDate(DateTime passage);
}

public class TollFeeDateService : ITollFeeDateService
{
    private readonly ITollFeeDateService _tollFeeDateService;

    public TollFeeDateService(ITollFeeDateService tollFeeDateService)
    {
        _tollFeeDateService = tollFeeDateService;
    }
    
    public bool IsTollFreeDate(DateTime passage)
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