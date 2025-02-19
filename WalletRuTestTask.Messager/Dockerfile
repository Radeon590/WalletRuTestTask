FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WalletRuTestTask.Messager/WalletRuTestTask.Messager.csproj", "WalletRuTestTask.Messager/"]
RUN dotnet restore "WalletRuTestTask.Messager/WalletRuTestTask.Messager.csproj"
COPY . .
WORKDIR "/src/WalletRuTestTask.Messager"
RUN dotnet build "WalletRuTestTask.Messager.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WalletRuTestTask.Messager.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WalletRuTestTask.Messager.dll"]
