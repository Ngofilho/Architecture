FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "ServiceBus.HealthCheck.csproj"

#WORKDIR "CityInfo.API/"

RUN dotnet build "ServiceBus.HealthCheck.csproj" -c Release -o /app/build

FROM build AS publish

RUN dotnet publish "ServiceBus.HealthCheck.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
EXPOSE 443

RUN apt update && apt install curl -y

ENTRYPOINT ["dotnet", "ServiceBus.HealthCheck.dll"]