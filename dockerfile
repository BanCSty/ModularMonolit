FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем конфигурационные файлы
COPY ["NuGet.config", "./"]

COPY ["ModularMonolit.sln", "./"]

COPY ["ModularMonolit/ModularMonolit.Api.csproj", "ModularMonolit/"]
COPY ["Moduls/Users/Users.Domain/Users.Domain.csproj", "Moduls/Users/Users.Domain/"]
COPY ["Moduls/Users/Users.Application/Users.Application.csproj", "Moduls/Users/Users.Application/"]
COPY ["Moduls/Users/Users.Infrastructure/Users.Infrastructure.csproj", "Moduls/Users/Users.Infrastructure/"]
COPY ["Moduls/Users/Users.Presentation/Users.Presentation.csproj", "Moduls/Users/Users.Presentation/"]
COPY ["Moduls/Orders/Orders.Domain/Orders.Domain.csproj", "Moduls/Orders/Orders.Domain/"]
COPY ["Moduls/Orders/Orders.Application/Orders.Application.csproj", "Moduls/Orders/Orders.Application/"]
COPY ["Moduls/Orders/Orders.Infrastructure/Orders.Infrastructure.csproj", "Moduls/Orders/Orders.Infrastructure/"]
COPY ["Moduls/Orders/Orders.Presentation/Orders.Presentation.csproj", "Moduls/Orders/Orders.Presentation/"]
COPY ["Shared/Shared.Abstractions/Shared.Abstractions.csproj", "Shared/Shared.Abstractions/"]
COPY ["Shared/Shared.Events/Shared.Events.csproj", "Shared/Shared.Events/"]
COPY ["Shared/Shared.Extensions/Shared.Extensions.csproj", "Shared/Shared.Extensions/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
COPY ["Shared/Shared.Outbox/Shared.Outbox.csproj", "Shared/Shared.Outbox/"]

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