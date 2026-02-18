using BusinessDashboard.Application.Customers;
using BusinessDashboard.Infrastructure.Customers;
using Microsoft.AspNetCore.Mvc;

namespace Api.Test.Controllers;

[TestClass]
public class CustomersControllerTests
{
    private FakeCustomerService _service = null!;
    private CustomersController _controller = null!;

    [TestInitialize]
    public void SetUp()
    {
        _service = new FakeCustomerService();
        _controller = new CustomersController(_service);
    }

    [TestMethod]
    public async Task GetCustomers_ShouldReturnOkWithCustomers()
    {
        var expected = new List<CustomerDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Juan Pérez", Email = "juan@mail.com", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "María García", IsActive = true }
        };
        _service.GetAllResult = expected;

        var result = await _controller.GetCustomers();

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var value = ok.Value as IEnumerable<CustomerDto>;
        Assert.IsNotNull(value);
        Assert.AreEqual(2, value.Count());
    }

    [TestMethod]
    public async Task GetCustomerById_ShouldReturnOkWithCustomer()
    {
        var id = Guid.NewGuid();
        var expected = new CustomerDto { Id = id, Name = "Juan Pérez", Email = "juan@mail.com", IsActive = true };
        _service.GetByIdResult = expected;

        var result = await _controller.GetCustomerById(id);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        var value = ok.Value as CustomerDto;
        Assert.IsNotNull(value);
        Assert.AreEqual(id, value.Id);
    }

    [TestMethod]
    public async Task CreateCustomer_ShouldReturnCreatedAtAction()
    {
        var id = Guid.NewGuid();
        _service.CreateResult = id;
        var request = new CustomerCreationDto { Name = "Juan Pérez", Email = "juan@mail.com" };

        var result = await _controller.CreateCustomer(request);

        var created = result as CreatedAtActionResult;
        Assert.IsNotNull(created);
        Assert.AreEqual(nameof(CustomersController.GetCustomerById), created.ActionName);
        Assert.IsNotNull(created.RouteValues);
        Assert.AreEqual(id, created.RouteValues["id"]);
    }

    [TestMethod]
    public async Task UpdateCustomer_ShouldReturnOk()
    {
        var id = Guid.NewGuid();
        var request = new CustomerUpdateDto { Name = "Juan Pérez", IsActive = true };

        var result = await _controller.UpdateCustomer(id, request);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.AreEqual(id, _service.LastUpdatedId);
    }

    [TestMethod]
    public async Task DeleteCustomer_ShouldReturnOk()
    {
        var id = Guid.NewGuid();

        var result = await _controller.DeleteCustomer(id);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        Assert.AreEqual(id, _service.LastDeletedId);
    }

    private sealed class FakeCustomerService : ICustomerService
    {
        public IEnumerable<CustomerDto> GetAllResult { get; set; } = Array.Empty<CustomerDto>();
        public CustomerDto GetByIdResult { get; set; } = new();
        public Guid CreateResult { get; set; } = Guid.NewGuid();
        public Guid? LastUpdatedId { get; private set; }
        public Guid? LastDeletedId { get; private set; }

        public Task<Guid> CreateCustomerAsync(CustomerCreationDto request, CancellationToken ct = default)
            => Task.FromResult(CreateResult);

        public Task DeleteCustomerAsync(Guid customerId, CancellationToken ct = default)
        {
            LastDeletedId = customerId;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(CancellationToken ct = default)
            => Task.FromResult(GetAllResult);

        public Task<CustomerDto> GetCustomerByIdAsync(Guid customerId, CancellationToken ct = default)
            => Task.FromResult(GetByIdResult);

        public Task UpdateCustomerAsync(Guid customerId, CustomerUpdateDto request, CancellationToken ct = default)
        {
            LastUpdatedId = customerId;
            return Task.CompletedTask;
        }
    }
}
