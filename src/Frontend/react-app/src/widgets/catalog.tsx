// Import CSS - Tailwind 4 handles directives automatically via Vite plugin
import './catalog.css';

import React from 'react';
import { createRoot } from 'react-dom/client';
import { CatalogWidget } from '../components/widgets/CatalogWidget';

// Inicializar window.openai si no existe
if (typeof window !== 'undefined') {
  (window as any).openai = (window as any).openai || {};
}

// Renderizar el widget
const rootElement = document.getElementById('root');
if (rootElement) {
  const root = createRoot(rootElement);
  root.render(
    <React.StrictMode>
      <CatalogWidget />
    </React.StrictMode>
  );
}
