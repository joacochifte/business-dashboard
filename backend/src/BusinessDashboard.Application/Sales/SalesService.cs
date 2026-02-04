using BusinessDashboard.Infrastructure.Sales;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using BusinessDashboard.Domain.Sales;

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
        var items = CreateItemsFromRequest(request.Items);
        var sale = new Sale(items, request.CustomerName, request.PaymentMethod);

        if (request.Total > 0 && request.Total != sale.Total)
            throw new InvalidOperationException("Total mismatch.");

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

    public async Task UpdateSaleAsync(Guid saleId, SaleUpdateDto request, CancellationToken ct = default)
    {
        var sale = await _repo.GetByIdAsync(saleId);

        sale.SetCustomerName(request.CustomerName);
        sale.SetPaymentMethod(request.PaymentMethod);

        await _repo.UpdateAsync(sale, ct);
    }

    private static IEnumerable<SaleItem> CreateItemsFromRequest(IEnumerable<SaleItemDto> items)
    {
        if (items is null || !items.Any())
            throw new InvalidOperationException("A sale must have at least one item.");

        var saleItems = items.Select(i => new SaleItem(
            productId: i.ProductId,
            quantity: i.Quantity,
            unitPrice: i.UnitPrice
        ));
        return saleItems;
    }

    private static SaleDto MapToDto(Sale sale)
    {
        return new SaleDto
        {
            ProductId = sale.Items.First().ProductId,
            Items = sale.Items.Select(i => new SaleItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice

            }).ToList(),
            CustomerName = sale.CustomerName,
            PaymentMethod = sale.PaymentMethod,
            Total = sale.Total,
            CreatedAt = sale.CreatedAt
        };
    }
}
