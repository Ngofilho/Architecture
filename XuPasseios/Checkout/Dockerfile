FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Checkout/Checkout.csproj", "Checkout/Checkout.csproj"]
#COPY ["Checkout.csproj", "Checkout/Checkout.csproj"]
COPY ["Checkout/aspnetapp.pfx",  "Checkout/https/aspnetapp.pfx"]
#COPY ["aspnetapp.pfx",  "Checkout/https/aspnetapp.pfx"]
COPY ["Common/Common.csproj", "Common/Common.csproj"]
#COPY ["Common.csproj", "Common/Common.csproj"]

COPY . .

RUN dotnet restore "Checkout/Checkout.csproj"
#RUN dotnet restore "Checkout.csproj"

RUN dotnet build "Checkout/Checkout.csproj" -c Release -o /app/build
#RUN dotnet build "Checkout.csproj" -c Release -o /app/build

FROM build AS publish

RUN dotnet publish "Checkout/Checkout.csproj" -c Release -o /app/publish /p:UseAppHost=false
#RUN dotnet publish "Checkout.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
#COPY aspnetapp.pfx  /https/aspnetapp.pfx
#COPY Catalog.db /app/Catalog.db


EXPOSE 8080
EXPOSE 443


#ENTRYPOINT ["dotnet", "Checkout/Checkout.dll"]
ENTRYPOINT ["dotnet", "Checkout.dll"]