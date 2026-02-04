using BusinessDashboard.Infrastructure.Sales;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using BusinessDashboard.Domain.Sales;
using BusinessDashboard.Application;
using BusinessDashboard.Infrastructure.Products;

namespace BusinessDashboard.Application.Sales;



public sealed class SalesService : ISalesService
{
    private readonly ISaleRepository _repo;

    public SalesService(ISaleRepository repo)
    {
        _repo = repo;
    }

    public async Task<Guid> CreateSaleAsync(SaleCreationDto request, CancellationToken ct = default)
    {
        if (request.Items is null || !request.Items.Any())
            throw new InvalidOperationException("A sale must have at least one product.");

        if (request.Total <= 0)
            throw new ArgumentOutOfRangeException(nameof(request.Total), "Total must be greater than 0.");

        var items = CreateItemsFromRequest(request.Items, request.Total);
        var sale = new Sale(items);

        await _repo.AddAsync(sale, ct);
        return sale.Id;
    }

    public async Task DeleteSaleAsync(Guid saleId, CancellationToken ct = default)
    {
        await _repo.DeleteAsync(saleId, ct);
    }

    public async Task<IEnumerable<SaleDto>> GetAllSalesAsync(CancellationToken ct = default)
    {
        var sales = await _repo.GetAllAsync();
        return sales.Select(MapToDto);
    }

    public async Task<SaleDto> GetSaleByIdAsync(Guid saleId, CancellationToken ct = default)
    {
        var sale = await _repo.GetByIdAsync(saleId);
        return MapToDto(sale);
    }

    public Task UpdateSaleAsync(Guid saleId, SaleUpdateDto request, CancellationToken ct = default)
    {
        throw new InvalidOperationException("Sales cannot be updated.");
    }

    private static IEnumerable<SaleItem> CreateItemsFromRequest(IEnumerable<SaleItemDto> items, decimal total)
    {
        var itemList = items.ToList();
        if (itemList.Count == 0)
            throw new InvalidOperationException("A sale must have at least one product.");

        var unitPrice = total / itemList.Count;
        if (unitPrice <= 0)
            throw new ArgumentOutOfRangeException(nameof(total), "Total must be greater than 0.");

        return itemList.Select(p => new SaleItem(p.ProductId, quantity: 1, unitPrice: unitPrice));
    }

    private static SaleDto MapToDto(Sale sale)
    {
        return new SaleDto
        {
            ProductId = sale.Items.First().ProductId,
            Items = sale.Items.Select(i => new SaleItemDto
            {
                ProductId = i.ProductId,
                Quantity =  i.Quantity,
                UnitPrice = i.UnitPrice
                
            }),
            Total = sale.Total,
            CreatedAt = sale.CreatedAt
        };
    }
}
