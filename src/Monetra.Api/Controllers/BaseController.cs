using MediatR;
using Microsoft.AspNetCore.Mvc;
using Monetra.Application.Common.DTOs;

namespace Monetra.Api.Controllers;

/// <summary>
/// Controller base com helpers para respostas padronizadas.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected IMediator Mediator => HttpContext.RequestServices.GetRequiredService<IMediator>();

    /// <summary>
    /// Retorna resposta de sucesso 200 OK.
    /// </summary>
    protected IActionResult Ok<T>(T data)
    {
        return base.Ok(SuccessResponse<T>.Create(data));
    }

    /// <summary>
    /// Retorna resposta de sucesso 201 Created.
    /// </summary>
    protected IActionResult Created<T>(string actionName, object routeValues, T data)
    {
        return base.CreatedAtAction(actionName, routeValues, SuccessResponse<T>.Create(data));
    }

    /// <summary>
    /// Retorna resposta paginada 200 OK.
    /// </summary>
    protected IActionResult OkPaginated<T>(PaginatedResult<T> result)
    {
        return base.Ok(PaginatedSuccessResponse<T>.Create(result));
    }

    /// <summary>
    /// Retorna resposta 204 No Content.
    /// </summary>
    protected new IActionResult NoContent()
    {
        return base.NoContent();
    }
}
