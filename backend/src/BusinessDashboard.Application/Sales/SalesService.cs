using BusinessDashboard.Infrastructure.Sales;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using BusinessDashboard.Domain.Sales;
using BusinessDashboard.Domain.Inventory;
using BusinessDashboard.Domain.Common.Exceptions;

namespace BusinessDashboard.Application.Sales;

public sealed class SalesService : ISalesService
{
    private readonly ISaleRepository _saleRepo;
    private readonly IProductRepository _productRepo;
    private readonly ICustomerRepository _customerRepo;

    public SalesService(ISaleRepository saleRepo, IProductRepository productRepo, ICustomerRepository customerRepo)
    {
        _saleRepo = saleRepo;
        _productRepo = productRepo;
        _customerRepo = customerRepo;
    }

    public async Task<Guid> CreateSaleAsync(SaleCreationDto request, CancellationToken ct = default)
    {
        var items = CreateItemsFromRequest(request.Items).ToList();
        var sale = new Sale(items, request.CustomerId, request.PaymentMethod);

        if (request.Total > 0 && request.Total != sale.Total)
            throw new BusinessRuleException("Total mismatch.");

        if (request.CustomerId.HasValue)
        {
            var customer = await _customerRepo.GetByIdAsync(request.CustomerId.Value, ct);
            sale.SetCustomer(customer);
            customer.UpdateLastPurchaseDate(sale.CreatedAt, sale.Total);
            await _customerRepo.UpdateAsync(customer, ct);
        }

        var movements = await AdjustStockAndCreateMovements(items, sale.CreatedAt, ct);
        await _saleRepo.AddAsync(sale, movements, ct);
        return sale.Id;
    }

    public async Task DeleteSaleAsync(Guid saleId, CancellationToken ct = default)
    {
        var sale = await _saleRepo.GetByIdAsync(saleId);
        var movements = await RestoreStockAndCreateMovementsForSaleDelete(sale.Items.ToList(), DateTime.UtcNow, ct);
        await _saleRepo.DeleteAsync(saleId, movements, ct);
    }

    public async Task<IEnumerable<SaleDto>> GetAllSalesAsync(CancellationToken ct = default)
    {
        var sales = await _saleRepo.GetAllAsync();
        return sales.Select(MapToDto).OrderByDescending(s => s.CreatedAt);
    }

    public async Task<SaleDto> GetSaleByIdAsync(Guid saleId, CancellationToken ct = default)
    {
        var sale = await _saleRepo.GetByIdAsync(saleId);
        return MapToDto(sale);
    }

    public async Task UpdateSaleAsync(Guid saleId, SaleUpdateDto request, CancellationToken ct = default)
    {
        var sale = await _saleRepo.GetByIdAsync(saleId);
        var oldItems = sale.Items.ToList();
        var items = CreateItemsFromRequest(request.Items).ToList();

        sale.ReplaceItems(items);

        if (request.Total > 0 && request.Total != sale.Total)
            throw new BusinessRuleException("Total mismatch.");

        var movements = await AdjustStockForSaleUpdateAndCreateMovements(oldItems, items, DateTime.UtcNow, ct);

        if (request.CustomerId.HasValue)
        {
            var customer = await _customerRepo.GetByIdAsync(request.CustomerId.Value, ct);
            sale.SetCustomer(customer);
        }
        else
        {
            sale.SetCustomer(null);
        }

        sale.SetPaymentMethod(request.PaymentMethod);
        await _saleRepo.UpdateAsync(sale, movements, ct);
    }

    private static IEnumerable<SaleItem> CreateItemsFromRequest(IEnumerable<SaleItemDto> items)
    {
        if (items is null || !items.Any())
            throw new BusinessRuleException("A sale must have at least one item.");

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
            IsProductActive(product);
            // Products with Stock == null do not track inventory.
            if (product.Stock is null)
                continue;

            if (product.Stock.Value < entry.Quantity)
                throw new BusinessRuleException($"Insufficient stock for product {product.Name}.");

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

    private async Task<IReadOnlyList<InventoryMovement>> AdjustStockForSaleUpdateAndCreateMovements(
        IReadOnlyList<SaleItem> oldItems,
        IReadOnlyList<SaleItem> newItems,
        DateTime createdAt,
        CancellationToken ct = default)
    {
        var movements = new List<InventoryMovement>();

        var oldByProduct = oldItems
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        var newByProduct = newItems
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        var productIds = oldByProduct.Keys.Union(newByProduct.Keys);

        foreach (var productId in productIds)
        {
            var oldQty = oldByProduct.GetValueOrDefault(productId, 0);
            var newQty = newByProduct.GetValueOrDefault(productId, 0);
            var delta = newQty - oldQty;

            if (delta == 0)
                continue;

            var product = await _productRepo.GetByIdAsync(productId);

            // If quantity increased, treat it like a sale and enforce active/stock rules.
            if (delta > 0)
                IsProductActive(product);

            if (product.Stock is null)
                continue;

            if (delta > 0 && product.Stock.Value < delta)
                throw new BusinessRuleException($"Insufficient stock for product {product.Name}.");

            product.AdjustStock(-delta);

            movements.Add(new InventoryMovement(
                productId: productId,
                type: delta > 0 ? InventoryMovementType.Out : InventoryMovementType.In,
                reason: InventoryMovementReason.Sale,
                quantity: Math.Abs(delta),
                createdAt: createdAt
            ));
        }

        return movements;
    }

    private async Task<IReadOnlyList<InventoryMovement>> RestoreStockAndCreateMovementsForSaleDelete(
        IReadOnlyList<SaleItem> items,
        DateTime createdAt,
        CancellationToken ct = default)
    {
        var movements = new List<InventoryMovement>();

        var perProduct = items
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) });

        foreach (var entry in perProduct)
        {
            var product = await _productRepo.GetByIdAsync(entry.ProductId);

            // Products with Stock == null do not track inventory.
            if (product.Stock is null)
                continue;

            product.AdjustStock(entry.Quantity);

            movements.Add(new InventoryMovement(
                productId: entry.ProductId,
                type: InventoryMovementType.In,
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
            CustomerId = sale.CustomerId,
            CustomerName = sale.CustomerName,
            PaymentMethod = sale.PaymentMethod,
            Total = sale.Total,
            CreatedAt = sale.CreatedAt
        };
    }
    private void IsProductActive(Domain.Products.Product product)
    {
        if (!product.IsActive)
            throw new InvalidOperationException($"Product {product.Name} is inactive.");
    }
}
