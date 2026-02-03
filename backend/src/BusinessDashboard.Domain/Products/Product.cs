using BusinessDashboard.Domain.Common;

namespace BusinessDashboard.Domain.Products;

public class Product : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int? Stock { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Product() { }

    public Product(string name, decimal price, int? initialStock = 0, string? description = null)
    {
        SetName(name);
        SetPrice(price);
        SetStock(initialStock);
        Description = description;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.");

        Name = name.Trim();
    }

    public void SetPrice(decimal price)
    {
        if (price <= 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be greater than 0.");

        Price = price;
    }

    public void Deactivate() => IsActive = false;
    public void AdjustStock(int delta)
    {
        if (Stock is null)
            throw new InvalidOperationException("Stock is not tracked for this product.");

        var newStock = Stock.Value + delta;
        if (newStock < 0)
            throw new InvalidOperationException("Stock cannot be negative.");

        Stock = newStock;
    }

    private void SetStock(int? stock)
    {
        if (stock is null)
        {
            Stock = null;
            return;
        }

        if (stock < 0)
            throw new ArgumentOutOfRangeException(nameof(stock), "Stock cannot be negative.");

        Stock = stock.Value;
    }
}
