namespace TollCalculator.Logic.Models;

public class Vehicle
{
    public VehicleType VehicleType { get; init; }

    public bool IsTollFree()
    {
        VehicleType[] tollFreeVehicleTypes =
        {
            VehicleType.Diplomat, VehicleType.Emergency, VehicleType.Foreign, VehicleType.Military,
            VehicleType.Motorbike,
            VehicleType.Tractor
        };
        return tollFreeVehicleTypes.Contains(VehicleType);
    }
}