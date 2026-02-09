using BusinessDashboard.Domain.Common;

namespace BusinessDashboard.Domain.Costs;

public class Cost : Entity
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DateIncurred { get; set; }
    public string? Description { get; set; }

    private Cost() { }

    public Cost(string name, decimal amount, DateTime dateIncurred, string? description = null)
    {
        SetName(name);
        SetAmount(amount);
        DateIncurred = dateIncurred;
        Description = description;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Cost name is required.");

        Name = name.Trim();
    }

    public void SetAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than 0.");

        Amount = amount;
    }
}

