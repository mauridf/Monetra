using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Application.Common.DTOs;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Repositories;

namespace Monetra.Api.Controllers.Admin;

/// <summary>
/// Controller administrativo para gestão de usuários.
/// Acesso restrito a administradores.
/// </summary>
[Authorize(Policy = "Admin")]
[Route("api/v1/admin/users")]
public class AdminUsersController : BaseController
{
    private readonly UserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public AdminUsersController(UserRepository userRepo, IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Lista todos os usuários do sistema.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int perPage = 20)
    {
        var users = await _userRepo.GetAllAsync();
        var userDtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email.Value,
            Role = u.Role.ToString(),
            IsPremium = u.IsPremium,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        }).ToList();

        var paged = PaginatedResult<UserDto>.Create(
            userDtos.Skip((page - 1) * perPage).Take(perPage).ToList(),
            userDtos.Count,
            page,
            perPage);

        return OkPaginated(paged);
    }

    /// <summary>
    /// Ativa ou desativa um usuário.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ToggleUserStatus(Guid id, [FromBody] ToggleStatusRequest request)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        user.SetActive(request.IsActive);
        _userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Gerencia status premium de um usuário.
    /// </summary>
    [HttpPatch("{id:guid}/premium")]
    public async Task<IActionResult> ManagePremium(Guid id, [FromBody] ManagePremiumRequest request)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        user.SetPremium(request.IsPremium, request.PremiumUntil);
        _userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}

public class ToggleStatusRequest
{
    public bool IsActive { get; set; }
}

public class ManagePremiumRequest
{
    public bool IsPremium { get; set; }
    public DateTime? PremiumUntil { get; set; }
}
