using TollCalculator.Logic;

namespace TollCalculator.Extensions;

public static class TollCalculatorExtensions
{
    public static void AddTollCalculator(this IServiceCollection services)
    {
        services.AddTransient<ITollCalculator, Logic.TollCalculator>();
    }
}