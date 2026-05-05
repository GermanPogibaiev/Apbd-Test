namespace ApbdTest.DTOs;

public class VendorDetailsDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public List<ProductDto> Products { get; set; } = new();
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal StrickerPrice { get; set; }
    public ProductTypeDto ProductType { get; set; } = null!;
    public MakerDto Maker { get; set; } = null!;
    public VendorOfferDto VendorOffer { get; set; } = null!;
}

public class ProductTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class MakerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class VendorOfferDto
{
    public int Amount { get; set; }
    public decimal PricePerUnit { get; set; }
}