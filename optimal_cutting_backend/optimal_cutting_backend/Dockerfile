FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG configuration=Debug
WORKDIR /src
COPY ["optimal_cutting_backend/optimal_cutting_backend/vega.csproj", "optimal_cutting_backend/optimal_cutting_backend/"]
RUN dotnet restore "optimal_cutting_backend/optimal_cutting_backend/vega.csproj"
COPY . .
RUN dotnet build "optimal_cutting_backend/optimal_cutting_backend/vega.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Debug
RUN dotnet publish "optimal_cutting_backend/optimal_cutting_backend/vega.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "vega.dll"]
