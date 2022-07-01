namespace UnitTests;

public class ItemsControllerTests
{
    private const int ItemsCount = 10;

    private readonly Item[] _mockItems;
    private readonly Mock<IItemsRepository> _mockRepository;
    private readonly Mock<IItemsCache> _mockCache;

    private readonly ItemsController _itemsController;

    public ItemsControllerTests()
    {
        #region Mock Items

        _mockItems = new Item[ItemsCount];
        for (int i = 1; i <= ItemsCount; i++)
        {
            var currentIndex = i - 1;
            _mockItems[currentIndex] = new Item
            {
                Id = i,
                Name = $"Item {i}",
                Type = i % 2 == 0 ? OperationType.Income : OperationType.Expense
            };
        }

        #endregion

        #region Mock Items Repository

        _mockRepository = new Mock<IItemsRepository>();

        _mockRepository
                .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .Returns((int i) => Task.FromResult(i <= ItemsCount ? (Item?)_mockItems[i - 1] : null));

        _mockRepository
            .Setup(repo => repo.GetAllByTypesAsync(It.IsAny<OperationType[]>()))
            .Returns((OperationType[] types) => Task.FromResult(_mockItems.Where(i => types.Contains(i.Type)).AsEnumerable()));

        _mockRepository
            .Setup(repo => repo.PostAsync(It.IsAny<Item>()))
            .Returns((Item item) => Task.FromResult(new Item { Id = _mockItems.Length + 1, Name = item.Name, Type = item.Type }));

        _mockRepository
            .Setup(repo => repo.EqualExistsAsync(It.IsAny<OperationType>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns((OperationType type, string name, int id) => Task.FromResult(
                _mockItems
                    .Where(i => i.Id != id && i.Type == type && i.Name == name)
                    .Any()));

        _mockRepository
            .Setup(repo => repo.PutAsync(It.IsAny<int>(), It.IsAny<ItemDTO>()))
            .Returns((int id, ItemDTO dto) => Task.FromResult(Put_Mock(id, dto)));

        _mockRepository
            .Setup(repo => repo.DeleteAsync(It.IsAny<int>()))
            .Returns((int id) => Task.FromResult(Delete_Mock(id)));

        #endregion

        #region Mock Items Cache

        _mockCache = new Mock<IItemsCache>();
        _mockCache.Setup(cache => cache.GetItem(It.IsAny<int>())).Returns(() => null);
        _mockCache.Setup(cache => cache.GetItemsCollection(It.IsAny<OperationType?>())).Returns(() => null);

        #endregion

        _itemsController = new ItemsController(_mockRepository.Object, _mockCache.Object);
    }

    private Item? Put_Mock(int id, ItemDTO dto)
    {
        var item = _mockItems.Where(i => i.Id == id).FirstOrDefault();

        if (item is null)
            return item;

        item.Name = dto.Name;

        return item;
    }
    private bool Delete_Mock(int id)
    {
        var item = _mockItems.Where(i => i.Id == id).FirstOrDefault();

        if (item is null)
            return false;

        return true;
    }

    [Fact]
    public async void GetById_ReturnsOk()
    {
        for (int i = 1; i <= _mockItems.Length; i++)
        {
            var result = await _itemsController.GetById(i) as ObjectResult;

            Assert.NotNull(result);

            var statusCode = result!.StatusCode;
            var value = result!.Value as Item;

            Assert.Equal(StatusCodes.Status200OK, statusCode);
            Assert.Equal(_mockItems[i - 1], value);
        }
    }

    [Fact]
    public async void GetById_ReturnsStatusNotFound()
    {
        var result = await _itemsController.GetById(_mockItems.Count() + 1) as StatusCodeResult;

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
            result = await _itemsController.GetAll() as ObjectResult;
        else if (types.Contains(OperationType.Income))
            result = await _itemsController.GetAllIncomes() as ObjectResult;
        else if (types.Contains(OperationType.Expense))
            result = await _itemsController.GetAllExpenses() as ObjectResult;
        else
            throw new Exception();

        Assert.NotNull(result);

        var statusCode = result!.StatusCode;
        var values = result!.Value as IEnumerable<Item>;

        Assert.Equal(StatusCodes.Status200OK, statusCode);
        Assert.Equal(_mockItems.Where(i => types.Contains(i.Type)), values);
    }

    [Theory]
    [InlineData(OperationType.Income, "Test Income")]
    [InlineData(OperationType.Expense, "Test Expense")]
    public async void Post_ReturnsOk(OperationType type, string name)
    {
        var itemDTO = new ItemDTO { Name = name };
       
        ObjectResult? result = type switch
        {
            OperationType.Income => await _itemsController.PostIncome(itemDTO) as ObjectResult,
            OperationType.Expense => await _itemsController.PostExpense(itemDTO) as ObjectResult,
            _ => throw new Exception(),
        };
        
        Assert.NotNull(result);

        var statusCode = result!.StatusCode;
        var value = result!.Value as Item;

        Assert.Equal(StatusCodes.Status200OK, statusCode);
        Assert.Equal(_mockItems.Length + 1, value?.Id);
        Assert.Equal(name, value?.Name);
        Assert.Equal(type, value?.Type);
    }

    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async void Post_ReturnsBadRequest(OperationType type)
    {
        var name = _mockItems.Where(i => i.Type == type).Select(i => i.Name).First();
        var itemDTO = new ItemDTO { Name = name };

        ObjectResult? result = type switch
        {
            OperationType.Income => await _itemsController.PostIncome(itemDTO) as ObjectResult,
            OperationType.Expense => await _itemsController.PostExpense(itemDTO) as ObjectResult,
            _ => throw new Exception(),
        };

        Assert.NotNull(result);

        var statusCode = result!.StatusCode;
        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
    }

    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async void Put_ReturnsOk(OperationType type)
    {
        var item = _mockItems.Where(i => i.Type == type).First();
        var currentName = item.Name;

        var name = "Test";
        var itemDTO = new ItemDTO { Name = name };

        ObjectResult? result = type switch
        {
            OperationType.Income => await _itemsController.PutIncome(item.Id, itemDTO) as ObjectResult,
            OperationType.Expense => await _itemsController.PutExpense(item.Id, itemDTO) as ObjectResult,
            _ => throw new Exception(),
        };

        Assert.NotNull(result);

        var statusCode = result!.StatusCode;
        var value = result!.Value as Item;

        Assert.Equal(StatusCodes.Status200OK, statusCode);
        Assert.Equal(item.Id, value?.Id);
        Assert.Equal(name, value?.Name);
        Assert.Equal(type, value?.Type);

        itemDTO.Name = currentName;
        Put_Mock(item.Id, itemDTO);
    }

    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async void Put_ReturnsBadRequest(OperationType type)
    {
        var item = _mockItems.Where(i => i.Type == type).First();
        var name = _mockItems.Where(i => i.Type == type && i.Id != item.Id).Select(i => i.Name).First();

        var itemDTO = new ItemDTO { Name = name };

        ObjectResult? result = type switch
        {
            OperationType.Income => await _itemsController.PutIncome(item.Id, itemDTO) as ObjectResult,
            OperationType.Expense => await _itemsController.PutExpense(item.Id, itemDTO) as ObjectResult,
            _ => throw new Exception(),
        };

        Assert.NotNull(result);

        var statusCode = result!.StatusCode;

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
    }

    [Theory]
    [InlineData(OperationType.Income)]
    [InlineData(OperationType.Expense)]
    public async void Put_ReturnsNotFound(OperationType type)
    {
        var id = _mockItems.Length + 1;
        var itemDTO = new ItemDTO { Name = "Test" };

        StatusCodeResult? result = type switch
        {
            OperationType.Income => await _itemsController.PutIncome(id, itemDTO) as StatusCodeResult,
            OperationType.Expense => await _itemsController.PutExpense(id, itemDTO) as StatusCodeResult,
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
        var item = _mockItems.Where(i => i.Type == type).First();
        var result = await _itemsController.Delete(item.Id) as StatusCodeResult;

        Assert.NotNull(result);

        var statusCode = result!.StatusCode;

        Assert.Equal(StatusCodes.Status200OK, statusCode);
    }

    [Fact]
    public async void Delete_ReturnsNotFound()
    {
        var id = _mockItems.Length + 1;
        var result = await _itemsController.Delete(id) as StatusCodeResult;

        Assert.NotNull(result);

        var statusCode = result!.StatusCode;

        Assert.Equal(StatusCodes.Status404NotFound, statusCode);
    }
}