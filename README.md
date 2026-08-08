# 📋 Görev Yönetim Sistemi (Task Management System)

> Milsoft Yazılım Teknolojileri — Yaz Stajı Projesi 2026

Görev Yönetim Sistemi; görevleri oluşturma, kategorize etme, yorum yapma, dosya ekleme ve kanban panosunda sürükle-bırak ile yönetmeyi sağlayan full-stack bir web uygulamasıdır.

---

## Teknoloji Yığını

### Backend
| Teknoloji | Açıklama |
|---|---|
| **.NET 10 Web API** | REST API katmanı |
| **Clean Architecture** | Domain / Application / Infrastructure / API |
| **Entity Framework Core** | ORM — Code First, Migration |
| **PostgreSQL** | Ana veritabanı |
| **Oracle EF Core** | Çoklu veritabanı desteği |
| **JWT Bearer Auth** | Rol tabanlı kimlik doğrulama (User / Admin) |
| **AutoMapper** | DTO ↔ Entity dönüşümleri |
| **Serilog** | Yapısal loglama (Console + File) |
| **xUnit + FluentAssertions** | 34 entegrasyon testi |

### Frontend
| Teknoloji | Açıklama |
|---|---|
| **Angular 18+** | SPA framework |
| **Angular Material** | UI bileşen kütüphanesi |
| **CDK Drag & Drop** | Kanban sürükle-bırak |
| **RxJS** | Reaktif programlama |
| **xlsx + jsPDF** | Excel ve PDF dışa aktarma |

### DevOps
| Teknoloji | Açıklama |
|---|---|
| **Docker + Docker Compose** | Konteyner orkestrasyonu |
| **Nginx** | Reverse proxy + SSL sonlandırma |
| **Let's Encrypt** | Ücretsiz SSL sertifikası |

---

## Özellikler

- ✅ **Kullanıcı Yönetimi** — Kayıt, giriş, JWT token yenileme
- ✅ **Rol Tabanlı Yetkilendirme** — User ve Admin rolleri
- ✅ **Görev CRUD** — Oluşturma, düzenleme, silme, durum güncelleme
- ✅ **Kategori Yönetimi** — Renk kodlu kategori oluşturma ve atama
- ✅ **Kanban Panosu** — 4 sütunlu sürükle-bırak (CDK)
- ✅ **Dosya Yükleme** — Whitelist koruması (pdf/jpg/png/txt), 10 MB limit
- ✅ **Yorum Sistemi** — Göreve yorum ekleme ve listeleme
- ✅ **Gelişmiş Filtreleme** — Arama, durum, öncelik, sayfalama
- ✅ **Dışa Aktarma** — Excel (.xlsx) ve PDF çıktısı
- ✅ **Dark Mode** — Sistem temasına uyumlu koyu mod
- ✅ **Mobil Uyumlu** — 4 breakpoint responsive tasarım
- ✅ **Admin Paneli** — Sistem istatistikleri ve kullanıcı listesi

---

## Proje Yapısı

```text
TaskManagementSystem/
├── Backend/
│   ├── TaskManagement.Domain/          # Entity, Enum, Interface
│   ├── TaskManagement.Application/     # DTO, Service, Interface, Mapping
│   ├── TaskManagement.Infrastructure/  # EF Core, Repository, JWT, Migration
│   ├── TaskManagement.API/             # Controller, Middleware, Program.cs
│   └── TaskManagement.Tests/           # xUnit entegrasyon testleri (34 test)
├── Frontend/
│   └── src/app/
│       ├── core/                       # Servisler, Guard, Interceptor, Model
│       ├── features/                   # Auth, Dashboard, Tasks (CRUD, Board, Detail)
│       └── layout/                     # Navbar, Sidebar
├── Infrastructure/
│   └── nginx/nginx.conf                # Reverse proxy + SSL + Security Headers
├── docker-compose.yml
├── deploy.sh
└── .env.example
```

---

## Hızlı Başlangıç (Local Geliştirme)

### Gereksinimler
- .NET 10 SDK
- Node.js 22+
- PostgreSQL 16
- Docker (opsiyonel)

### 1. Veritabanı

```bash
# PostgreSQL veritabanı oluştur
createdb TaskManagementDb

# Backend'e gir ve migration uygula
cd Backend
dotnet ef database update \
  --project TaskManagement.Infrastructure \
  --startup-project TaskManagement.API
```

### 2. Backend

```bash
cd Backend/TaskManagement.API

# Geliştirici secret'larını ayarla
dotnet user-secrets set "ConnectionStrings:PostgresConnection" \
  "Host=localhost;Database=TaskManagementDb;Username=postgres;Password=yourpassword"

dotnet user-secrets set "JwtSettings:SecretKey" "your-32-char-minimum-secret-key"
dotnet user-secrets set "AdminEmail" "admin@example.com"

# Çalıştır
dotnet run
# API → http://localhost:5116
# Swagger → http://localhost:5116/swagger
```

### 3. Frontend

```bash
cd Frontend
npm install
npm run start
# Uygulama → http://localhost:4200
```

---

## Docker ile Production Deploy

```bash
# 1. Secret'ları ayarla
cp .env.example .env
# .env dosyasını düzenleyip gerçek değerleri gir
# JWT_SECRET için: openssl rand -hex 32

# 2. İlk deploy (SSL dahil)
chmod +x deploy.sh
./deploy.sh --ssl-init

# 3. Sonraki deploylar
./deploy.sh
```

### Çalışan Servisler

| Servis | Adres | Açıklama |
|---|---|---|
| Nginx | :80 / :443 | Reverse proxy + SSL |
| Frontend | internal | Angular SPA |
| Backend | internal | .NET API |
| PostgreSQL | internal | Veritabanı (dışa kapalı) |

---

## Güvenlik Özellikleri

| Katman | Önlem |
|---|---|
| **Secret Yönetimi** | `.env` + Docker env vars (asla commit edilmez) |
| **TLS/SSL** | Let's Encrypt, TLS 1.2/1.3, HSTS (1 yıl) |
| **HTTP Headers** | CSP, X-Frame-Options, X-Content-Type-Options |
| **Rate Limiting** | Auth: 10 istek/dk, API: 30 istek/dk |
| **JWT** | HS256 imzalı, rol claim dahil |
| **Dosya Yükleme** | Uzantı + MIME çift kontrolü, 10 MB limit |
| **Ağ İzolasyonu** | PostgreSQL yalnızca internal Docker network'te |

---

## Testleri Çalıştırma

```bash
cd Backend
dotnet test TaskManagement.Tests/TaskManagement.Tests.csproj \
  --logger "console;verbosity=quiet"

# Sonuç: 34/34 test başarılı
# AuthTests (10) | TaskCrudTests (11) | AttachmentSecurityTests (8) | RoleAuthorizationTests (5)
```

---

## Geliştirici

**Fatih Bıyıklı**  
Milsoft Yazılım Teknolojileri — Yaz Stajı 2026
