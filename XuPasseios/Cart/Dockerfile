FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "Cart.csproj"


RUN dotnet build "Cart.csproj" -c Release -o /app/build

FROM build AS publish

RUN dotnet publish "Cart.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY aspnetapp.pfx  /https/aspnetapp.pfx
#COPY Catalog.db /app/Catalog.db


EXPOSE 8080
EXPOSE 443


ENTRYPOINT ["dotnet", "Cart.dll"]