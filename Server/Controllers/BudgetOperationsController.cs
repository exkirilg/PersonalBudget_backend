namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BudgetOperationsController : ControllerBase
{
    private readonly IBudgetOperationsRepository _repository;
    private readonly IBudgetItemsRepository _itemsRepository;

    private readonly IBudgetOperationsCache _cache;
    private readonly IBudgetItemsCache _itemsCache;

    public BudgetOperationsController(
        IBudgetOperationsRepository repository, IBudgetItemsRepository itemsRepository,
        IBudgetOperationsCache cache, IBudgetItemsCache itemsCache)
    {
        _repository = repository;
        _itemsRepository = itemsRepository;

        _cache = cache;
        _itemsCache = itemsCache;
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AuthorizationPolicies.MustBeOperationAuthor)]
    public async Task<IActionResult> GetById(int id)
    {
        IBudgetOperation? result;

        result = _cache.GetOperation(id);
        if (result is not null)
            return Ok(result);

        result = await _repository.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        _cache.SetOperation(result);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOverTimePeriod([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo)
    {
        return Ok(await GetOperations(dateFrom, dateTo));
    }

    [HttpGet("incomes")]
    public async Task<IActionResult> GetAllIncomes([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo)
    {
        return Ok(await GetOperations(dateFrom, dateTo, OperationType.Income));
    }

    [HttpGet("expenses")]
    public async Task<IActionResult> GetAllExpenses([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo)
    {
        return Ok(await GetOperations(dateFrom, dateTo, OperationType.Expense));
    }

    [HttpPost("incomes")]
    public async Task<IActionResult> PostIncome([FromBody] BudgetOperationDTO operationDTO)
    {
        if (ModelState.IsValid == false)
            return BadRequest(ModelState);

        IBudgetItem? item = await TryGetBudgetItem(operationDTO.ItemId, OperationType.Income);
        if (item is null)
        {
            ModelState.AddModelError(string.Empty, $"There is no income with id: {operationDTO.ItemId}");
            return NotFound(ModelState);
        }

        return Ok(await PostOperation(operationDTO, OperationType.Income, item));
    }

    [HttpPost("expenses")]
    public async Task<IActionResult> PostExpense([FromBody] BudgetOperationDTO operationDTO)
    {
        if (ModelState.IsValid == false)
            return BadRequest(ModelState);

        IBudgetItem? item = await TryGetBudgetItem(operationDTO.ItemId, OperationType.Expense);
        if (item is null)
        {
            ModelState.AddModelError(string.Empty, $"There is no expense with id: {operationDTO.ItemId}");
            return NotFound(ModelState);
        }

        return Ok(await PostOperation(operationDTO, OperationType.Expense, item));
    }

    [HttpPut("incomes/{id}")]
    public async Task<IActionResult> PutIncome(int id, [FromBody] BudgetOperationDTO operationDTO)
    {
        if (ModelState.IsValid == false)
            return BadRequest(ModelState);

        IBudgetItem? item = await TryGetBudgetItem(operationDTO.ItemId, OperationType.Income);
        if (item is null)
        {
            ModelState.AddModelError(string.Empty, $"There is no income with id: {operationDTO.ItemId}");
            return NotFound(ModelState);
        }

        var result = await PutOperation(id, operationDTO, OperationType.Income, item);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("expenses/{id}")]
    public async Task<IActionResult> PutExpense(int id, [FromBody] BudgetOperationDTO operationDTO)
    {
        if (ModelState.IsValid == false)
            return BadRequest(ModelState);

        IBudgetItem? item = await TryGetBudgetItem(operationDTO.ItemId, OperationType.Expense);
        if (item is null)
        {
            ModelState.AddModelError(string.Empty, $"There is no expense with id: {operationDTO.ItemId}");
            return NotFound(ModelState);
        }

        var result = await PutOperation(id, operationDTO, OperationType.Expense, item);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _repository.DeleteAsync(id);
        if (result == false)
            return NotFound();

        _cache.RemoveOperation(id);
        return Ok();
    }

    private async Task<IEnumerable<IBudgetOperation>> GetOperations(DateTime dateFrom, DateTime dateTo, OperationType? type = null)
    {
        IEnumerable<IBudgetOperation> result;

        result = _cache.GetOperationsCollection(dateFrom, dateTo, type);
        if (result is not null)
            return result;

        OperationType[] types = type switch
        {
            OperationType.Income => new OperationType[] { OperationType.Income },
            OperationType.Expense => new OperationType[] { OperationType.Expense },
            _ => Enum.GetValues<OperationType>(),
        };
        result = await _repository.GetAllOverTimePeriodAsync(types, dateFrom, dateTo);

        _cache.SetOperationsCollection(result, dateFrom, dateTo, type);

        return result;
    }
    private async Task<IBudgetOperation> PostOperation(BudgetOperationDTO operationDTO, OperationType type, IBudgetItem item)
    {
        var result = await _repository.PostAsync(new BudgetOperation
        {
            Date = operationDTO.Date,
            Type = type,
            Sum = operationDTO.Sum,
            Item = item
        });

        _cache.SetOperation(result!);

        return result!;
    }
    private async Task<IBudgetOperation?> PutOperation(int id, BudgetOperationDTO operationDTO, OperationType type, IBudgetItem item)
    {
        var result = await _repository.PutAsync(id, new BudgetOperation
        {
            Date = operationDTO.Date,
            Type = type,
            Sum = operationDTO.Sum,
            Item = item
        });
        if (result is null)
            return result;

        _cache.RemoveOperation(id);
        _cache.SetOperation(result);

        return result;
    }
    private async Task<IBudgetItem?> TryGetBudgetItem(int id, OperationType type)
    {
        IBudgetItem? result;

        result = _itemsCache.GetItem(id);
        if (result is null)
            result = await _itemsRepository.GetByIdAsync(id);

        if (result is null || result.Type != type)
            return null;

        _itemsCache.SetItem(result);
        return result;
    }
}
