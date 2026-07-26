# Monetra - Sistema de Gestão Financeira Pessoal

Monetra é um sistema de gestão financeira pessoal construído com **.NET 10**, seguindo princípios de **Domain-Driven Design (DDD)**, **Clean Architecture** e **CQRS**.

## Arquitetura

```
Monetra.slnx
├── src/
│   ├── Monetra.Core/          # Camada de domínio: entidades, value objects, enums, eventos, interfaces
│   ├── Monetra.Application/   # Casos de uso: serviços de aplicação, DTOs, interfaces de serviços
│   ├── Monetra.Infrastructure/ # Persistência (EF Core + PostgreSQL), repositórios, provedores de serviço
│   ├── Monetra.Api/           # API REST: controllers, middlewares, configuração de pipelines
│   └── Monetra.Scheduler/     # Jobs agendados (Quartz.NET): faturas, notificações, limpeza
├── tests/
│   ├── Monetra.Tests.Unit/        # Testes unitários (xUnit + FluentAssertions)
│   ├── Monetra.Tests.Integration/ # Testes de integração (EF Core InMemory)
│   └── Monetra.Tests.E2E/         # Testes end-to-end (API running)
└── README.md
```

### Camadas

- **Core**: Entidades de domínio (`User`, `Transaction`, `Wallet`, `BankAccount`, `Budget`, `Invoice`, etc.), Value Objects (`Email`, `Money`), Enums, Eventos de Domínio, Exceções, Interfaces de Repositório.
- **Application**: Serviços de aplicação (`AuthService`, `FinancialCalculatorService`, `RecurringTransactionService`, `ReportGeneratorService`), DTOs, interfaces de serviços.
- **Infrastructure**: `MonetraDbContext` (EF Core), implementações de repositórios (`GenericRepository<T>`, `TransactionRepository`, etc.), interceptors (`AuditInterceptor`, `DomainEventInterceptor`), serviços (`SmtpNotificationService`, `OutboxProcessor`).
- **Api**: Controllers REST (13 controllers, ~54 endpoints), `GlobalExceptionMiddleware`, configuração Swagger/OpenAPI/Scalar, políticas de rate limiting.
- **Scheduler**: 6 jobs Quartz.NET para tarefas agendadas (faturamento, notificações, expiração premium, limpeza).

## Entidades de Domínio (18)

| Entidade | Descrição |
|----------|-----------|
| `User` | Usuário com autenticação, 2FA, premium, soft delete |
| `Person` | Dados pessoais (CPF, RG, endereço) - LGPD |
| `BankAccount` | Contas bancárias com saldo e histórico |
| `BankAccountBalance` | Histórico de saldos |
| `Transaction` | Transações financeiras com conciliação |
| `TransactionCategory` | Categorias de transações |
| `RecurringTransaction` | Transações recorrentes |
| `Wallet` | Carteiras/metas financeiras |
| `WalletTransaction` | Movimentações de carteira |
| `Budget` | Orçamentos mensais/anuais |
| `BudgetCategory` | Categorias do orçamento |
| `CreditCard` | Cartões de crédito |
| `Invoice` | Faturas de cartão |
| `Transfer` | Transferências entre contas |
| `Notification` | Notificações do sistema |
| `ActivityLog` | Log de auditoria |
| `OutboxMessage` | Mensagens da outbox pattern |
| `FinancialGoal` | Metas financeiras |

## Value Objects (6)

- `Email`, `Money`, `Phone`, `Address`, `Document`, `TransactionType` (string-based)

## Eventos de Domínio (9)

- `TransactionCreatedEvent`, `TransactionCategorizedEvent`, `UserRegisteredEvent`, `AccountBalanceChangedEvent`, `WalletContributedEvent`, `WalletGoalReachedEvent`, `BudgetExceededEvent`, `TransferCreatedEvent`, `TransferCompletedEvent`

## Tecnologias

- .NET 10 + C# 13
- ASP.NET Core Minimal API + Controllers
- Entity Framework Core 10 + PostgreSQL (Npgsql)
- MediatR (estrutura CQRS disponível, controllers usam DI direta atualmente)
- Quartz.NET 3.x (agendamento)
- Scalar (documentação API)
- FluentValidation
- xUnit + FluentAssertions
- Serilog

## Build & Testes

```bash
# Build completo
dotnet build Monetra.slnx

# Testes unitários (75 testes)
dotnet test tests/Monetra.Tests.Unit

# Testes de integração (requer InMemory EF Core configurado)
dotnet test tests/Monetra.Tests.Integration

# Testes E2E (requer instância da API rodando)
dotnet test tests/Monetra.Tests.E2E
```

## Endpoints da API

A API expõe ~54 endpoints RESTful sob `/api/v1/`, incluindo:

- **Auth**: registro, login, refresh token, 2FA
- **Users**: perfil, preferências, administração
- **Bank Accounts**: CRUD, arquivamento, conciliação de saldo
- **Transactions**: CRUD com filtros (tipo, data, categoria, busca), pagamento, conciliação
- **Categories**: CRUD hierárquico
- **Recurring Transactions**: CRUD com simulação
- **Budgets**: CRUD com categorias, monitoramento de limite
- **Wallets**: CRUD com contribuições, retiradas, progresso
- **Credit Cards**: CRUD com limite, data de fechamento/vencimento
- **Invoices**: visualização, pagamento
- **Transfers**: transferências entre contas
- **Reports**: relatórios mensais/anuais, exportação PDF
- **Notifications**: listagem, marcação como lida

## Licença

Proprietária - Todos os direitos reservados.
