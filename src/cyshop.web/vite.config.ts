import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// When running under Aspire, service URLs are injected as env vars.
// Fall back to the default HTTP ports for standalone dev.
const catalogTarget = process.env.services__catalog_api__https__0 ?? process.env.services__catalog_api__http__0 ?? 'http://localhost:5146'
const basketTarget = process.env.services__basket_api__https__0 ?? process.env.services__basket_api__http__0 ?? 'https://localhost:7097'
const customersTarget = process.env.services__customers_api__https__0 ?? process.env.services__customers_api__http__0 ?? 'https://localhost:7261'
const ordersTarget = process.env.services__orders_api__https__0 ?? process.env.services__orders_api__http__0 ?? 'https://localhost:7042'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api/catalog': {
        target: catalogTarget,
        changeOrigin: true,
        secure: false, // allow self-signed certs in dev
      },
      '/api/basket': {
        target: basketTarget,
        changeOrigin: true,
        secure: false,
      },
      '/api/customers': {
        target: customersTarget,
        changeOrigin: true,
        secure: false,
      },
      '/api/orders': {
        target: ordersTarget,
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
