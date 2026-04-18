namespace Entities.Interfaces;

public interface IVehicle
{
    string GetVehicleType();
    Guid Id { get; }
    string RegistrationNumber { get; }
}