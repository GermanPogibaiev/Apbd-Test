using ApbdTest.DTOs;
using Microsoft.Data.SqlClient;

namespace ApbdTest.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;

    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")!;
    }

    public async Task<VendorDetailsDto?> GetVendorByCodeAsync(string code)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = """
                    SELECT 
                        RTRIM(v.Code) AS VendorCode,
                        v.Name AS VendorName,

                        p.Id AS ProductId,
                        p.Name AS ProductName,
                        p.Description,
                        p.StickerPrice,

                        pt.Id AS ProductTypeId,
                        pt.Name AS ProductTypeName,

                        m.Id AS MakerId,
                        m.Name AS MakerName,

                        vp.Amount,
                        vp.PricePerUnit
                    FROM Vendors v
                    LEFT JOIN VendorProducts vp ON v.Code = vp.VendorCode
                    LEFT JOIN Products p ON vp.ProductId = p.Id
                    LEFT JOIN ProductTypes pt ON p.ProductTypeId = pt.Id
                    LEFT JOIN Makers m ON p.MakerId = m.Id
                    WHERE RTRIM(v.Code) = @code;
                    """;

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@code", code);

        await using var reader = await command.ExecuteReaderAsync();

        VendorDetailsDto? vendor = null;

        while (await reader.ReadAsync())
        {
            if (vendor == null)
            {
                vendor = new VendorDetailsDto
                {
                    Code = reader["VendorCode"].ToString()!,
                    Name = reader["VendorName"].ToString()!
                };
            }

            if (reader["ProductId"] == DBNull.Value)
            {
                continue;
            }

            var product = new ProductDto
            {
                Id = (int)reader["ProductId"],
                Name = reader["ProductName"].ToString()!,
                Description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString(),
                StrickerPrice = (decimal)reader["StickerPrice"],
                ProductType = new ProductTypeDto
                {
                    Id = (int)reader["ProductTypeId"],
                    Name = reader["ProductTypeName"].ToString()!
                },
                Maker = new MakerDto
                {
                    Id = (int)reader["MakerId"],
                    Name = reader["MakerName"].ToString()!
                },
                VendorOffer = new VendorOfferDto
                {
                    Amount = (int)reader["Amount"],
                    PricePerUnit = (decimal)reader["PricePerUnit"]
                }
            };

            vendor.Products.Add(product);
        }

        return vendor;
    }

    public async Task<bool> VendorExistsAsync(string code)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT 1 FROM Vendors WHERE RTRIM(Code) = @code;";

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@code", code);

        var result = await command.ExecuteScalarAsync();

        return result != null;
    }

    public async Task<bool> ProductsExistAsync(List<int> productIds)
    {
        if (productIds.Count == 0)
        {
            return true;
        }

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var parameters = new List<string>();

        for (var i = 0; i < productIds.Count; i++)
        {
            parameters.Add($"@id{i}");
        }

        var query = $"SELECT COUNT(*) FROM Products WHERE Id IN ({string.Join(",", parameters)});";

        await using var command = new SqlCommand(query, connection);

        for (var i = 0; i < productIds.Count; i++)
        {
            command.Parameters.AddWithValue($"@id{i}", productIds[i]);
        }

        var count = (int)await command.ExecuteScalarAsync();

        return count == productIds.Distinct().Count();
    }

    public async Task AddVendorAsync(CreateVendorDto dto)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var insertVendor = """
                               INSERT INTO Vendors (Code, Name)
                               VALUES (@code, @name);
                               """;

            await using var vendorCommand = new SqlCommand(insertVendor, connection, (SqlTransaction)transaction);
            vendorCommand.Parameters.AddWithValue("@code", dto.Code);
            vendorCommand.Parameters.AddWithValue("@name", dto.Name);

            await vendorCommand.ExecuteNonQueryAsync();

            foreach (var product in dto.Products)
            {
                var insertProduct = """
                                    INSERT INTO VendorProducts (ProductId, VendorCode, Amount, PricePerUnit)
                                    VALUES (@productId, @vendorCode, @amount, @pricePerUnit);
                                    """;

                await using var productCommand = new SqlCommand(insertProduct, connection, (SqlTransaction)transaction);
                productCommand.Parameters.AddWithValue("@productId", product.Id);
                productCommand.Parameters.AddWithValue("@vendorCode", dto.Code);
                productCommand.Parameters.AddWithValue("@amount", product.Amount);
                productCommand.Parameters.AddWithValue("@pricePerUnit", product.PricePerUnit);

                await productCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}