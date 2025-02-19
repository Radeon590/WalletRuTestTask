FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WalletRuTestTask/WalletRuTestTask.csproj", "WalletRuTestTask/"]
RUN dotnet restore "WalletRuTestTask/WalletRuTestTask.csproj"
COPY . .
WORKDIR "/src/WalletRuTestTask"
RUN dotnet build "WalletRuTestTask.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WalletRuTestTask.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WalletRuTestTask.dll"]
