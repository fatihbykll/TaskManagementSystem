# ── Aşama 1: Build ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Önce sadece proje dosyaları → layer cache optimizasyonu
COPY TaskManagement.slnx .
COPY TaskManagement.API/TaskManagement.API.csproj            TaskManagement.API/
COPY TaskManagement.Application/TaskManagement.Application.csproj TaskManagement.Application/
COPY TaskManagement.Domain/TaskManagement.Domain.csproj      TaskManagement.Domain/
COPY TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj TaskManagement.Infrastructure/
RUN dotnet restore TaskManagement.API/TaskManagement.API.csproj
COPY . .
RUN dotnet publish TaskManagement.API/TaskManagement.API.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore
# ── Aşama 2: Runtime (SDK içermez → imaj boyutu küçük) ────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
# Güvenlik: root olmayan kullanıcı
RUN groupadd -r appgroup && useradd -r -g appgroup appuser
COPY --from=build /app/publish .
# Dosya yükleme dizini
RUN mkdir -p wwwroot/uploads/attachments \
    && chown -R appuser:appgroup wwwroot
USER appuser
# Kestrel sadece internal ağda dinler
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "TaskManagement.API.dll"]
