using BusinessDashboard.Domain.Common;

namespace BusinessDashboard.Domain.Sales;

public class SaleItem : Entity
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal? SpecialPrice { get; private set; }

    public decimal LineTotal => Quantity * (SpecialPrice ?? UnitPrice);

    private SaleItem() { }

    public SaleItem(Guid productId, int quantity, decimal unitPrice, decimal? specialPrice = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId is required.");

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        if (unitPrice <= 0)
            throw new ArgumentOutOfRangeException(nameof(unitPrice));

        if (specialPrice.HasValue && specialPrice <= 0)
            throw new ArgumentOutOfRangeException(nameof(specialPrice));

        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        SpecialPrice = specialPrice;
    }

    public void SetSpecialPrice(decimal? specialPrice)
    {
        if (specialPrice.HasValue && specialPrice <= 0)
            throw new ArgumentOutOfRangeException(nameof(specialPrice));
        SpecialPrice = specialPrice;
    }
}
