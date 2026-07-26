using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Core.Entities;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Repositories;

namespace Monetra.Api.Controllers;

[Authorize]
public class BudgetsController : BaseController
{
    private readonly BudgetRepository _budgetRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public BudgetsController(
        BudgetRepository budgetRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _budgetRepo = budgetRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Obtém orçamento do período atual.
    /// </summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentBudget()
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var budget = await _budgetRepo.GetCurrentBudgetAsync(userId);

        if (budget == null)
            return NotFound(new { Message = "Nenhum orçamento ativo no período atual." });

        return Ok(MapToDto(budget));
    }

    /// <summary>
    /// Obtém orçamento específico com progresso.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBudget(Guid id)
    {
        var budget = await _budgetRepo.GetWithProgressAsync(id);
        if (budget == null)
            return NotFound();

        return Ok(MapToDto(budget));
    }

    /// <summary>
    /// Cria um novo orçamento.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetRequest request)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var budget = Budget.Create(
            userId,
            request.Name,
            request.Period,
            DateOnly.FromDateTime(request.StartDate),
            DateOnly.FromDateTime(request.EndDate),
            request.TotalLimit,
            request.IsTemplate);

        // Adicionar categorias
        if (request.Categories != null)
        {
            foreach (var cat in request.Categories)
            {
                budget.AddCategory(cat.CategoryId, cat.LimitAmount);
            }
        }

        budget.Activate(); // Ativar diretamente

        await _budgetRepo.AddAsync(budget);
        await _unitOfWork.SaveChangesAsync();

        return Created(nameof(GetBudget), new { id = budget.Id }, MapToDto(budget));
    }

    private static object MapToDto(Budget budget)
    {
        return new
        {
            budget.Id,
            budget.Name,
            Period = budget.Period.ToString(),
            budget.StartDate,
            budget.EndDate,
            budget.TotalLimit,
            budget.TotalSpent,
            ProgressPercentage = budget.GetSpentPercentage(),
            budget.Status,
            Categories = budget.Categories.Select(bc => new
            {
                bc.Id,
                bc.CategoryId,
                CategoryName = bc.Category?.Name ?? "Desconhecida",
                bc.LimitAmount,
                bc.SpentAmount,
                ProgressPercentage = bc.GetSpentPercentage(),
                IsOverLimit = bc.IsOverLimit()
            })
        };
    }
}

public class CreateBudgetRequest
{
    public string Name { get; set; } = string.Empty;
    public string Period { get; set; } = "monthly";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalLimit { get; set; }
    public bool IsTemplate { get; set; }
    public List<BudgetCategoryRequest>? Categories { get; set; }
}

public class BudgetCategoryRequest
{
    public Guid CategoryId { get; set; }
    public decimal LimitAmount { get; set; }
}
