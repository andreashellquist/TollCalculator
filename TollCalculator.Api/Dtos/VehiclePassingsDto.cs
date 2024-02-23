using TollCalculator.Logic.Models;

namespace TollCalculator.Dtos;

public class VehiclePassingsDto
{
    public Vehicle Vehicle { get; set; }
    public DateTime[] Passings { get; set; }
}