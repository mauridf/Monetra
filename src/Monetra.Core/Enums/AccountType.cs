namespace Monetra.Core.Enums;

public enum AccountType
{
    Checking = 0,      // Conta Corrente
    Savings = 1,       // Poupança
    Cash = 2,          // Dinheiro físico
    Investment = 3,    // Investimento
    CreditCard = 4,    // Cartão de crédito (quando tratado como conta)
    Other = 5          // Outros
}
