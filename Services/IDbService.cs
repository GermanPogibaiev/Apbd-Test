using ApbdTest.DTOs;

namespace ApbdTest.Services;

public interface IDbService
{
    Task<VendorDetailsDto?> GetVendorByCodeAsync(string code);
    Task<bool> VendorExistsAsync(string code);
    Task<bool> ProductsExistAsync(List<int> productIds);
    Task AddVendorAsync(CreateVendorDto dto);
}