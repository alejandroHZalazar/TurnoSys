FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/TurnoSys.API/TurnoSys.API.csproj", "src/TurnoSys.API/"]
COPY ["src/TurnoSys.Application/TurnoSys.Application.csproj", "src/TurnoSys.Application/"]
COPY ["src/TurnoSys.Domain/TurnoSys.Domain.csproj", "src/TurnoSys.Domain/"]
COPY ["src/TurnoSys.Infrastructure/TurnoSys.Infrastructure.csproj", "src/TurnoSys.Infrastructure/"]
COPY ["NuGet.Config", "."]
RUN dotnet restore "src/TurnoSys.API/TurnoSys.API.csproj"
COPY . .
RUN dotnet build "src/TurnoSys.API/TurnoSys.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/TurnoSys.API/TurnoSys.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TurnoSys.API.dll"]
