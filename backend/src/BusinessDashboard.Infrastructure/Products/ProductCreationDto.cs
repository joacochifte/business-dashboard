public class ProductCreationDto
{
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int InitialStock { get; init; }
    public string? Description { get; init; }
}
