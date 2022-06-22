using Domain.DTO;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Interfaces.DataAccess;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BudgetOperationsController : ControllerBase
{
    private readonly IBudgetOperationsRepository _repository;
    private readonly IBudgetItemsRepository _itemsRepository;

    public BudgetOperationsController(IBudgetOperationsRepository repository, IBudgetItemsRepository itemsRepository)
    {
        _repository = repository;
        _itemsRepository = itemsRepository;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _repository.GetByIdAsync(id);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOverTimePeriod([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo)
    {
        return Ok(await _repository.GetAllOverTimePeriodAsync(Enum.GetValues<OperationType>(), dateFrom, dateTo));
    }

    [HttpGet("incomes")]
    public async Task<IActionResult> GetAllIncomes([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo)
    {
        return Ok(await _repository.GetAllOverTimePeriodAsync(new OperationType[] { OperationType.Income }, dateFrom, dateTo));
    }

    [HttpGet("expenses")]
    public async Task<IActionResult> GetAllExpenses([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo)
    {
        return Ok(await _repository.GetAllOverTimePeriodAsync(new OperationType[] { OperationType.Expense }, dateFrom, dateTo));
    }

    [HttpPost("incomes")]
    public async Task<IActionResult> PostIncome([FromBody] BudgetOperationDTO operationDTO)
    {
        if (ModelState.IsValid == false)
            return BadRequest(ModelState);

        IBudgetItem? item = await _itemsRepository.GetByIdAsync(operationDTO.ItemId);

        if (item is null || item.Type != OperationType.Income)
        {
            ModelState.AddModelError(string.Empty, $"There is no income with id: {operationDTO.ItemId}");
            return NotFound(ModelState);
        }

        var result = await _repository.PostAsync(new BudgetOperation
        {
            Date = operationDTO.Date,
            Type = OperationType.Income,
            Sum = operationDTO.Sum,
            Item = item
        });
        return Ok(result);
    }

    [HttpPost("expenses")]
    public async Task<IActionResult> PostExpense([FromBody] BudgetOperationDTO operationDTO)
    {
        if (ModelState.IsValid == false)
            return BadRequest(ModelState);

        IBudgetItem? item = await _itemsRepository.GetByIdAsync(operationDTO.ItemId);

        if (item is null || item.Type != OperationType.Expense)
        {
            ModelState.AddModelError(string.Empty, $"There is no expense with id: {operationDTO.ItemId}");
            return NotFound(ModelState);
        }

        var result = await _repository.PostAsync(new BudgetOperation
        {
            Date = operationDTO.Date,
            Type = OperationType.Expense,
            Sum = operationDTO.Sum,
            Item = item
        });
        return Ok(result);
    }

    [HttpPut("incomes/{id}")]
    public async Task<IActionResult> PutIncome(int id, [FromBody] BudgetOperationDTO operationDTO)
    {
        if (ModelState.IsValid == false)
            return BadRequest(ModelState);

        IBudgetItem? item = await _itemsRepository.GetByIdAsync(operationDTO.ItemId);

        if (item is null || item.Type != OperationType.Income)
        {
            ModelState.AddModelError(string.Empty, $"There is no income with id: {operationDTO.ItemId}");
            return NotFound(ModelState);
        }

        var result = await _repository.PutAsync(id, new BudgetOperation
        {
            Date = operationDTO.Date,
            Type = OperationType.Income,
            Sum = operationDTO.Sum,
            Item = item            
        });

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("expenses/{id}")]
    public async Task<IActionResult> PutExpense(int id, [FromBody] BudgetOperationDTO operationDTO)
    {
        if (ModelState.IsValid == false)
            return BadRequest(ModelState);

        IBudgetItem? item = await _itemsRepository.GetByIdAsync(operationDTO.ItemId);

        if (item is null || item.Type != OperationType.Expense)
        {
            ModelState.AddModelError(string.Empty, $"There is no expense with id: {operationDTO.ItemId}");
            return NotFound(ModelState);
        }

        var result = await _repository.PutAsync(id, new BudgetOperation
        {
            Date = operationDTO.Date,
            Type = OperationType.Expense,
            Sum = operationDTO.Sum,
            Item = item
        });

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

        return Ok();
    }
}
