using BusinessDashboard.Infrastructure.Sales;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using BusinessDashboard.Domain.Sales;
using BusinessDashboard.Domain.Inventory;

namespace BusinessDashboard.Application.Sales;

public sealed class SalesService : ISalesService
{
    private readonly ISaleRepository _saleRepo;
    private readonly IProductRepository _productRepo;

    public SalesService(ISaleRepository saleRepo, IProductRepository productRepo)
    {
        _saleRepo = saleRepo;
        _productRepo = productRepo;
    }

    public async Task<Guid> CreateSaleAsync(SaleCreationDto request, CancellationToken ct = default)
    {
        var items = CreateItemsFromRequest(request.Items).ToList();
        var sale = new Sale(items, request.CustomerName, request.PaymentMethod);

        if (request.Total > 0 && request.Total != sale.Total)
            throw new InvalidOperationException("Total mismatch.");

        var movements = await AdjustStockAndCreateMovements(items, sale.CreatedAt, ct);
        await _saleRepo.AddAsync(sale, movements, ct);
        return sale.Id;
    }

    public async Task DeleteSaleAsync(Guid saleId, CancellationToken ct = default)
    {
        await _saleRepo.DeleteAsync(saleId, ct);
    }

    public async Task<IEnumerable<SaleDto>> GetAllSalesAsync(CancellationToken ct = default)
    {
        var sales = await _saleRepo.GetAllAsync();
        return sales.Select(MapToDto);
    }

    public async Task<SaleDto> GetSaleByIdAsync(Guid saleId, CancellationToken ct = default)
    {
        var sale = await _saleRepo.GetByIdAsync(saleId);
        return MapToDto(sale);
    }

    public async Task UpdateSaleAsync(Guid saleId, SaleUpdateDto request, CancellationToken ct = default)
    {
        var sale = await _saleRepo.GetByIdAsync(saleId);

        sale.SetCustomerName(request.CustomerName);
        sale.SetPaymentMethod(request.PaymentMethod);

        await _saleRepo.UpdateAsync(sale, ct);
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

    private async Task<IReadOnlyList<InventoryMovement>> AdjustStockAndCreateMovements(
        IReadOnlyList<SaleItem> items,
        DateTime createdAt,
        CancellationToken ct = default)
    {
        var movements = new List<InventoryMovement>();

        // Prevent repeated products in the same sale.
        var perProduct = items
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) });

        foreach (var entry in perProduct)
        {
            var product = await _productRepo.GetByIdAsync(entry.ProductId);

            // Products with Stock == null do not track inventory.
            if (product.Stock is null)
                continue;

            if (product.Stock.Value < entry.Quantity)
                throw new InvalidOperationException($"Insufficient stock for product {product.Name}.");

            product.AdjustStock(-entry.Quantity);

            movements.Add(new InventoryMovement(
                productId: entry.ProductId,
                type: InventoryMovementType.Out,
                reason: InventoryMovementReason.Sale,
                quantity: entry.Quantity,
                createdAt: createdAt
            ));
        }

        return movements;
    }
    private static SaleDto MapToDto(Sale sale)
    {
        return new SaleDto
        {
            Id = sale.Id,
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
