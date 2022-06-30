namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OperationsController : ControllerBase
{
    private readonly IOperationsRepository _repository;
    private readonly IItemsRepository _itemsRepository;

    private readonly IOperationsCache _cache;

    private readonly UserManager<IdentityUser> _userManager;

    public OperationsController(
        IOperationsRepository repository, IItemsRepository itemsRepository,
        IOperationsCache cache, UserManager<IdentityUser> userManager)
    {
        _repository = repository;
        _itemsRepository = itemsRepository;

        _cache = cache;

        _userManager = userManager;
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AuthorizationPolicies.MustBeOperationAuthor)]
    public async Task<IActionResult> GetById(int id)
    {
        Operation? result;

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
    public async Task<IActionResult> PostIncome([FromBody] OperationDTO operationDTO)
    {
        if (ModelState.IsValid == false)
            return BadRequest(ModelState);

        Item? item = await TryGetBudgetItem(operationDTO.ItemId, OperationType.Income);
        if (item is null)
        {
            ModelState.AddModelError(string.Empty, $"There is no income with id: {operationDTO.ItemId}");
            return NotFound(ModelState);
        }

        return Ok(await PostOperation(operationDTO, OperationType.Income, item));
    }

    [HttpPost("expenses")]
    public async Task<IActionResult> PostExpense([FromBody] OperationDTO operationDTO)
    {
        if (ModelState.IsValid == false)
            return BadRequest(ModelState);

        Item? item = await TryGetBudgetItem(operationDTO.ItemId, OperationType.Expense);
        if (item is null)
        {
            ModelState.AddModelError(string.Empty, $"There is no expense with id: {operationDTO.ItemId}");
            return NotFound(ModelState);
        }

        return Ok(await PostOperation(operationDTO, OperationType.Expense, item));
    }

    [HttpPut("incomes/{id}")]
    [Authorize(Policy = AuthorizationPolicies.MustBeOperationAuthor)]
    public async Task<IActionResult> PutIncome(int id, [FromBody] OperationDTO operationDTO)
    {
        if (ModelState.IsValid == false)
            return BadRequest(ModelState);

        Item? item = await TryGetBudgetItem(operationDTO.ItemId, OperationType.Income);
        if (item is null)
        {
            ModelState.AddModelError(string.Empty, $"There is no income with id: {operationDTO.ItemId}");
            return NotFound(ModelState);
        }

        var result = await PutOperation(id, operationDTO, item);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("expenses/{id}")]
    [Authorize(Policy = AuthorizationPolicies.MustBeOperationAuthor)]
    public async Task<IActionResult> PutExpense(int id, [FromBody] OperationDTO operationDTO)
    {
        if (ModelState.IsValid == false)
            return BadRequest(ModelState);

        Item? item = await TryGetBudgetItem(operationDTO.ItemId, OperationType.Expense);
        if (item is null)
        {
            ModelState.AddModelError(string.Empty, $"There is no expense with id: {operationDTO.ItemId}");
            return NotFound(ModelState);
        }

        var result = await PutOperation(id, operationDTO, item);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthorizationPolicies.MustBeOperationAuthor)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _repository.DeleteAsync(id);
        if (result == false)
            return NotFound();

        _cache.RemoveOperation(id);
        return Ok();
    }

    private async Task<IEnumerable<Operation>> GetOperations(DateTime dateFrom, DateTime dateTo, OperationType? type = null)
    {
        var user = await _userManager.GetUserAsync(User);
        var userId = user.Id;
        var isAdmin = await _userManager.IsInRoleAsync(user, AuthorizationRoles.Admin);
        
        IEnumerable<Operation> result;

        result = _cache.GetOperationsCollection(userId, dateFrom, dateTo, type);
        if (result is not null)
            return result;

        OperationType[] types = type switch
        {
            OperationType.Income => new OperationType[] { OperationType.Income },
            OperationType.Expense => new OperationType[] { OperationType.Expense },
            _ => Enum.GetValues<OperationType>(),
        };
        result = await _repository.GetAllByTypesOverTimePeriodAsync(userId, isAdmin, types, dateFrom, dateTo);

        _cache.SetOperationsCollection(userId, result, dateFrom, dateTo, type);

        return result;
    }
    private async Task<Operation> PostOperation(OperationDTO operationDTO, OperationType type, Item item)
    {
        var result = await _repository.PostAsync(new Operation
        {
            Date = operationDTO.Date,
            Type = type,
            Sum = operationDTO.Sum,
            Item = item,
            AuthorId = _userManager.GetUserId(User)
        });

        _cache.SetOperation(result!);

        return result!;
    }
    private async Task<Operation?> PutOperation(int id, OperationDTO operationDTO, Item item)
    {
        operationDTO.Item = item;
        var result = await _repository.PutAsync(id, operationDTO);
        if (result is null)
            return result;

        _cache.RemoveOperation(id);
        _cache.SetOperation(result);

        return result;
    }
    private async Task<Item?> TryGetBudgetItem(int id, OperationType type)
    {
        var result = await _itemsRepository.GetByIdAsync(id);

        if (result is null || result.Type != type)
            return null;

        return result;
    }
}
