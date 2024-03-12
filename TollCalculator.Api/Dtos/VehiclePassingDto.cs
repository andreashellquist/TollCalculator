using TollCalculator.Logic.Models;

namespace TollCalculator.Dtos;

public class VehiclePassingDto
{
    public Vehicle Vehicle { get; set; }
    public DateTime Passing { get; set; }
}