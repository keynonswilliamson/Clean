namespace CleanArchitecture.Demo.Application.DTOs;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public List<OrderDto> Orders { get; set; } = new();
}

