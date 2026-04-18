namespace Customers.Infrastructure.Data.Dtos;

public class CustomerAddressDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public CustomerDto Customer { get; set; } = null!;
    public string Label { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime CreatedDateTime { get; set; }
}
