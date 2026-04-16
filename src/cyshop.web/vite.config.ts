import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api/catalog': {
        target: 'http://localhost:5146',
        changeOrigin: true,
      },
      '/api/basket': {
        target: 'http://localhost:5167',
        changeOrigin: true,
      },
    },
  },
})
