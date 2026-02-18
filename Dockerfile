# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY QrCafe.sln ./
COPY QrCafe.Domain/QrCafe.Domain.csproj         QrCafe.Domain/
COPY QrCafe.Infrastructure/QrCafe.Infrastructure.csproj QrCafe.Infrastructure/
COPY QrCafe.Application/QrCafe.Application.csproj     QrCafe.Application/
COPY QrCafe.Api/QrCafe.Api.csproj               QrCafe.Api/
RUN dotnet restore

COPY . .
RUN dotnet publish QrCafe.Api/QrCafe.Api.csproj -c Release -o /app/publish --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5015
EXPOSE 5015

ENTRYPOINT ["dotnet", "QrCafe.Api.dll"]
