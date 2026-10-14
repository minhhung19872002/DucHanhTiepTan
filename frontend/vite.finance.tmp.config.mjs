import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
export default defineConfig({
  root: 'D:/Source/DucHanhTiepTan/frontend',
  plugins: [react()],
  server: { port: 5199, strictPort: true, proxy: { '/api': { target: 'http://localhost:5123', changeOrigin: true } } },
})
