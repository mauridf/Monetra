using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Monetra.Application.Common.DTOs;

namespace Monetra.Api.Filters;

/// <summary>
/// Filtro global para validação automática do ModelState.
/// </summary>
public class ValidateModelFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(err =>
                    ErrorDetail.Create(e.Key, err.ErrorMessage)))
                .ToList();

            var errorResponse = ErrorResponse.Create(
                "VALIDATION_ERROR",
                "Dados inválidos. Verifique os campos enviados.",
                errors);

            context.Result = new BadRequestObjectResult(errorResponse);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Não precisa de implementação
    }
}
