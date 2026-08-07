# ── Aşama 1: Angular Build ─────────────────────────────────────────────
FROM node:22-alpine AS build
WORKDIR /app
# package dosyaları önce → bağımlılık cache optimizasyonu
COPY package*.json ./
RUN npm ci --prefer-offline --ignore-scripts
COPY . .
RUN npm run build -- --configuration production
# ── Aşama 2: Nginx ─────────────────────────────────────────────────────
FROM nginx:1.27-alpine AS runtime
# Angular dist → nginx html root
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
# SPA routing config
COPY nginx/nginx.conf /etc/nginx/conf.d/default.conf
# Güvenlik: nginx non-root
RUN chown -R nginx:nginx /usr/share/nginx/html \
    && chmod -R 755 /usr/share/nginx/html
EXPOSE 80
HEALTHCHECK --interval=30s --timeout=5s \
  CMD curl -f http://localhost/ || exit 1
