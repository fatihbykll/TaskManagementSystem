// Production environment — apiUrl sunucuda nginx üzerinden gelir
// DOMAIN değişkeni deploy.sh tarafından inject edilir
export const environment = {
  production: true,
  apiUrl: '/api'   // Relative URL — nginx proxy üzerinden aynı domain
};
