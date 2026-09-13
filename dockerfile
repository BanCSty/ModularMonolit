FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем конфигурационные файлы
COPY ["NuGet.config", "./"]

COPY ["ModularMonolit.sln", "./"]

COPY ["ModularMonolit/ModularMonolit.Api.csproj", "ModularMonolit/"]
COPY ["Users.Domain/Users.Domain.csproj", "Users.Domain/"]
COPY ["Users.Contracts/Users.Contracts.csproj", "Users.Contracts/"]
COPY ["Users.Application/Users.Application.csproj", "Users.Application/"]
COPY ["Users.Infrastructure/Users.Infrastructure.csproj", "Users.Infrastructure/"]
COPY ["Users.Presentation/Users.Presentation.csproj", "Users.Presentation/"]
COPY ["Orders.Domain/Orders.Domain.csproj", "Orders.Domain/"]
COPY ["Orders.Application/Orders.Application.csproj", "Orders.Application/"]
COPY ["Orders.Infrastructure/Orders.Infrastructure.csproj", "Orders.Infrastructure/"]
COPY ["Orders.Presentation/Orders.Presentation.csproj", "Orders.Presentation/"]
COPY ["Shared/Shared.csproj", "Shared/"]

RUN dotnet restore ModularMonolit.sln

COPY . .

RUN dotnet publish ModularMonolit/ModularMonolit.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Создаём директорию для данных
RUN mkdir -p /app/data

EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:8080

ENV ConnectionStrings__UsersDatabase="Data Source=/app/data/users.db"
ENV ConnectionStrings__OrdersDatabase="Data Source=/app/data/orders.db"

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD wget -qO- http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ModularMonolit.Api.dll"]