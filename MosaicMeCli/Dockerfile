FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["MosaicMeCli/MosaicMeCli.csproj", "MosaicMeCli/"]
RUN dotnet restore "MosaicMeCli/MosaicMeCli.csproj"
COPY . .
WORKDIR "/src/MosaicMeCli"
RUN dotnet build "MosaicMeCli.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MosaicMeCli.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MosaicMeCli.dll"]
