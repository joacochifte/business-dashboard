using BusinessDashboard.Domain.Common;
using BusinessDashboard.Domain.Customers;

namespace BusinessDashboard.Domain.Sales;

public class Sale : Entity
{
    private readonly List<SaleItem> _items = new();

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    public decimal Total { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public string? CustomerName {get; private set; } = "";
    public string? PaymentMethod { get; private set; }
    public bool IsDebt { get; private set; }

    private Sale() { }

    public Sale(IEnumerable<SaleItem> items, Guid? customerId = null, string? paymentMethod = null, bool isDebt = false)
    {
        if (items == null || !items.Any())
            throw new InvalidOperationException("A sale must have at least one item.");

        CustomerId = customerId;
        PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? null : paymentMethod.Trim();
        IsDebt = isDebt;

        foreach (var item in items)
        {
            AddItem(item);
        }
    }

    public void SetCustomer(Customer? customer)
    {
        Customer = customer;
        CustomerId = customer?.Id;
    }

    public void SetPaymentMethod(string? paymentMethod)
    {
        PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? null : paymentMethod.Trim();
    }

    public void SetIsDebt(bool isDebt)
    {
        IsDebt = isDebt;
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
