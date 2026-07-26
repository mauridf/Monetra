using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Application.Common.DTOs;
using Monetra.Core.Entities;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Repositories;

namespace Monetra.Api.Controllers;

[Authorize]
public class TransactionCategoriesController : BaseController
{
    private readonly TransactionCategoryRepository _categoryRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public TransactionCategoriesController(
        TransactionCategoryRepository categoryRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _categoryRepo = categoryRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lista categorias em árvore hierárquica.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategories([FromQuery] string? type)
    {
        var userId = _currentUser.UserId;
        var categories = await _categoryRepo.GetTreeAsync(userId, type);

        var dtos = categories.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Obtém detalhes de uma categoria.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        var category = await _categoryRepo.GetByIdAsync(id);
        if (category == null)
            return NotFound();

        return Ok(MapToDto(category));
    }

    /// <summary>
    /// Cria uma nova categoria personalizada.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var category = TransactionCategory.Create(
            request.Name,
            request.TransactionType,
            userId,
            request.ParentId,
            false,
            request.Description,
            request.Icon,
            request.Color,
            request.MonthlyBudgetLimit);

        await _categoryRepo.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return Created(nameof(GetCategory), new { id = category.Id }, MapToDto(category));
    }

    /// <summary>
    /// Atualiza uma categoria (apenas categorias do usuário).
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var category = await _categoryRepo.GetByIdAsync(id);
        if (category == null)
            return NotFound();

        category.Update(
            request.Name,
            request.Description,
            request.Icon,
            request.Color,
            request.MonthlyBudgetLimit);

        _categoryRepo.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Remove uma categoria personalizada.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var category = await _categoryRepo.GetByIdAsync(id);
        if (category == null)
            return NotFound();

        if (category.IsSystem)
            return BadRequest("Categorias do sistema não podem ser removidas.");

        _categoryRepo.Remove(category);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    private static CategoryDto MapToDto(TransactionCategory category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Icon = category.Icon,
            Color = category.Color,
            TransactionType = category.TransactionType.ToString(),
            IsSystem = category.IsSystem,
            Level = category.Level,
            Children = category.Children?.Select(MapToDto).ToList() ?? new()
        };
    }
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string TransactionType { get; set; } = "expense";
    public Guid? ParentId { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public decimal? MonthlyBudgetLimit { get; set; }
}

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public decimal? MonthlyBudgetLimit { get; set; }
}
