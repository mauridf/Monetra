using MediatR;
using Microsoft.Extensions.Logging;
using Monetra.Core.Interfaces;

namespace Monetra.Application.Common.Behaviors;

/// <summary>
/// Behavior do MediatR para gerenciamento de transações do EF Core.
/// Abre transação para Commands e confirma/rollback conforme resultado.
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(IUnitOfWork unitOfWork, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Apenas aplicar transação para Commands (não Queries)
        var isCommand = typeof(TRequest).Name.EndsWith("Command");

        if (!isCommand)
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;

        try
        {
            _logger.LogDebug("Iniciando transação para {RequestName}", requestName);
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var response = await next();

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            _logger.LogDebug("Transação confirmada para {RequestName}", requestName);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Transação revertida para {RequestName}", requestName);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
