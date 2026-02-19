using BusinessDashboard.Domain.Common;

namespace BusinessDashboard.Domain.Customers;

public class Customer : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public DateTime? BirthDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime LastPurchaseDate { get; private set; } = DateTime.UtcNow;
    public int TotalPurchases { get; private set; } = 0;
    public decimal TotalLifetimeValue { get; private set; } = 0;

    private Customer() { }

    public Customer(string name, string? email = null, string? phone = null, DateTime? birthDate = null)
    {
        SetName(name);
        SetEmail(email);
        SetPhone(phone);
        BirthDate = birthDate;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name is required.");
        Name = name.Trim();
    }

    public void SetEmail(string? email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            email = email.Trim();
            if (!IsValidEmail(email))
                throw new ArgumentException("Invalid email format.");
        }
        Email = string.IsNullOrWhiteSpace(email) ? null : email;
    }

    public void SetPhone(string? phone)
    {
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
    }

    public void SetBirthDate(DateTime? birthDate)
    {
        BirthDate = birthDate;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public void UpdateLastPurchaseDate(DateTime purchaseDate, decimal purchaseAmount)
    {
        LastPurchaseDate = purchaseDate;
        TotalPurchases += 1;
        TotalLifetimeValue += purchaseAmount;
    }

    public void RemovePurchase(decimal purchaseAmount)
    {
        TotalPurchases = Math.Max(0, TotalPurchases - 1);
        TotalLifetimeValue = Math.Max(0, TotalLifetimeValue - purchaseAmount);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
