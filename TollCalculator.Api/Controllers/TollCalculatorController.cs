using Microsoft.AspNetCore.Mvc;
using TollCalculator.Dtos;
using TollCalculator.Logic;
using TollCalculator.Logic.Models;

namespace TollCalculator.Controllers;

[ApiController]
[Route("[controller]")]
public class TollCalculatorController : ControllerBase
{
    private readonly ITollCalculator _tollCalculator;
    private readonly ILogger<TollCalculatorController> _logger;

    public TollCalculatorController(ITollCalculator tollCalculator, ILogger<TollCalculatorController> logger)
    {
        _tollCalculator = tollCalculator;
        _logger = logger;
    }

    [HttpGet(Name = "CalculateTollFeeByPassage")]
    public int Get(VehiclePassingsDto dto)
    {
        if (dto == null)
        {
            _logger.LogError("VehiclePassingsDto object is null.");
            throw new ArgumentNullException(nameof(dto));
        }
        if (dto.Vehicle.VehicleType == VehicleType.Undefined)
        {
            _logger.LogInformation("Vehicle type is not valid. Skipping calculation");
            return 0;
        }
        if (!dto.Passings.Any())
        {
            _logger.LogInformation("No passings registered for vehicle. Skipping calculation.");
            return 0;
        }

        var totalFee = dto.Passings.Length == 1
            ? _tollCalculator.GetTollFee(dto.Vehicle, dto.Passings.First())
            : _tollCalculator.GetTotalTollFeeForMultiplePassings(dto.Vehicle, dto.Passings);

        return totalFee;
    }
}