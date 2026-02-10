using BusinessDashboard.Domain.Common;

namespace BusinessDashboard.Domain.Sales;

public class Sale : Entity
{
    private readonly List<SaleItem> _items = new();

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    public decimal Total { get; private set; }
    public string? CustomerName { get; private set; }
    public string? PaymentMethod { get; private set; }

    private Sale() { }

    public Sale(IEnumerable<SaleItem> items, string? customerName = null, string? paymentMethod = null)
    {
        if (items == null || !items.Any())
            throw new InvalidOperationException("A sale must have at least one item.");

        CustomerName = string.IsNullOrWhiteSpace(customerName) ? null : customerName.Trim();
        PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? null : paymentMethod.Trim();

        foreach (var item in items)
        {
            AddItem(item);
        }
    }

    public void SetCustomerName(string? customerName)
    {
        CustomerName = string.IsNullOrWhiteSpace(customerName) ? null : customerName.Trim();
    }

    public void SetPaymentMethod(string? paymentMethod)
    {
        PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? null : paymentMethod.Trim();
    }

    public void AddItem(SaleItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        _items.Add(item);
        RecalculateTotal();
    }

    public void ReplaceItems(IEnumerable<SaleItem> items)
    {
        if (items == null || !items.Any())
            throw new InvalidOperationException("A sale must have at least one item.");

        _items.Clear();
        _items.AddRange(items);
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        Total = _items.Sum(i => i.LineTotal);
    }
}
