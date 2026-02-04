namespace BusinessDashboard.Application.Sales;
using BusinessDashboard.Infrastructure.Sales;

public interface ISalesService
{
    Task<Guid> CreateSaleAsync(SaleCreationDto request, CancellationToken ct = default);
    Task DeleteSaleAsync(Guid saleId, CancellationToken ct = default);
    Task<IEnumerable<SaleDto>> GetAllSalesAsync(CancellationToken ct = default);
    Task<SaleDto> GetSaleByIdAsync(Guid saleId,  CancellationToken ct = default);
    Task UpdateSaleAsync(Guid saleId, SaleUpdateDto request, CancellationToken ct = default);
}