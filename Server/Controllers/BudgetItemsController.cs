using Domain.DTO;
using Domain.Enums;
using Domain.Interfaces.DataAccess;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BudgetItemsController : ControllerBase
{
    private readonly IBudgetItemsRepository _repository;

    public BudgetItemsController(IBudgetItemsRepository repository)
    {
        _repository = repository;
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
    public async Task<IActionResult> GetAllWithPaging([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _repository.GetAllWithPagingAsync(Enum.GetValues<OperationType>(), pageNumber, pageSize));
    }

    [HttpGet("incomes")]
    public async Task<IActionResult> GetAllIncomesWithPaging([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _repository.GetAllWithPagingAsync(new OperationType[] { OperationType.Income }, pageNumber, pageSize));
    }

    [HttpGet("expenses")]
    public async Task<IActionResult> GetAllExpensesWithPaging([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _repository.GetAllWithPagingAsync(new OperationType[] { OperationType.Expense }, pageNumber, pageSize));
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

        var result = await _repository.PostAsync(new BudgetItem { Name = itemDTO.Name, Type = OperationType.Income } );

        return Ok(result);
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

        var result = await _repository.PostAsync(new BudgetItem { Name = itemDTO.Name, Type = OperationType.Expense });

        return Ok(result);
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

        var result = await _repository.PutAsync(id, new BudgetItem { Name = itemDTO.Name, Type = OperationType.Income });

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

        var result = await _repository.PutAsync(id, new BudgetItem { Name = itemDTO.Name, Type = OperationType.Expense });

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
