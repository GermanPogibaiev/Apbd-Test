namespace ApbdTest.DTOs;

public class CreateVendorDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public List<CreateVendorProductDto> Products { get; set; } = new();
}

public class CreateVendorProductDto
{
    public int Id { get; set; }
    public int Amount { get; set; }
    public decimal PricePerUnit { get; set; }
}