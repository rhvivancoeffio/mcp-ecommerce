import React from 'react';
import Catalog from './components/Catalog';
import './App.css';

function App() {
  return (
    <div className="app">
      <header className="app-header">
        <h1>MCP Ecommerce</h1>
      </header>
      <main>
        <Catalog />
      </main>
    </div>
  );
}

export default App;
