# =============================================
# Monetra Scheduler - Docker Image
# =============================================

# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copiar arquivos de projeto e restaurar dependências
COPY src/Monetra.Core/Monetra.Core.csproj Monetra.Core/
COPY src/Monetra.Application/Monetra.Application.csproj Monetra.Application/
COPY src/Monetra.Infrastructure/Monetra.Infrastructure.csproj Monetra.Infrastructure/
COPY src/Monetra.Scheduler/Monetra.Scheduler.csproj Monetra.Scheduler/

RUN dotnet restore Monetra.Scheduler/Monetra.Scheduler.csproj

# Copiar código fonte e compilar
COPY src/ .
WORKDIR /src/Monetra.Scheduler
RUN dotnet publish Monetra.Scheduler.csproj -c Release -o /app/publish /p:UseAppHost=false

# Estágio 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Criar usuário não-root
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copiar binários publicados
COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=60s --timeout=10s --retries=3 \
    CMD pgrep dotnet || exit 1

# Variáveis de ambiente
ENV ASPNETCORE_ENVIRONMENT=Production

# Iniciar scheduler
ENTRYPOINT ["dotnet", "Monetra.Scheduler.dll"]