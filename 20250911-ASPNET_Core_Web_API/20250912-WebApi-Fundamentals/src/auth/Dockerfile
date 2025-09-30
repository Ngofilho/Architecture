FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

COPY ["IdentityServer/IdentityServer.csproj", "IdentityServer.csproj"]

RUN ls -la

RUN dotnet restore "IdentityServer/IdentityServer.csproj"


WORKDIR "IdentityServer/"

RUN dotnet build "IdentityServer.csproj" -c Release -o /app/build


FROM build AS publish

RUN dotnet publish "IdentityServer.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
EXPOSE 7090
EXPOSE 5243

ENTRYPOINT ["dotnet", "IdentityServer.dll"]