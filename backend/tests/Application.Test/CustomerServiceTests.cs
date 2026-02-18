using BusinessDashboard.Application.Customers;
using BusinessDashboard.Domain.Customers;
using BusinessDashboard.Domain.Common.Exceptions;
using BusinessDashboard.Infrastructure.Customers;
using BusinessDashboard.Infrastructure.Repositories.Interfaces;
using Moq;

namespace Application.Test.Customers;

[TestClass]
public class CustomerServiceTests
{
    private Mock<ICustomerRepository> _repo = null!;
    private CustomerService _service = null!;

    [TestInitialize]
    public void SetUp()
    {
        _repo = new Mock<ICustomerRepository>(MockBehavior.Strict);
        _service = new CustomerService(_repo.Object);
    }

    [TestMethod]
    public async Task CreateCustomerAsync_ShouldCreateAndReturnId()
    {
        var request = new CustomerCreationDto
        {
            Name = "Juan Pérez",
            Email = "juan@mail.com",
            Phone = "1234567890",
            BirthDate = new DateTime(1990, 5, 15)
        };

        _repo.Setup(r => r.GetByEmailAsync(request.Email!, It.IsAny<CancellationToken>())).ReturnsAsync((Customer?)null);

        Customer? captured = null;
        _repo
            .Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .Callback<Customer, CancellationToken>((c, _) => captured = c)
            .Returns(Task.CompletedTask);

        var id = await _service.CreateCustomerAsync(request);

        Assert.IsNotNull(captured);
        Assert.AreEqual(request.Name, captured.Name);
        Assert.AreEqual(request.Email, captured.Email);
        Assert.AreEqual(request.Phone, captured.Phone);
        Assert.AreEqual(request.BirthDate, captured.BirthDate);
        Assert.AreEqual(captured.Id, id);

        _repo.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task GetAllCustomersAsync_ShouldMapToDto()
    {
        var customers = new List<Customer>
        {
            new Customer(name: "Juan Pérez", email: "juan@mail.com"),
            new Customer(name: "María García")
        };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(customers);

        var result = (await _service.GetAllCustomersAsync()).ToList();

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Juan Pérez", result[0].Name);
        Assert.AreEqual("juan@mail.com", result[0].Email);
        Assert.AreEqual("María García", result[1].Name);

        _repo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task GetCustomerByIdAsync_ShouldMapToDto()
    {
        var id = Guid.NewGuid();
        var customer = new Customer(name: "Juan Pérez", email: "juan@mail.com", phone: "123");

        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var dto = await _service.GetCustomerByIdAsync(id);

        Assert.AreEqual(customer.Name, dto.Name);
        Assert.AreEqual(customer.Email, dto.Email);
        Assert.AreEqual(customer.Phone, dto.Phone);
        Assert.AreEqual(customer.Id, dto.Id);

        _repo.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task DeleteCustomerAsync_ShouldCallRepository()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _service.DeleteCustomerAsync(id);

        _repo.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task UpdateCustomerAsync_ShouldUpdateAndPersist()
    {
        var id = Guid.NewGuid();
        var customer = new Customer(name: "Nombre Viejo", email: "viejo@mail.com");
        var request = new CustomerUpdateDto
        {
            Name = "Nombre Nuevo",
            Email = "nuevo@mail.com",
            Phone = "9999999",
            BirthDate = new DateTime(1995, 6, 10),
            IsActive = true
        };

        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _repo.Setup(r => r.GetByEmailAsync(request.Email!, It.IsAny<CancellationToken>())).ReturnsAsync((Customer?)null);
        _repo.Setup(r => r.UpdateAsync(customer, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _service.UpdateCustomerAsync(id, request);

        Assert.AreEqual("Nombre Nuevo", customer.Name);
        Assert.AreEqual("nuevo@mail.com", customer.Email);
        Assert.AreEqual("9999999", customer.Phone);
        Assert.AreEqual(new DateTime(1995, 6, 10), customer.BirthDate);
        Assert.IsTrue(customer.IsActive);

        _repo.Verify(r => r.UpdateAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task UpdateCustomerAsync_WithIsActiveFalse_ShouldDeactivate()
    {
        var id = Guid.NewGuid();
        var customer = new Customer(name: "Juan");
        var request = new CustomerUpdateDto { Name = "Juan", IsActive = false };

        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _repo.Setup(r => r.UpdateAsync(customer, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _service.UpdateCustomerAsync(id, request);

        Assert.IsFalse(customer.IsActive);
    }

    [TestMethod]
    [ExpectedException(typeof(BusinessRuleException))]
    public async Task CreateCustomerAsync_WithDuplicateEmail_ShouldThrowBusinessRuleException()
    {
        var request = new CustomerCreationDto { Name = "Juan", Email = "juan@mail.com" };
        var existing = new Customer(name: "Otro", email: "juan@mail.com");

        _repo.Setup(r => r.GetByEmailAsync("juan@mail.com", It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        await _service.CreateCustomerAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(BusinessRuleException))]
    public async Task UpdateCustomerAsync_WithDuplicateEmail_ShouldThrowBusinessRuleException()
    {
        var id = Guid.NewGuid();
        var customer = new Customer(name: "Juan", email: "viejo@mail.com");
        var request = new CustomerUpdateDto { Name = "Juan", Email = "duplicado@mail.com", IsActive = true };
        var existing = new Customer(name: "Otro", email: "duplicado@mail.com");

        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _repo.Setup(r => r.GetByEmailAsync("duplicado@mail.com", It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        await _service.UpdateCustomerAsync(id, request);
    }

    [TestMethod]
    public async Task UpdateCustomerAsync_WithSameEmail_ShouldNotThrow()
    {
        var id = Guid.NewGuid();
        var customer = new Customer(name: "Juan", email: "juan@mail.com");
        var request = new CustomerUpdateDto { Name = "Juan Actualizado", Email = "juan@mail.com", IsActive = true };

        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _repo.Setup(r => r.UpdateAsync(customer, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _service.UpdateCustomerAsync(id, request);

        Assert.AreEqual("Juan Actualizado", customer.Name);
    }
}

