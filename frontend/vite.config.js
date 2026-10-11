import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api/events': 'http://localhost:5000',
      '/api/bookings': 'http://localhost:5001',
    },
  },
});
