import React, { useState, useEffect } from 'react';

interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  sku: string;
  category: string;
  imageUrl: string;
  stock: number;
  attributes: Record<string, string>;
}

const Catalog: React.FC = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [category, setCategory] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState<string>('');

  useEffect(() => {
    // TODO: Implement MCP client to call catalog_list tool
    // For now, using mock data
    const mockProducts: Product[] = [
      {
        id: '00000000-0000-0000-0000-000000000001',
        name: 'Laptop Pro 15',
        description: 'High-performance laptop with latest processor',
        price: 1299.99,
        sku: 'LAP-001',
        category: 'Electronics',
        imageUrl: '/images/laptop-pro-15.jpg',
        stock: 25,
        attributes: {
          Processor: 'Intel i7',
          RAM: '16GB',
          Storage: '512GB SSD',
          Screen: '15.6 inch'
        }
      },
      {
        id: '00000000-0000-0000-0000-000000000002',
        name: 'Wireless Mouse',
        description: 'Ergonomic wireless mouse with long battery life',
        price: 29.99,
        sku: 'MOU-001',
        category: 'Accessories',
        imageUrl: '/images/wireless-mouse.jpg',
        stock: 150,
        attributes: {
          Connectivity: 'Bluetooth 5.0',
          Battery: '12 months',
          DPI: '1600'
        }
      }
    ];
    setProducts(mockProducts);
    setLoading(false);
  }, [category, searchTerm]);

  if (loading) {
    return <div>Loading...</div>;
  }

  return (
    <div className="catalog">
      <div className="catalog-filters">
        <input
          type="text"
          placeholder="Search products..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
        <select
          value={category}
          onChange={(e) => setCategory(e.target.value)}
        >
          <option value="">All Categories</option>
          <option value="Electronics">Electronics</option>
          <option value="Accessories">Accessories</option>
        </select>
      </div>
      <div className="product-grid">
        {products.map((product) => (
          <div key={product.id} className="product-card">
            <h3>{product.name}</h3>
            <p>{product.description}</p>
            <p className="price">${product.price.toFixed(2)}</p>
            <p className="stock">Stock: {product.stock}</p>
            <p>SKU: {product.sku}</p>
            <p>Category: {product.category}</p>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Catalog;
