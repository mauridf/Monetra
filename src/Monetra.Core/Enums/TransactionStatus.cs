namespace Monetra.Core.Enums;

public enum TransactionStatus
{
    Pending = 0,      // Pendente
    Completed = 1,    // Concluída (paga/recebida)
    Cancelled = 2,    // Cancelada
    Reconciled = 3    // Conciliada
}
