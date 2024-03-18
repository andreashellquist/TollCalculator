using TollCalculator.Logic;

namespace TollCalculator.Extensions;

public static class TollCalculatorExtensions
{
    public static void AddTollCalculatorDependencies(this IServiceCollection services)
    {
        services.AddTransient<ITollCalculator, Logic.TollCalculator>();
        services.AddTransient<ITollFeeDateValidationService, TollFeeDateValidationValidationService>();
    }
}