using Domain.DTO;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Interfaces.Cache;
using Domain.Interfaces.DataAccess;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BudgetItemsController : ControllerBase
{
    private readonly IBudgetItemsRepository _repository;
    private readonly IBudgetItemsCache _cache;

    public BudgetItemsController(IBudgetItemsRepository repository, IBudgetItemsCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        IBudgetItem? result;

        result = _cache.GetItem(id);

        if (result is not null)
            return Ok(result);

        result = await _repository.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        _cache.SetItem(result);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await GetItems());
    }

    [HttpGet("incomes")]
    public async Task<IActionResult> GetAllIncomes()
    {
        return Ok(await GetItems(OperationType.Income));
    }

    [HttpGet("expenses")]
    public async Task<IActionResult> GetAllExpenses()
    {
        return Ok(await GetItems(OperationType.Expense));
    }

    [HttpPost("incomes")]
    public async Task<IActionResult> PostIncome([FromBody] BudgetItemDTO itemDTO)
    {
        if (ModelState.IsValid == false)
        {
            return BadRequest(ModelState);
        }
        else if (await _repository.EqualExistsAsync(OperationType.Income, itemDTO.Name))
        {
            ModelState.AddModelError(string.Empty, "Income with same name already exists");
            return BadRequest(ModelState);
        }

        return Ok(await PostItem(itemDTO, OperationType.Income));
    }

    [HttpPost("expenses")]
    public async Task<IActionResult> PostExpense([FromBody] BudgetItemDTO itemDTO)
    {
        if (ModelState.IsValid == false)
        {
            return BadRequest(ModelState);
        }
        else if (await _repository.EqualExistsAsync(OperationType.Expense, itemDTO.Name))
        {
            ModelState.AddModelError(string.Empty, "Expense with same name already exists");
            return BadRequest(ModelState);
        }

        return Ok(await PostItem(itemDTO, OperationType.Expense));
    }

    [HttpPut("incomes/{id}")]
    public async Task<IActionResult> PutIncome(int id, [FromBody] BudgetItemDTO itemDTO)
    {
        if (ModelState.IsValid == false)
        {
            return BadRequest(ModelState);
        }
        else if (await _repository.EqualExistsAsync(OperationType.Income, itemDTO.Name, id))
        {
            ModelState.AddModelError(string.Empty, "Income with same name already exists");
            return BadRequest(ModelState);
        }

        var result = await PutItem(id, itemDTO, OperationType.Income);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("expenses/{id}")]
    public async Task<IActionResult> PutExpense(int id, [FromBody] BudgetItemDTO itemDTO)
    {
        if (ModelState.IsValid == false)
        {
            return BadRequest(ModelState);
        }
        else if (await _repository.EqualExistsAsync(OperationType.Expense, itemDTO.Name, id))
        {
            ModelState.AddModelError(string.Empty, "Expense with same name already exists");
            return BadRequest(ModelState);
        }

        var result = await PutItem(id, itemDTO, OperationType.Expense);

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

        _cache.RemoveItem(id);
        _cache.RemoveItemsCollection();
        _cache.RemoveItemsCollection(OperationType.Income);
        _cache.RemoveItemsCollection(OperationType.Expense);

        return Ok();
    }

    private async Task<IEnumerable<IBudgetItem>> GetItems(OperationType? type = null)
    {
        IEnumerable<IBudgetItem> result;

        result = _cache.GetItemsCollection();
        if (result is not null)
            return result;
        
        OperationType[] types = type switch
        {
            OperationType.Income => new OperationType[] { OperationType.Income },
            OperationType.Expense => new OperationType[] { OperationType.Expense },
            _ => Enum.GetValues<OperationType>(),
        };
        result = await _repository.GetAllAsync(types);

        _cache.SetItemsCollection(result, type);

        return result;
    }
    private async Task<IBudgetItem> PostItem(BudgetItemDTO itemDTO, OperationType type)
    {
        var result = await _repository.PostAsync(new BudgetItem { Name = itemDTO.Name, Type = type });

        _cache.RemoveItemsCollection();
        _cache.RemoveItemsCollection(type);
        _cache.SetItem(result!);

        return result!;
    }
    private async Task<IBudgetItem?> PutItem(int id, BudgetItemDTO itemDTO, OperationType type)
    {
        var result = await _repository.PutAsync(id, new BudgetItem { Name = itemDTO.Name, Type = type });
        if (result is null)
            return result;

        _cache.RemoveItem(id);
        _cache.RemoveItemsCollection();
        _cache.RemoveItemsCollection(OperationType.Expense);
        _cache.SetItem(result);

        return result;
    }
}
