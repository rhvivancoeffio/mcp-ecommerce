import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import { resolve } from 'path';

export default defineConfig({
  plugins: [react(), tailwindcss()],
  build: {
    outDir: resolve(__dirname, '../../Backend/Server/wwwroot'),
    emptyOutDir: true,
    rollupOptions: {
      input: {
        main: resolve(__dirname, 'index.html'),
        catalog: resolve(__dirname, 'src/widgets/catalog.tsx'),
        'product-comparison': resolve(__dirname, 'src/widgets/product-comparison.tsx'),
        cart: resolve(__dirname, 'src/widgets/cart.tsx'),
      },
      output: {
        entryFileNames: (chunkInfo) => {
          if (chunkInfo.name === 'main') {
            return 'assets/[name]-[hash].js';
          }
          return 'widgets/[name]-[hash].js';
        },
        chunkFileNames: 'widgets/[name]-[hash].js',
        assetFileNames: (assetInfo) => {
          if (assetInfo.name?.endsWith('.html')) {
            return 'widgets/[name]-[hash][extname]';
          }
          return 'assets/[name]-[hash][extname]';
        },
      },
    },
  },
  base: '/',
  resolve: {
    dedupe: ['react', 'react-dom'],
  },
});
