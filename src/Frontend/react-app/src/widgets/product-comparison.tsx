import React from 'react';
import { createRoot } from 'react-dom/client';
import { ProductComparisonWidget } from '../components/widgets/ProductComparisonWidget';

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
      <ProductComparisonWidget />
    </React.StrictMode>
  );
}
