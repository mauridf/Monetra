using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monetra.Application.Common.DTOs;
using Monetra.Core.Entities;
using Monetra.Core.Exceptions;
using Monetra.Core.Interfaces;
using Monetra.Infrastructure.Repositories;

namespace Monetra.Api.Controllers;

[Authorize]
public class ProfileController : BaseController
{
    private readonly IPersonRepository _personRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ProfileController(
        IPersonRepository personRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _personRepo = personRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var person = await _personRepo.GetByUserIdAsync(userId);

        if (person == null)
            return NotFound();

        return Ok(new PersonDto
        {
            Id = person.Id,
            Phone = person.Phone,
            BirthDate = person.BirthDate,
            Occupation = person.Occupation,
            City = person.City,
            State = person.State,
            Country = person.Country
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateProfile([FromBody] CreatePersonRequest request)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var existing = await _personRepo.GetByUserIdAsync(userId);
        if (existing != null)
            throw new ConflictException("Perfil já existe.");

        var person = Person.Create(userId);

        if (!string.IsNullOrWhiteSpace(request.Phone) || request.BirthDate.HasValue || !string.IsNullOrWhiteSpace(request.Occupation) ||
            !string.IsNullOrWhiteSpace(request.City) || !string.IsNullOrWhiteSpace(request.State))
        {
            person.Update(
                request.Phone,
                request.BirthDate.HasValue ? DateOnly.FromDateTime(request.BirthDate.Value) : null,
                request.Occupation,
                city: request.City,
                state: request.State);
        }

        await _personRepo.AddAsync(person);
        await _unitOfWork.SaveChangesAsync();

        return Created(nameof(GetProfile), new { id = person.Id }, MapToDto(person));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdatePersonRequest request)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var person = await _personRepo.GetByUserIdAsync(userId)
            ?? throw new NotFoundException("Perfil não encontrado.");

        person.Update(
            request.Phone,
            request.BirthDate.HasValue ? DateOnly.FromDateTime(request.BirthDate.Value) : null,
            request.Occupation,
            city: request.City,
            state: request.State);

        _personRepo.Update(person);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    private static PersonDto MapToDto(Person p) => new()
    {
        Id = p.Id,
        Phone = p.Phone,
        BirthDate = p.BirthDate,
        Occupation = p.Occupation,
        City = p.City,
        State = p.State,
        Country = p.Country
    };
}

public class CreatePersonRequest
{
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Occupation { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
}

public class UpdatePersonRequest
{
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Occupation { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
}
