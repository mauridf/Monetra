using System.Net;
using System.Text.Json;
using Monetra.Application.Common.DTOs;
using Monetra.Core.Exceptions;

namespace Monetra.Api.Middlewares;

/// <summary>
/// Middleware global para tratamento de exceções não capturadas.
/// Converte exceções de domínio em respostas HTTP padronizadas.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogInformation(ex, "Recurso não encontrado");
            await WriteErrorResponse(context, HttpStatusCode.NotFound, "NOT_FOUND", ex.Message);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação: {Message}", ex.Message);
            var details = ex.Errors?.SelectMany(kvp =>
                kvp.Value.Select(v => ErrorDetail.Create(kvp.Key, v))).ToList();

            await WriteErrorResponse(context, HttpStatusCode.BadRequest, "VALIDATION_ERROR", ex.Message, details);
        }
        catch (InsufficientBalanceException ex)
        {
            _logger.LogWarning(ex, "Saldo insuficiente: Disponível={Available}, Solicitado={Requested}",
                ex.AvailableBalance, ex.RequestedAmount);
            await WriteErrorResponse(context, HttpStatusCode.UnprocessableEntity, "INSUFFICIENT_BALANCE", ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Acesso não autorizado");
            await WriteErrorResponse(context, HttpStatusCode.Unauthorized, "UNAUTHORIZED", ex.Message);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Conflito: {Message}", ex.Message);
            await WriteErrorResponse(context, HttpStatusCode.Conflict, "CONFLICT", ex.Message);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Regra de negócio violada: {Message}", ex.Message);
            await WriteErrorResponse(context, HttpStatusCode.UnprocessableEntity, ex.ErrorCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno não tratado: {Message}", ex.Message);
            await WriteErrorResponse(context, HttpStatusCode.InternalServerError,
                "INTERNAL_ERROR", "Ocorreu um erro interno. Tente novamente mais tarde.");
        }
    }

    private static async Task WriteErrorResponse(
        HttpContext context,
        HttpStatusCode statusCode,
        string code,
        string message,
        List<ErrorDetail>? details = null)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var errorResponse = ErrorResponse.Create(code, message, details);
        var json = JsonSerializer.Serialize(errorResponse, JsonOptions);

        await context.Response.WriteAsync(json);
    }
}
