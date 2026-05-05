using ApbdTest.DTOs;
using ApbdTest.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApbdTest.Controllers;

[ApiController]
[Route("api/vendors")]
public class VendorsController : ControllerBase
{
    private readonly IDbService _dbService;

    public VendorsController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetVendor(string code)
    {
        var vendor = await _dbService.GetVendorByCodeAsync(code);

        if (vendor == null)
        {
            return NotFound($"Vendor with code {code} was not found.");
        }

        return Ok(vendor);
    }

    [HttpPost]
    public async Task<IActionResult> AddVendor(CreateVendorDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
        {
            return BadRequest("Vendor code is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("Vendor name is required.");
        }

        if (dto.Code.Length > 10)
        {
            return BadRequest("Vendor code cannot be longer than 10 characters.");
        }

        if (dto.Name.Length > 100)
        {
            return BadRequest("Vendor name cannot be longer than 100 characters.");
        }

        if (dto.Products.Any(p => p.Amount < 0))
        {
            return BadRequest("Product amount cannot be negative.");
        }

        if (dto.Products.Any(p => p.PricePerUnit < 0))
        {
            return BadRequest("Product price cannot be negative.");
        }

        var duplicatedProducts = dto.Products
            .GroupBy(p => p.Id)
            .Any(g => g.Count() > 1);

        if (duplicatedProducts)
        {
            return BadRequest("Duplicated products are not allowed.");
        }

        var vendorExists = await _dbService.VendorExistsAsync(dto.Code);

        if (vendorExists)
        {
            return Conflict($"Vendor with code {dto.Code} already exists.");
        }

        var productIds = dto.Products.Select(p => p.Id).ToList();
        var productsExist = await _dbService.ProductsExistAsync(productIds);

        if (!productsExist)
        {
            return BadRequest("One or more products do not exist.");
        }

        await _dbService.AddVendorAsync(dto);

        var createdVendor = await _dbService.GetVendorByCodeAsync(dto.Code);

        return CreatedAtAction(nameof(GetVendor), new { code = dto.Code }, createdVendor);
    }
}