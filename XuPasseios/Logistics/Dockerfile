FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Logistics/Logistics.csproj", "Logistics/Logistics.csproj"]
COPY ["Common/Common.csproj", "Common/Common.csproj"]

COPY . .

RUN dotnet restore "Logistics/Logistics.csproj"


RUN dotnet build "Logistics/Logistics.csproj" -c Release -o /app/build

FROM build AS publish

RUN dotnet publish "Logistics/Logistics.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
EXPOSE 443


ENTRYPOINT ["dotnet", "Logistics.dll"]