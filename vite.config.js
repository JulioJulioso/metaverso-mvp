import { defineConfig } from 'vite';
import { resolve } from 'path';

// Project Pages: https://juliojulioso.github.io/metaverso-mvp/
// Local dev still works with this base (Vite handles it).
export default defineConfig({
  base: '/metaverso-mvp/',
  root: '.',
  publicDir: 'public',
  build: {
    outDir: 'dist',
    emptyOutDir: true,
    assetsDir: 'assets',
  },
  server: {
    port: 5173,
    open: false,
  },
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
    },
  },
});
