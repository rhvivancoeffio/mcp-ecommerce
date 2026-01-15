import React from 'react';
import { ImageCarousel } from './ImageCarousel';

interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  sku: string;
  category: string;
  brand?: string;
  sellerName?: string;
  shopKey?: string;
  imageUrl?: string; // Deprecated: Use imageUrls instead
  imageUrls?: string[];
  stock: number;
  attributes: Record<string, string>;
  features?: string[];
}

interface ProductDetailModalProps {
  product: Product | null;
  quantity: number;
  onClose: () => void;
  onQuantityChange: (delta: number) => void;
  onAddToCart?: () => void;
}

export const ProductDetailModal: React.FC<ProductDetailModalProps> = ({
  product,
  quantity,
  onClose,
  onQuantityChange,
  onAddToCart,
}) => {
  if (!product) return null;

  const isOutOfStock = product.stock === 0;
  const packageInfo = product.attributes['Package'] || '';
  const pricePerUnit = product.attributes['PricePerUnit'] || '';

  // Extract nutritional info from attributes
  const nutritionKeys = ['Fiber', 'Fat', 'Potassium', 'Calories', 'Protein', 'Carbs'];
  const nutritionInfo = nutritionKeys
    .filter(key => product.attributes[key])
    .map(key => ({
      label: key,
      value: product.attributes[key],
    }));

  return (
    <div
      className="fixed inset-0 z-50 flex items-start justify-center bg-black/50 p-4 overflow-y-auto"
      onClick={onClose}
      style={{ paddingTop: '2rem', paddingBottom: '2rem' }}
    >
      <div
        className="relative w-full max-w-2xl max-h-[calc(100vh-4rem)] rounded-2xl bg-white shadow-xl my-auto flex flex-col overflow-hidden"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Close Button */}
        <button
          onClick={onClose}
          className="absolute right-4 top-4 z-10 flex h-8 w-8 items-center justify-center rounded-full bg-white/90 text-blue-600 transition-colors hover:bg-white"
          aria-label="Close modal"
        >
          <svg
            className="h-5 w-5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M6 18L18 6M6 6l12 12"
            />
          </svg>
        </button>

        {/* Product Image Carousel */}
        <div className="relative w-full bg-gradient-to-b from-gray-50 to-white flex-shrink-0">
          <div className="aspect-square w-full max-h-[400px] mx-auto">
            <ImageCarousel
              images={
                product.imageUrls && product.imageUrls.length > 0
                  ? product.imageUrls
                  : product.imageUrl && product.imageUrl.trim() !== ''
                  ? [product.imageUrl]
                  : []
              }
              alt={product.name}
              className="h-full w-full"
              showIndicators={true}
              showNavigation={true}
              autoPlay={false}
            />
          </div>
        </div>

        {/* Product Content */}
        <div className="p-6 overflow-y-auto flex-1 min-h-0">
          {/* Price and Title */}
          <div className="mb-2">
            <div className="flex items-center gap-2 mb-2">
              {product.sellerName && (
                <span className="text-xs px-2 py-1 rounded bg-blue-50 text-blue-700 font-semibold">
                  {product.sellerName}
                </span>
              )}
              {product.brand && product.brand.trim() !== '' && (
                <span className="text-xs px-2 py-1 rounded bg-gray-100 text-gray-700 font-medium">
                  {product.brand}
                </span>
              )}
            </div>
            <p className="text-2xl font-semibold text-slate-900">${product.price.toFixed(2)}</p>
            <h2 className="mt-1 text-2xl font-semibold text-slate-900">{product.name}</h2>
          </div>

          {/* Description */}
          <p className="mb-4 text-sm leading-relaxed text-black/70">
            {product.description}
          </p>

          {/* Package/Price Info */}
          {(packageInfo || pricePerUnit) && (
            <p className="mb-4 text-sm text-black/60">
              {packageInfo && <span>{packageInfo}</span>}
              {packageInfo && pricePerUnit && <span> • </span>}
              {pricePerUnit && <span>{pricePerUnit}</span>}
            </p>
          )}

          {/* Features */}
          {product.features && product.features.length > 0 && (
            <div className="mb-4">
              <h3 className="mb-2 text-sm font-semibold text-slate-900">Features</h3>
              <ul className="space-y-1.5">
                {product.features.map((feature, index) => (
                  <li key={index} className="flex items-start text-sm text-black/70">
                    <svg
                      className="mr-2 mt-0.5 h-4 w-4 flex-shrink-0 text-blue-600"
                      fill="currentColor"
                      viewBox="0 0 20 20"
                    >
                      <path
                        fillRule="evenodd"
                        d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                        clipRule="evenodd"
                      />
                    </svg>
                    <span>{feature}</span>
                  </li>
                ))}
              </ul>
            </div>
          )}

          {/* Quantity Selector */}
          <div className="mb-6 flex items-center justify-between gap-4">
            <div className="flex items-center rounded-full bg-black/[0.04] px-2 py-1.5 border border-gray-200">
              <button
                type="button"
                className={`flex h-8 w-8 items-center justify-center rounded-full transition-colors ${
                  quantity === 0 || isOutOfStock
                    ? 'opacity-30 cursor-not-allowed'
                    : 'opacity-50 hover:bg-slate-200 hover:opacity-100'
                }`}
                aria-label="Decrease quantity"
                onClick={() => onQuantityChange(-1)}
                disabled={quantity === 0 || isOutOfStock}
              >
                <span className="text-lg font-medium text-slate-900">−</span>
              </button>
              <span className="min-w-[32px] px-2 text-center text-base font-semibold text-slate-900">
                {quantity}
              </span>
              <button
                type="button"
                className={`flex h-8 w-8 items-center justify-center rounded-full transition-colors ${
                  quantity >= product.stock || isOutOfStock
                    ? 'opacity-30 cursor-not-allowed'
                    : 'opacity-50 hover:bg-slate-200 hover:opacity-100'
                }`}
                aria-label="Increase quantity"
                onClick={() => onQuantityChange(1)}
                disabled={quantity >= product.stock || isOutOfStock}
              >
                <span className="text-lg font-medium text-slate-900">+</span>
              </button>
            </div>

            <div className="flex items-center gap-2">
              {/* Add to Cart Button */}
              {quantity > 0 && onAddToCart && (
                <button
                  type="button"
                  onClick={onAddToCart}
                  className="bg-black text-white font-semibold py-2.5 px-6 rounded-lg hover:bg-black/90 transition-colors text-base"
                >
                  Agregar al Carrito
                </button>
              )}

              {/* Stock Badge */}
              {isOutOfStock && (
                <span className="text-sm font-medium text-red-600">Out of Stock</span>
              )}
            </div>
          </div>

          {/* Nutritional Information */}
          {nutritionInfo.length > 0 && (
            <div className="mb-4 border-t border-black/5 pt-4">
              <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
                {nutritionInfo.map(({ label, value }) => (
                  <div key={label} className="text-center">
                    <p className="text-lg font-semibold text-slate-900">{value}</p>
                    <p className="text-xs text-black/60">{label}</p>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Additional Description */}
          {product.description && product.description.length > 100 && (
            <p className="text-sm leading-relaxed text-black/60">
              {product.description.split('.').slice(1).join('.').trim()}
            </p>
          )}
        </div>
      </div>
    </div>
  );
};
