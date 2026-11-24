namespace SampleWpfApp;

public sealed class Customer
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string City { get; init; }
    public bool IsActive { get; init; }

    public string IsActiveText => IsActive ? "Evet" : "Hayır";
}
