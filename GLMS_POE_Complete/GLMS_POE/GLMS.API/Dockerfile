# ─────────────────────────────────────────────────────────────────────────────
# GLMS.API Dockerfile — Multi-stage build
# Stage 1: SDK image compiles and publishes the app
# Stage 2: Slim runtime image contains only what's needed to run
# ─────────────────────────────────────────────────────────────────────────────

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj first — NuGet restore is cached as its own layer
# If only .cs files change, this layer is reused (faster rebuilds)
COPY ["GLMS.API/GLMS.API.csproj", "GLMS.API/"]
RUN dotnet restore "GLMS.API/GLMS.API.csproj"

COPY . .
WORKDIR "/src/GLMS.API"
RUN dotnet publish "GLMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5001
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5001

ENTRYPOINT ["dotnet", "GLMS.API.dll"]
