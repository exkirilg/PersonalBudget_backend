using Microsoft.AspNetCore.Identity;
using Server.Authorization;
using System.Security.Claims;

namespace UnitTests;

public class OperationsControllerTests
{
    private const int OperationsCount = 10;

    private readonly Operation[] _mockOperations;
    private readonly Mock<IOperationsRepository> _mockRepository;
    private readonly Mock<IItemsRepository> _mockItemsRepository;
    private readonly Mock<IOperationsCache> _mockCache;
    private readonly Mock<IAuthorizationServices> _mockAuthorizationServices;

    private readonly OperationsController _operationsController;

    public OperationsControllerTests()
    {
        #region Mock Operations

        var currentMonth = CurrentMonthBegining();

        _mockOperations = new Operation[OperationsCount];
        for (int i = 1; i <= OperationsCount; i++)
        {
            var currentIndex = i - 1;
            var type = i % 2 == 0 ? OperationType.Income : OperationType.Expense;
            _mockOperations[currentIndex] = new Operation
            {
                Id = i,
                Type = type,
                Date = currentMonth.AddDays(i),
                Sum = i * 1000,
                Item = new Item { Id = type == OperationType.Income ? 2 : 1, Name = "Test item", Type = type }
            };
        }

        #endregion

        #region Mock Operations Repository

        _mockRepository = new Mock<IOperationsRepository>();

        _mockRepository
                .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .Returns((int i) => Task.FromResult(i <= OperationsCount ? (Operation?)_mockOperations[i - 1] : null));

        _mockRepository
            .Setup(repo => repo.GetAllByTypesOverTimePeriodAsync("", true, It.IsAny<IEnumerable<OperationType>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(
                (string userId, bool isAdmin, IEnumerable<OperationType> types, DateTime dateFrom, DateTime dateTo)
                => Task.FromResult(_mockOperations.Where(o => types.Contains(o.Type) && o.Date >= dateFrom && o.Date <= dateTo).AsEnumerable()));

        _mockRepository
            .Setup(repo => repo.PostAsync(It.IsAny<Operation>()))
            .Returns((Operation operation) => Task.FromResult(new Operation { Id = _mockOperations.Length + 1, Type = operation.Type, Date = operation.Date, Item = operation.Item, Sum = operation.Sum }));

        _mockRepository
            .Setup(repo => repo.PutAsync(It.IsAny<int>(), It.IsAny<OperationDTO>()))
            .Returns((int id, OperationDTO dto) => Task.FromResult(Put_Mock(id, dto)));

        _mockRepository
            .Setup(repo => repo.DeleteAsync(It.IsAny<int>()))
            .Returns((int id) => Task.FromResult(Delete_Mock(id)));

        #endregion

        #region Mock Items Repository

        _mockItemsRepository = new Mock<IItemsRepository>();

        _mockItemsRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .Returns((int i) => Task.FromResult(i == 0 ? null : new Item { Id = i, Name = "Test item", Type = i == 1 ? OperationType.Expense : OperationType.Income }));

        #endregion

        #region Mock Operations Cache

        _mockCache = new Mock<IOperationsCache>();
        _mockCache.Setup(cache => cache.GetOperation(It.IsAny<int>())).Returns(() => null);
        _mockCache.Setup(cache => cache.GetOperationsCollection(
            It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<OperationType?>())).Returns(() => null);

        #endregion

        #region Mock User Manager

        _mockAuthorizationServices = new Mock<IAuthorizationServices>();

        _mockAuthorizationServices
            .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .Returns(() => Task.FromResult((IdentityUser?)new IdentityUser("Test user")));
        _mockAuthorizationServices
            .Setup(manager => manager.GetUserIdAsync(It.IsAny<ClaimsPrincipal>()))
            .Returns(() => Task.FromResult((string?)string.Empty));
        _mockAuthorizationServices
            .Setup(manager => manager.IsInRoleAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .Returns(() => Task.FromResult(true));

        #endregion
        
        _operationsController = new OperationsController(_mockRepository.Object, _mockItemsRepository.Object, _mockCache.Object, _mockAuthorizationServices.Object);
    }

    private static DateTime CurrentMonthBegining()
    {
        var currentDate = DateTime.UtcNow;
        return new DateTime(currentDate.Year, currentDate.Month, 1);
    }
    private static DateTime CurrentMonthEnd()
    {
        var currentDate = DateTime.UtcNow;
        var nextMonth = new DateTime(currentDate.Year, currentDate.Month + 1, 1);
        return nextMonth.Subtract(new TimeSpan(hours: 0, minutes: 0, seconds: 1));
    }
    private Operation? Put_Mock(int id, OperationDTO dto)
    {
        var operation = _mockOperations.Where(i => i.Id == id).FirstOrDefault();

        if (operation is null)
            return operation;

        operation.Date = dto.Date;
        operation.Item = dto.Item;
        operation.Sum = dto.Sum;

        return operation;
    }
    private bool Delete_Mock(int id)
    {
        var operation = _mockOperations.Where(o => o.Id == id).FirstOrDefault();

        if (operation is null)
            return false;

        return true;
    }

    [Fact]
    public async void GetById_ReturnsOk()
    {
        for (int i = 1; i <= _mockOperations.Length; i++)
        {
            var result = await _operationsController.GetById(i) as ObjectResult;

            Assert.NotNull(result);

            var statusCode = result!.StatusCode;
            var value = result!.Value as Operation;

            Assert.Equal(StatusCodes.Status200OK, statusCode);
            Assert.Equal(_mockOperations[i - 1], value);
        }
    }

    [Fact]
    public async void GetById_ReturnsStatusNotFound()
    {
        var result = await _operationsController.GetById(_mockOperations.Length + 1) as StatusCodeResult;

        Assert.NotNull(result);

        var statusCode = result!.StatusCode;
        Assert.Equal(StatusCodes.Status404NotFound, statusCode);
    }

    [Theory]
    [InlineData(new OperationType[] { OperationType.Income, OperationType.Expense })]
    [InlineData(new OperationType[] { OperationType.Income })]
    [InlineData(new OperationType[] { OperationType.Expense })]
    public async void GetAll_ReturnsOk(OperationType[] types)
    {
        ObjectResult? result;
        if (types.Contains(OperationType.Income) && types.Contains(OperationType.Expense))
            result = await _operationsController.GetAllOverTimePeriod(CurrentMonthBegining(), CurrentMonthEnd()) as ObjectResult;
        else if (types.Contains(OperationType.Income))
            result = await _operationsController.GetAllIncomes(CurrentMonthBegining(), CurrentMonthEnd()) as ObjectResult;
        else if (types.Contains(OperationType.Expense))
            result = await _operationsController.GetAllExpenses(CurrentMonthBegining(), CurrentMonthEnd()) as ObjectResult;
        else
            throw new Exception();

        Assert.NotNull(result);

        var statusCode = result!.StatusCode;
        var values = result!.Value as IEnumerable<Operation>;

        Assert.Equal(StatusCodes.Status200OK, statusCode);
        Assert.Equal(_mockOperations.Where(o => types.Contains(o.Type) && o.Date >= CurrentMonthBegining() && o.Date <= CurrentMonthEnd()), values);
    }

    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async void Post_ReturnsOk(OperationType type)
    {
        var date = DateTime.UtcNow;
        var itemId = type == OperationType.Income ? 2 : 1;
        var sum = 100;

        var operationDTO = new OperationDTO { Date = date, ItemId = itemId, Sum = sum };

        ObjectResult? result = type switch
        {
            OperationType.Income => await _operationsController.PostIncome(operationDTO) as ObjectResult,
            OperationType.Expense => await _operationsController.PostExpense(operationDTO) as ObjectResult,
            _ => throw new Exception(),
        };

        Assert.NotNull(result);

        var statusCode = result!.StatusCode;
        var value = result!.Value as Operation;

        Assert.Equal(StatusCodes.Status200OK, statusCode);
        Assert.Equal(_mockOperations.Length + 1, value?.Id);
        Assert.Equal(type, value?.Type);
        Assert.Equal(date, value?.Date);
        Assert.Equal(itemId, value?.Item?.Id);
        Assert.Equal(sum, value?.Sum);
    }

    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async void Post_ReturnsNotFound(OperationType type)
    {
        var operationDTO = new OperationDTO { Date = DateTime.UtcNow, ItemId = 0, Sum = 100 };

        ObjectResult? result = type switch
        {
            OperationType.Income => await _operationsController.PostIncome(operationDTO) as ObjectResult,
            OperationType.Expense => await _operationsController.PostExpense(operationDTO) as ObjectResult,
            _ => throw new Exception(),
        };

        Assert.NotNull(result);

        var statusCode = result!.StatusCode;

        Assert.Equal(StatusCodes.Status404NotFound, statusCode);
    }

    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async void Put_ReturnsOk(OperationType type)
    {
        var operation = _mockOperations.Where(o => o.Type == type).First();

        var currentDate = operation.Date;
        var currentItemId = operation.Item!.Id;
        var currentSum = operation.Sum;

        var date = DateTime.UtcNow;
        var itemId = type == OperationType.Income ? 2 : 1;
        var sum = 100;

        var operationDTO = new OperationDTO { Date = date, ItemId = itemId, Sum = sum };

        ObjectResult? result = type switch
        {
            OperationType.Income => await _operationsController.PutIncome(operation.Id, operationDTO) as ObjectResult,
            OperationType.Expense => await _operationsController.PutExpense(operation.Id, operationDTO) as ObjectResult,
            _ => throw new Exception(),
        };

        Assert.NotNull(result);

        var statusCode = result!.StatusCode;
        var value = result!.Value as Operation;

        Assert.Equal(StatusCodes.Status200OK, statusCode);
        Assert.Equal(operation.Id, value?.Id);
        Assert.Equal(type, value?.Type);
        Assert.Equal(date, value?.Date);
        Assert.Equal(itemId, value?.Item?.Id);
        Assert.Equal(sum, value?.Sum);

        operationDTO.Date = currentDate;
        operationDTO.ItemId = currentItemId;
        operationDTO.Sum = currentSum;
        Put_Mock(operation.Id, operationDTO);
    }

    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async void Put_OperationNotFound_ReturnsNotFound(OperationType type)
    {
        var date = DateTime.UtcNow;
        var itemId = type == OperationType.Income ? 2 : 1;
        var sum = 100;

        var operationDTO = new OperationDTO { Date = date, ItemId = itemId, Sum = sum };

        StatusCodeResult? result = type switch
        {
            OperationType.Income => await _operationsController.PutIncome(_mockOperations.Length + 1, operationDTO) as StatusCodeResult,
            OperationType.Expense => await _operationsController.PutExpense(_mockOperations.Length + 1, operationDTO) as StatusCodeResult,
            _ => throw new Exception(),
        };

        Assert.NotNull(result);

        var statusCode = result!.StatusCode;

        Assert.Equal(StatusCodes.Status404NotFound, statusCode);
    }

    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async void Put_ItemNotFound_ReturnsNotFound(OperationType type)
    {
        var operation = _mockOperations.Where(o => o.Type == type).First();

        var date = DateTime.UtcNow;
        var itemId = 0;
        var sum = 100;

        var operationDTO = new OperationDTO { Date = date, ItemId = itemId, Sum = sum };

        ObjectResult? result = type switch
        {
            OperationType.Income => await _operationsController.PutIncome(operation.Id, operationDTO) as ObjectResult,
            OperationType.Expense => await _operationsController.PutExpense(operation.Id, operationDTO) as ObjectResult,
            _ => throw new Exception(),
        };

        Assert.NotNull(result);

        var statusCode = result!.StatusCode;

        Assert.Equal(StatusCodes.Status404NotFound, statusCode);
    }

    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async void Delete_ReturnsOk(OperationType type)
    {
        var operation = _mockOperations.Where(o => o.Type == type).First();
        var result = await _operationsController.Delete(operation.Id) as StatusCodeResult;

        Assert.NotNull(result);

        var statusCode = result!.StatusCode;

        Assert.Equal(StatusCodes.Status200OK, statusCode);
    }

    [Fact]
    public async void Delete_ReturnsNotFound()
    {
        var id = _mockOperations.Length + 1;
        var result = await _operationsController.Delete(id) as StatusCodeResult;

        Assert.NotNull(result);

        var statusCode = result!.StatusCode;

        Assert.Equal(StatusCodes.Status404NotFound, statusCode);
    }
}
