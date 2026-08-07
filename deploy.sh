#!/usr/bin/env bash
# ════════════════════════════════════════════════════════════════
# deploy.sh — TaskManagement System production deploy scripti
# Kullanım: chmod +x deploy.sh && ./deploy.sh [--ssl-init]
# ════════════════════════════════════════════════════════════════
set -euo pipefail
# Renkli çıktı
RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; NC='\033[0m'
log_info()  { echo -e "${GREEN}[INFO]${NC}  $1"; }
log_warn()  { echo -e "${YELLOW}[WARN]${NC}  $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; exit 1; }
# ── Ön Kontroller ─────────────────────────────────────────────────────
log_info "Deploy başlatılıyor..."
command -v docker  >/dev/null || log_error "Docker kurulu değil!"
command -v openssl >/dev/null || log_error "OpenSSL kurulu değil!"
[[ -f .env ]] || log_error ".env dosyası bulunamadı! cp .env.example .env yapıp doldurun."
# .env yükle
set -a; source .env; set +a
# JWT secret güvenlik kontrolü
JWT_LEN=${#JWT_SECRET}
[[ $JWT_LEN -ge 32 ]] || log_error "JWT_SECRET en az 32 karakter olmalı! (Şu an: $JWT_LEN)"
[[ "$JWT_SECRET" != "CHANGE_ME"* ]] || log_error "JWT_SECRET değiştirilmedi! Gerçek değer girin."
[[ "$POSTGRES_PASSWORD" != "CHANGE_ME"* ]] || log_error "POSTGRES_PASSWORD değiştirilmedi!"
log_info "Güvenlik kontrolleri geçti ✓"
# ── SSL Başlangıç Kurulumu (ilk deploy) ───────────────────────────────
if [[ "${1:-}" == "--ssl-init" ]]; then
    log_info "Let's Encrypt SSL sertifikası alınıyor: $DOMAIN"
    docker compose up -d nginx
    sleep 3
    docker compose run --rm certbot certonly \
        --webroot \
        --webroot-path=/var/www/certbot \
        --email "$CERTBOT_EMAIL" \
        --agree-tos \
        --no-eff-email \
        -d "$DOMAIN" \
        -d "www.$DOMAIN"
    log_info "SSL sertifikası alındı ✓"
fi
# ── Deploy Adımları ────────────────────────────────────────────────────
log_info "1/4 — Eski container'lar durduruluyor..."
docker compose down --remove-orphans
log_info "2/4 — Image'lar build ediliyor..."
docker compose build --no-cache --pull
log_info "3/4 — Migration uygulanıyor..."
docker compose run --rm backend dotnet ef database update \
    --project /app/TaskManagement.Infrastructure.dll 2>/dev/null \
    || log_warn "EF migration doğrudan çalışmıyor — manuel migration gerekebilir"
log_info "4/4 — Servisler başlatılıyor..."
docker compose up -d
# ── Sağlık Kontrolü ────────────────────────────────────────────────────
log_info "Servisler başlatılıyor, 15 saniye bekleniyor..."
sleep 15
HEALTH=$(curl -sf "http://localhost/health" 2>/dev/null && echo "OK" || echo "FAIL")
if [[ "$HEALTH" == "OK" ]]; then
    log_info "Deployment başarılı! ✓"
    log_info "URL: https://$DOMAIN"
else
    log_warn "Health check başarısız. docker compose logs ile inceleyin."
fi
# ── Özet ───────────────────────────────────────────────────────────────
echo ""
echo "════════════════════════════════════════"
echo " Çalışan Servisler:"
docker compose ps --format "table {{.Name}}\t{{.Status}}\t{{.Ports}}"
echo "════════════════════════════════════════"
