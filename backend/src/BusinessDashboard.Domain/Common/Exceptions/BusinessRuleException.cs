namespace BusinessDashboard.Domain.Common.Exceptions;

/// <summary>
/// Represents a domain/business rule violation (client error).
/// </summary>
public sealed class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }

    public BusinessRuleException(string message, Exception? innerException) : base(message, innerException) { }
}

