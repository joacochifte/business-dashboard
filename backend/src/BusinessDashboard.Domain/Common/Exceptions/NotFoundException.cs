namespace BusinessDashboard.Domain.Common.Exceptions;

/// <summary>
/// Represents a missing resource (not found).
/// </summary>
public sealed class NotFoundException : Exception
{
    public string ResourceName { get; }
    public string? ResourceId { get; }

    public NotFoundException(string resourceName, string? resourceId = null)
        : base(resourceId is null ? $"{resourceName} was not found." : $"{resourceName} '{resourceId}' was not found.")
    {
        ResourceName = resourceName;
        ResourceId = resourceId;
    }
}

