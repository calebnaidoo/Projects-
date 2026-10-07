# ─────────────────────────────────────────────────────────────────────────────
# GLMS.Web Dockerfile — Multi-stage build
# ─────────────────────────────────────────────────────────────────────────────

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["GLMS.Web/GLMS.Web.csproj", "GLMS.Web/"]
RUN dotnet restore "GLMS.Web/GLMS.Web.csproj"

COPY . .
WORKDIR "/src/GLMS.Web"
RUN dotnet publish "GLMS.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Ensure uploads folder exists inside the container
RUN mkdir -p /app/wwwroot/uploads/agreements

EXPOSE 5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "GLMS.Web.dll"]
