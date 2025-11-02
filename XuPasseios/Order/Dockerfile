FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "Order.csproj"


RUN dotnet build "Order.csproj" -c Release -o /app/build

FROM build AS publish

RUN dotnet publish "Order.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
EXPOSE 443


ENTRYPOINT ["dotnet", "Order.dll"]