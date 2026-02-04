using BusinessDashboard.Application.Dashboard;
using BusinessDashboard.Infrastructure.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace Api.Test.Controllers;

[TestClass]
public class DashboardControllerTests
{
    private FakeDashboardService _service = null!;
    private DashboardController _controller = null!;

    [TestInitialize]
    public void SetUp()
    {
        _service = new FakeDashboardService();
        _controller = new DashboardController(_service);
    }

    [TestMethod]
    public async Task GetSummary_ShouldReturnOk()
    {
        _service.SummaryResult = new DashboardSummaryDto
        {
            RevenueTotal = 100m,
            SalesCount = 2,
            AvgTicket = 50m
        };

        var result = await _controller.GetSummary(from: null, to: null, ct: CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);

        var dto = ok.Value as DashboardSummaryDto;
        Assert.IsNotNull(dto);
        Assert.AreEqual(100m, dto.RevenueTotal);
        Assert.AreEqual(2, dto.SalesCount);
    }

    [TestMethod]
    public async Task GetSummary_WithFromGreaterThanTo_ShouldReturnBadRequest()
    {
        var result = await _controller.GetSummary(
            from: new DateTime(2026, 02, 04),
            to: new DateTime(2026, 02, 01),
            ct: CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task GetSalesByPeriod_ShouldReturnOk()
    {
        _service.SalesByPeriodResult = new SalesByPeriodDto
        {
            GroupBy = "day",
            Points = new List<SalesByPeriodPointDto>
            {
                new() { PeriodStart = new DateTime(2026, 02, 01), SalesCount = 1, Revenue = 100m }
            }
        };

        var result = await _controller.GetSalesByPeriod(groupBy: "day", from: null, to: null, ct: CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);

        var dto = ok.Value as SalesByPeriodDto;
        Assert.IsNotNull(dto);
        Assert.AreEqual("day", dto.GroupBy);
        Assert.AreEqual(1, dto.Points.Count);
    }

    [TestMethod]
    public async Task GetSalesByPeriod_WithInvalidGroupBy_ShouldReturnBadRequest()
    {
        _service.ThrowInvalidGroupBy = true;

        var result = await _controller.GetSalesByPeriod(groupBy: "year", from: null, to: null, ct: CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task GetSalesByPeriod_WithFromGreaterThanTo_ShouldReturnBadRequest()
    {
        var result = await _controller.GetSalesByPeriod(
            groupBy: "day",
            from: new DateTime(2026, 02, 04),
            to: new DateTime(2026, 02, 01),
            ct: CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task GetTopProducts_ShouldReturnOk()
    {
        _service.TopProductsResult = new List<TopProductDto>
        {
            new() { ProductId = Guid.NewGuid(), ProductName = "A", Quantity = 3, Revenue = 300m }
        };

        var result = await _controller.GetTopProducts(limit: 10, from: null, to: null, ct: CancellationToken.None);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);

        var dto = ok.Value as IReadOnlyList<TopProductDto>;
        Assert.IsNotNull(dto);
        Assert.AreEqual(1, dto.Count);
        Assert.AreEqual("A", dto[0].ProductName);
    }

    [TestMethod]
    public async Task GetTopProducts_WithFromGreaterThanTo_ShouldReturnBadRequest()
    {
        var result = await _controller.GetTopProducts(
            limit: 10,
            from: new DateTime(2026, 02, 04),
            to: new DateTime(2026, 02, 01),
            ct: CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task Endpoints_ShouldPassParametersToService()
    {
        var from = new DateTime(2026, 02, 01);
        var to = new DateTime(2026, 02, 04);

        _service.SummaryResult = new DashboardSummaryDto();
        _service.SalesByPeriodResult = new SalesByPeriodDto();
        _service.TopProductsResult = new List<TopProductDto>();

        _ = await _controller.GetSummary(from, to, CancellationToken.None);
        Assert.AreEqual(from, _service.LastFrom);
        Assert.AreEqual(to, _service.LastTo);

        _ = await _controller.GetSalesByPeriod("week", from, to, CancellationToken.None);
        Assert.AreEqual("week", _service.LastGroupBy);

        _ = await _controller.GetTopProducts(7, from, to, CancellationToken.None);
        Assert.AreEqual(7, _service.LastLimit);
    }

    private sealed class FakeDashboardService : IDashboardService
    {
        public DashboardSummaryDto SummaryResult { get; set; } = new();
        public SalesByPeriodDto SalesByPeriodResult { get; set; } = new();
        public IReadOnlyList<TopProductDto> TopProductsResult { get; set; } = Array.Empty<TopProductDto>();
        public bool ThrowInvalidGroupBy { get; set; }

        public DateTime? LastFrom { get; private set; }
        public DateTime? LastTo { get; private set; }
        public string? LastGroupBy { get; private set; }
        public int? LastLimit { get; private set; }

        public Task<DashboardSummaryDto> GetSummaryAsync(DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
        {
            LastFrom = from;
            LastTo = to;
            return Task.FromResult(SummaryResult);
        }

        public Task<SalesByPeriodDto> GetSalesByPeriodAsync(string groupBy, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
        {
            if (ThrowInvalidGroupBy)
                throw new ArgumentException("groupBy must be 'day', 'week', or 'month'.", nameof(groupBy));

            LastGroupBy = groupBy;
            LastFrom = from;
            LastTo = to;
            return Task.FromResult(SalesByPeriodResult);
        }

        public Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(int limit = 10, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
        {
            LastLimit = limit;
            LastFrom = from;
            LastTo = to;
            return Task.FromResult(TopProductsResult);
        }
    }
}

