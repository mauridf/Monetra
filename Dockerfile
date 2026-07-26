# =============================================
# Monetra API - Docker Image
# =============================================

# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copiar arquivos de projeto e restaurar dependências
COPY src/Monetra.Core/Monetra.Core.csproj Monetra.Core/
COPY src/Monetra.Application/Monetra.Application.csproj Monetra.Application/
COPY src/Monetra.Infrastructure/Monetra.Infrastructure.csproj Monetra.Infrastructure/
COPY src/Monetra.Api/Monetra.Api.csproj Monetra.Api/

RUN dotnet restore Monetra.Api/Monetra.Api.csproj

# Copiar código fonte e compilar
COPY src/ .
WORKDIR /src/Monetra.Api
RUN dotnet publish Monetra.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Estágio 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Criar usuário não-root para segurança
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copiar binários publicados
COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# Expor porta
EXPOSE 8080

# Variáveis de ambiente
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Iniciar aplicação
ENTRYPOINT ["dotnet", "Monetra.Api.dll"]