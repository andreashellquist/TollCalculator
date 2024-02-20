using Microsoft.AspNetCore.Mvc;
using TollCalculator.Logic;

namespace TollCalculator.Controllers;

[ApiController]
[Route("[controller]")]
public class TollCalculatorController : ControllerBase
{
    private readonly ILogger<TollCalculatorController> _logger;

    public TollCalculatorController(ITollCalculator tollCalculator, ILogger<TollCalculatorController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "CalculateTollFee")]
    public int Get()
    {
        return 0;
    }
}