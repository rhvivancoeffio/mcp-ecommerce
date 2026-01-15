import React from 'react';
import { useWidgetProps } from '../../hooks/useWidgetProps';

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

interface ProductComparisonWidgetProps {
  products?: Product[];
  comparisonData?: Record<string, Record<string, any>>;
}

export const ProductComparisonWidget: React.FC = () => {
  // Obtener props reactivamente desde window.openai.toolOutput.structuredContent
  const widgetProps = useWidgetProps<ProductComparisonWidgetProps>();
  
  const products = widgetProps?.products ?? [];
  const comparisonData = widgetProps?.comparisonData ?? {};
  if (products.length === 0) {
    return (
      <div className="w-full p-4 text-center">
        <p className="text-gray-500">No products to compare</p>
      </div>
    );
  }

  const comparisonKeys = Object.keys(comparisonData);
  
  return (
    <div className="w-full p-4">
      <h1 className="text-2xl font-bold mb-4">Product Comparison</h1>
      
      <div className="overflow-x-auto">
        <table className="w-full border-collapse">
          <thead>
            <tr className="border-b border-gray-200">
              <th className="text-left p-3 font-medium text-gray-500">Feature</th>
              {products.map((product) => (
                <th key={product.id} className="text-center p-3 font-medium">
                  {product.name}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {comparisonKeys.map((key) => (
              <tr key={key} className="border-b border-gray-200">
                <td className="p-3 font-medium text-gray-500">{key}</td>
                {products.map((product) => (
                  <td key={product.id} className="text-center p-3">
                    {comparisonData[key]?.[product.id] !== undefined 
                      ? String(comparisonData[key][product.id])
                      : '-'}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      
      <div className="mt-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {products.map((product) => (
          <div key={product.id} className="border border-gray-200 rounded-lg p-4 bg-white shadow-sm">
            <h3 className="text-lg font-semibold mb-2">{product.name}</h3>
            <p className="text-gray-500 text-sm mb-3">{product.description}</p>
            <div className="space-y-2">
              <div className="flex justify-between">
                <span className="text-gray-500 text-sm">Price:</span>
                <span className="font-medium">${product.price.toFixed(2)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-500 text-sm">Stock:</span>
                <span className={`px-2 py-1 rounded text-xs ${product.stock > 0 ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                  {product.stock}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-500 text-sm">Category:</span>
                <span>{product.category}</span>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};
