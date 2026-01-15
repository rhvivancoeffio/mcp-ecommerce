import React, { useState, useMemo, useEffect, useCallback } from 'react';
import { Button } from '@openai/apps-sdk-ui/components/Button';
import { EmptyMessage } from '@openai/apps-sdk-ui/components/EmptyMessage';
import { useWidgetProps } from '../../hooks/useWidgetProps';
import { useWidgetState } from '../../hooks/useWidgetState';
import { useMaxHeight } from '../../hooks/useMaxHeight';
import { useDisplayMode } from '../../hooks/useDisplayMode';
import { ProductDetailModal } from './ProductDetailModal';
import { CartModal } from './CartModal';
import { ImageCarousel } from './ImageCarousel';

interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  sku: string;
  category: string;
  brand: string;
  sellerName?: string;
  shopKey?: string;
  imageUrl?: string; // Deprecated: Use imageUrls instead
  imageUrls?: string[];
  stock: number;
  attributes: Record<string, string>;
  features?: string[];
}

interface CatalogWidgetProps extends Record<string, unknown> {
  products?: Product[];
  totalCount?: number;
  page?: number;
  pageSize?: number;
  status?: "loading" | "completed" | "error";
  widgetState?: {
    selectedCategory?: string;
    quantities?: Record<string, number>;
  };
}

interface CartItem {
  productId: string;
  productName: string;
  productSku: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  imageUrl?: string;
}

interface CatalogWidgetState extends Record<string, unknown> {
  selectedCategory?: string;
  selectedBrand?: string;
  minPrice?: number;
  maxPrice?: number;
  quantities?: Record<string, number>;
  cartItems?: CartItem[];
}

const createDefaultWidgetState = (): CatalogWidgetState => ({
  selectedCategory: 'All',
  selectedBrand: 'All',
  minPrice: undefined,
  maxPrice: undefined,
  quantities: {},
  cartItems: [],
});

export const CatalogWidget: React.FC = () => {
  // Layout hooks
  const maxHeight = useMaxHeight() ?? undefined;
  const displayMode = useDisplayMode();

  // Widget props and state
  const widgetProps = useWidgetProps<CatalogWidgetProps>(() => ({}));
  const [widgetState, setWidgetState] = useWidgetState<CatalogWidgetState>(
    createDefaultWidgetState
  );

  // Resolve products with priority: widgetState > widgetProps.widgetState > widgetProps > default
  const resolvedProducts = useMemo(() => {
    // Extract structuredContent if available
    const structuredContent = widgetProps && 'structuredContent' in widgetProps
      ? (widgetProps as CatalogWidgetProps & { structuredContent?: CatalogWidgetProps }).structuredContent
      : widgetProps;

    return structuredContent?.products ?? widgetProps?.products ?? [];
  }, [widgetProps]);

  // Resolve status from structuredContent or widgetProps
  const resolvedStatus = useMemo(() => {
    const structuredContent = widgetProps && 'structuredContent' in widgetProps
      ? (widgetProps as CatalogWidgetProps & { structuredContent?: CatalogWidgetProps }).structuredContent
      : widgetProps;

    return structuredContent?.status ?? widgetProps?.status ?? "loading";
  }, [widgetProps]);

  // Update loading state based on status
  useEffect(() => {
    // Only stop loading when status is "completed" AND we have widgetProps (meaning data has arrived)
    // If status is "completed" but no products, it means the response was empty (no products match filters)
    if (resolvedStatus === "completed" && widgetProps && Object.keys(widgetProps).length > 0) {
      setIsLoading(false);
    } else if (resolvedStatus === "loading") {
      setIsLoading(true);
    }
    // If status is undefined or not set, keep current loading state
  }, [resolvedStatus, widgetProps]);


  // Resolve selected category with priority
  const resolvedSelectedCategory = useMemo(() => {
    return widgetState?.selectedCategory ??
           widgetProps?.widgetState?.selectedCategory ??
           'All';
  }, [widgetState?.selectedCategory, widgetProps?.widgetState?.selectedCategory]);

  // Resolve quantities with priority
  const resolvedQuantities = useMemo(() => {
    return widgetState?.quantities ??
           widgetProps?.widgetState?.quantities ??
           {};
  }, [widgetState?.quantities, widgetProps?.widgetState?.quantities]);

  // Local state that syncs with resolved values
  const [selectedCategory, setSelectedCategory] = useState<string>(resolvedSelectedCategory);
  const [selectedBrand, setSelectedBrand] = useState<string>('All');
  const [minPrice, setMinPrice] = useState<number | undefined>(undefined);
  const [maxPrice, setMaxPrice] = useState<number | undefined>(undefined);
  const [quantities, setQuantities] = useState<Record<string, number>>(resolvedQuantities);
  // Pending quantities (temporary selection before adding to cart)
  const [pendingQuantities, setPendingQuantities] = useState<Record<string, number>>({});
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [isCartOpen, setIsCartOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  
  // Calculate cart items from quantities
  const cartItems = useMemo(() => {
    const items: CartItem[] = [];
    Object.entries(quantities).forEach(([productId, quantity]) => {
      if (quantity > 0) {
        const product = resolvedProducts.find(p => p.id === productId);
        if (product) {
          items.push({
            productId: product.id,
            productName: product.name,
            productSku: product.sku,
            quantity,
            unitPrice: product.price,
            totalPrice: product.price * quantity,
            imageUrl: product.imageUrl,
          });
        }
      }
    });
    return items;
  }, [quantities, resolvedProducts]);

  // Calculate total items in cart
  const totalCartItems = useMemo(() => {
    return cartItems.reduce((sum, item) => sum + item.quantity, 0);
  }, [cartItems]);

  // Sync local state with resolved values
  useEffect(() => {
    setSelectedCategory((prev) => prev === resolvedSelectedCategory ? prev : resolvedSelectedCategory);
  }, [resolvedSelectedCategory]);

  useEffect(() => {
    setQuantities((prev) => {
      // Only update if different
      const prevStr = JSON.stringify(prev);
      const resolvedStr = JSON.stringify(resolvedQuantities);
      return prevStr === resolvedStr ? prev : resolvedQuantities;
    });
  }, [resolvedQuantities]);

  // Get categories: first from initialData, then from products
  const categories = useMemo(() => {
    // Try to get from window.openai.initialData first
    const initialData = (window as any).openai?.initialData;
    if (initialData?.categories && Array.isArray(initialData.categories) && initialData.categories.length > 0) {
      return ['All', ...initialData.categories];
    }
    // Fallback to extracting from products
    return ['All', ...Array.from(new Set(resolvedProducts.map(p => p.category)))];
  }, [resolvedProducts]);

  // Get brands: first from initialData, then from products
  const brands = useMemo(() => {
    // Try to get from window.openai.initialData first
    const initialData = (window as any).openai?.initialData;
    if (initialData?.brands && Array.isArray(initialData.brands) && initialData.brands.length > 0) {
      return ['All', ...initialData.brands];
    }
    // Fallback to extracting from products
    return ['All', ...Array.from(new Set(resolvedProducts.map(p => p.brand).filter(b => b && b.trim() !== '')))];
  }, [resolvedProducts]);

  // Calculate price range from products
  const priceBounds = useMemo(() => {
    if (resolvedProducts.length === 0) return [0, 1000];
    const prices = resolvedProducts.map(p => p.price);
    const min = Math.floor(Math.min(...prices));
    const max = Math.ceil(Math.max(...prices));
    return [min, max];
  }, [resolvedProducts]);

  // Initialize price range if not set
  useEffect(() => {
    if (minPrice === undefined && maxPrice === undefined && priceBounds[0] !== priceBounds[1]) {
      setMinPrice(priceBounds[0]);
      setMaxPrice(priceBounds[1]);
    }
  }, [priceBounds, minPrice, maxPrice]);

  // Filter products by selected filters
  const filteredProducts = useMemo(() => {
    let filtered = resolvedProducts;

    // Filter by category
    if (selectedCategory !== 'All') {
      filtered = filtered.filter(p => p.category === selectedCategory);
    }

    // Filter by brand
    if (selectedBrand !== 'All') {
      filtered = filtered.filter(p => p.brand === selectedBrand);
    }

    // Filter by price range
    if (minPrice !== undefined) {
      filtered = filtered.filter(p => p.price >= minPrice);
    }
    if (maxPrice !== undefined) {
      filtered = filtered.filter(p => p.price <= maxPrice);
    }

    return filtered;
  }, [resolvedProducts, selectedCategory, selectedBrand, minPrice, maxPrice]);

  // Update pending quantity (temporary selection)
  const updatePendingQuantity = useCallback((productId: string, delta: number) => {
    setPendingQuantities(prev => {
      const current = prev[productId] || 0;
      const product = resolvedProducts.find(p => p.id === productId);
      const newQuantity = Math.max(0, Math.min(current + delta, product?.stock || 0));
      return { ...prev, [productId]: newQuantity };
    });
  }, [resolvedProducts]);

  // Update cart quantity (actual cart)
  const updateQuantity = useCallback((productId: string, delta: number) => {
    setQuantities(prev => {
      const current = prev[productId] || 0;
      const product = resolvedProducts.find(p => p.id === productId);
      const newQuantity = Math.max(0, Math.min(current + delta, product?.stock || 0));
      const updated = { ...prev, [productId]: newQuantity };
      
      // Persist to widget state
      setWidgetState(prevState => ({
        ...prevState,
        quantities: updated,
      }));
      
      return updated;
    });
  }, [resolvedProducts, setWidgetState]);

  const handleCategoryChange = useCallback((category: string) => {
    setSelectedCategory(category);
    setWidgetState(prevState => ({
      ...prevState,
      selectedCategory: category,
    }));
  }, [setWidgetState]);

  const handleBrandChange = useCallback((brand: string) => {
    setSelectedBrand(brand);
    setWidgetState(prevState => ({
      ...prevState,
      selectedBrand: brand,
    }));
  }, [setWidgetState]);

  const handleMinPriceChange = useCallback((value: number) => {
    const newMinPrice = Math.min(value, maxPrice ?? priceBounds[1]);
    setMinPrice(newMinPrice);
    setWidgetState(prevState => ({
      ...prevState,
      minPrice: newMinPrice,
    }));
  }, [maxPrice, priceBounds, setWidgetState]);

  const handleMaxPriceChange = useCallback((value: number) => {
    const newMaxPrice = Math.max(value, minPrice ?? priceBounds[0]);
    setMaxPrice(newMaxPrice);
    setWidgetState(prevState => ({
      ...prevState,
      maxPrice: newMaxPrice,
    }));
  }, [minPrice, priceBounds, setWidgetState]);

  const handleProductClick = useCallback((product: Product) => {
    setSelectedProduct(product);
  }, []);

  const handleCloseModal = useCallback(() => {
    setSelectedProduct(null);
  }, []);

  const handleModalQuantityChange = useCallback((delta: number) => {
    if (selectedProduct) {
      updatePendingQuantity(selectedProduct.id, delta);
    }
  }, [selectedProduct, updatePendingQuantity]);

  const handleAddToCart = useCallback((productId: string) => {
    const pendingQty = pendingQuantities[productId] || 0;
    if (pendingQty > 0) {
      // Add pending quantity to cart
      updateQuantity(productId, pendingQty);
      // Reset pending quantity for this product
      setPendingQuantities(prev => {
        const updated = { ...prev };
        delete updated[productId];
        return updated;
      });
    }
  }, [pendingQuantities, updateQuantity]);

  const handleCartItemRemove = useCallback((productId: string) => {
    updateQuantity(productId, -quantities[productId] || 0);
  }, [quantities, updateQuantity]);

  const handleCartItemQuantityChange = useCallback((productId: string, delta: number) => {
    updateQuantity(productId, delta);
  }, [updateQuantity]);

  const handleCheckout = useCallback(() => {
    // TODO: Implement checkout logic
    console.log('Checkout with items:', cartItems);
    alert(`Checkout initiated with ${totalCartItems} items!`);
    setIsCartOpen(false);
  }, [cartItems, totalCartItems]);

  return (
    <div 
      className="w-full bg-white flex flex-col"
      style={{
        maxHeight,
        height: displayMode === "fullscreen" ? maxHeight : undefined,
        overflow: "hidden",
      }}
    >
      {/* Header */}
      <div className="flex items-center justify-between px-5 py-3.5 bg-white border-b border-black/10">
        <div className="flex items-center gap-2">
          <h1 className="text-lg font-bold text-black">Catálogo</h1>
          {filteredProducts.length > 0 && (
            <span className="text-sm text-black/50">
              ({filteredProducts.length} {filteredProducts.length === 1 ? 'producto' : 'productos'})
            </span>
          )}
        </div>
        <button
          onClick={() => setIsCartOpen(true)}
          className="relative flex items-center gap-2 px-4 py-2 rounded-lg bg-black/[0.06] text-black/80 hover:bg-black/[0.10] transition-all shadow-sm hover:shadow"
        >
          <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
          </svg>
          <span className="text-sm font-semibold">Carrito</span>
          {totalCartItems > 0 && (
            <span className="absolute -top-1.5 -right-1.5 flex h-5 w-5 items-center justify-center rounded-full bg-blue-600 text-xs font-bold text-white shadow-md">
              {totalCartItems > 99 ? '99+' : totalCartItems}
            </span>
          )}
        </button>
      </div>

      {/* Filters */}
      <div className="px-5 py-4 bg-gradient-to-b from-white to-black/[0.02] border-b border-black/10">
        <div className="flex flex-wrap gap-4 items-start">
          {/* Category Select */}
          <div className="flex-1 min-w-[160px]">
            <label className="block text-xs font-bold text-black/80 mb-2 uppercase tracking-wider">
              Categoría
            </label>
            <select
              value={selectedCategory}
              onChange={(e) => handleCategoryChange(e.target.value)}
              className="w-full px-4 py-2.5 text-sm font-medium border-2 border-black/15 rounded-lg bg-white text-black shadow-sm hover:border-black/30 focus:outline-none focus:ring-2 focus:ring-black/15 focus:border-black/40 transition-all appearance-none cursor-pointer"
              style={{
                backgroundImage: `url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='%23374151' stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M6 8l4 4 4-4'/%3e%3c/svg%3e")`,
                backgroundPosition: 'right 0.75rem center',
                backgroundRepeat: 'no-repeat',
                backgroundSize: '1.25em 1.25em',
                paddingRight: '2.75rem'
              }}
            >
              {categories.map((category) => (
                <option key={category} value={category}>
                  {category}
                </option>
              ))}
            </select>
          </div>

          {/* Brand Select */}
          <div className="flex-1 min-w-[160px]">
            <label className="block text-xs font-bold text-black/80 mb-2 uppercase tracking-wider">
              Marca
            </label>
            <select
              value={selectedBrand}
              onChange={(e) => handleBrandChange(e.target.value)}
              className="w-full px-4 py-2.5 text-sm font-medium border-2 border-black/15 rounded-lg bg-white text-black shadow-sm hover:border-black/30 focus:outline-none focus:ring-2 focus:ring-black/15 focus:border-black/40 transition-all appearance-none cursor-pointer"
              style={{
                backgroundImage: `url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='%23374151' stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M6 8l4 4 4-4'/%3e%3c/svg%3e")`,
                backgroundPosition: 'right 0.75rem center',
                backgroundRepeat: 'no-repeat',
                backgroundSize: '1.25em 1.25em',
                paddingRight: '2.75rem'
              }}
            >
              {brands.map((brand) => (
                <option key={brand} value={brand}>
                  {brand}
                </option>
              ))}
            </select>
          </div>

          {/* Price Range Sliders */}
          {minPrice !== undefined && maxPrice !== undefined && (
            <div className="flex-1 min-w-[240px]">
              <label className="block text-xs font-bold text-black/80 mb-3 uppercase tracking-wider">
                Rango de Precios
              </label>
              <div className="space-y-5">
                <div>
                  <div className="flex justify-between items-center mb-2.5">
                    <span className="text-xs font-semibold text-black/70">Mínimo</span>
                    <span className="text-base font-bold text-black">${minPrice.toFixed(2)}</span>
                  </div>
                  <input
                    type="range"
                    value={minPrice}
                    min={priceBounds[0]}
                    max={maxPrice}
                    step={1}
                    onChange={(e) => handleMinPriceChange(Number(e.target.value))}
                    className="w-full h-2 bg-black/10 rounded-full appearance-none cursor-pointer slider"
                    style={{
                      background: `linear-gradient(to right, #000 0%, #000 ${((minPrice - priceBounds[0]) / (maxPrice - priceBounds[0])) * 100}%, rgba(0,0,0,0.1) ${((minPrice - priceBounds[0]) / (maxPrice - priceBounds[0])) * 100}%, rgba(0,0,0,0.1) 100%)`
                    }}
                  />
                </div>
                <div>
                  <div className="flex justify-between items-center mb-2.5">
                    <span className="text-xs font-semibold text-black/70">Máximo</span>
                    <span className="text-base font-bold text-black">${maxPrice.toFixed(2)}</span>
                  </div>
                  <input
                    type="range"
                    value={maxPrice}
                    min={minPrice}
                    max={priceBounds[1]}
                    step={1}
                    onChange={(e) => handleMaxPriceChange(Number(e.target.value))}
                    className="w-full h-2 bg-black/10 rounded-full appearance-none cursor-pointer slider"
                    style={{
                      background: `linear-gradient(to right, #000 0%, #000 ${((maxPrice - minPrice) / (priceBounds[1] - minPrice)) * 100}%, rgba(0,0,0,0.1) ${((maxPrice - minPrice) / (priceBounds[1] - minPrice)) * 100}%, rgba(0,0,0,0.1) 100%)`
                    }}
                  />
                </div>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Products Grid */}
      <section className="flex-1 overflow-y-auto p-5 bg-gradient-to-b from-white to-black/[0.01]">
        {isLoading || (resolvedStatus === "completed" && resolvedProducts.length === 0 && (!widgetProps || Object.keys(widgetProps).length === 0)) ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
            {[...Array(6)].map((_, index) => (
              <div
                key={`skeleton-${index}`}
                className="group relative flex flex-col overflow-hidden rounded-xl border-2 border-black/8 bg-white animate-pulse"
              >
                {/* Skeleton Image */}
                <div className="relative h-64 w-full bg-gray-200"></div>
                
                {/* Skeleton Content */}
                <div className="flex flex-1 flex-col gap-3.5 px-5 pt-4 pb-5">
                  <div className="space-y-1">
                    <div className="flex items-center gap-2 mb-1">
                      <div className="h-5 w-16 bg-gray-200 rounded"></div>
                    </div>
                    <div className="h-6 w-3/4 bg-gray-200 rounded"></div>
                    <div className="h-7 w-24 bg-gray-200 rounded"></div>
                  </div>
                  
                  <div className="h-4 w-full bg-gray-200 rounded"></div>
                  <div className="h-4 w-5/6 bg-gray-200 rounded"></div>
                  
                  {/* Skeleton Quantity and Button */}
                  <div className="flex items-center justify-between gap-3 mt-auto pt-2">
                    <div className="flex items-center rounded-lg bg-gray-200 border border-gray-200 px-2 py-1.5">
                      <div className="h-7 w-7 bg-gray-300 rounded-md"></div>
                      <div className="min-w-[28px] px-2">
                        <div className="h-4 w-4 bg-gray-300 rounded mx-auto"></div>
                      </div>
                      <div className="h-7 w-7 bg-gray-300 rounded-md"></div>
                    </div>
                    <div className="h-8 w-20 bg-gray-200 rounded-lg"></div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        ) : resolvedStatus === "completed" && resolvedProducts.length === 0 ? (
          <div className="py-16 px-4">
            <EmptyMessage>
              <EmptyMessage.Title>No se encontraron productos</EmptyMessage.Title>
              <EmptyMessage.Description>
                Intenta ajustar los filtros para encontrar lo que buscas.
              </EmptyMessage.Description>
            </EmptyMessage>
          </div>
        ) : filteredProducts.length > 0 ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
            {filteredProducts.map((product) => {
              const pendingQty = pendingQuantities[product.id] || 0;
              const isOutOfStock = product.stock === 0;

              return (
                <article 
                  key={product.id} 
                  className="group relative flex flex-col overflow-hidden rounded-xl border-2 border-black/8 bg-white transition-all hover:shadow-lg hover:border-black/15 cursor-pointer"
                  onClick={() => handleProductClick(product)}
                >
                  {/* Product Image Carousel */}
                  <div className="relative h-64 w-full overflow-hidden bg-gradient-to-b from-black/[0.02] to-transparent">
                    <ImageCarousel
                      images={
                        product.imageUrls && product.imageUrls.length > 0
                          ? product.imageUrls
                          : product.imageUrl && product.imageUrl.trim() !== ''
                          ? [product.imageUrl]
                          : []
                      }
                      alt={product.name}
                      className="h-64 w-full"
                      showIndicators={true}
                      showNavigation={true}
                      autoPlay={false}
                    />
                    {isOutOfStock && (
                      <div className="absolute top-3 right-3 px-2.5 py-1 bg-red-500 text-white text-xs font-bold rounded-md shadow-md">
                        Agotado
                      </div>
                    )}
                  </div>
                  
                  {/* Product Content */}
                  <div className="flex flex-1 flex-col gap-3.5 px-5 pt-4 pb-5">
                    <div className="space-y-1">
                      <div className="flex items-center gap-2 mb-1">
                        {/* {product.sellerName && (
                          <span className="text-xs px-2 py-0.5 rounded bg-blue-50 text-blue-700 font-semibold">
                            {product.sellerName}
                          </span>
                        )} */}
                        {product.brand && product.brand.trim() !== '' && (
                          <span className="text-xs px-2 py-0.5 rounded bg-gray-100 text-gray-700 font-medium">
                            {product.brand}
                          </span>
                        )}
                      </div>
                      <h3 className="text-lg font-bold text-black leading-tight">
                        {product.name}
                      </h3>
                      <p className="text-xl font-bold text-black">
                        ${product.price.toFixed(2)}
                      </p>
                    </div>
                    
                    <p className="text-sm leading-relaxed text-black/60 line-clamp-2 min-h-[2.5rem]" title={product.description}>
                      {product.description}
                    </p>
                    
                    {/* Quantity Selector and Add Button */}
                    <div className="flex items-center justify-between gap-3 mt-auto pt-2">
                      <div className="flex items-center rounded-lg bg-black/[0.06] border border-black/10 px-2 py-1.5">
                        <button
                          type="button"
                          className={`flex h-7 w-7 items-center justify-center rounded-md transition-all ${
                            pendingQty === 0 || isOutOfStock
                              ? 'opacity-30 cursor-not-allowed'
                              : 'hover:bg-black/10 active:scale-95'
                          }`}
                          aria-label={`Decrease quantity of ${product.name}`}
                          onClick={(event) => {
                            event.stopPropagation();
                            updatePendingQuantity(product.id, -1);
                          }}
                          disabled={pendingQty === 0 || isOutOfStock}
                        >
                          <span className="text-base font-bold">−</span>
                        </button>
                        <span className="min-w-[28px] px-2 text-center text-sm font-bold text-black">
                          {pendingQty}
                        </span>
                        <button
                          type="button"
                          className={`flex h-7 w-7 items-center justify-center rounded-md transition-all ${
                            pendingQty >= product.stock || isOutOfStock
                              ? 'opacity-30 cursor-not-allowed'
                              : 'hover:bg-black/10 active:scale-95'
                          }`}
                          aria-label={`Increase quantity of ${product.name}`}
                          onClick={(event) => {
                            event.stopPropagation();
                            updatePendingQuantity(product.id, 1);
                          }}
                          disabled={pendingQty >= product.stock || isOutOfStock}
                        >
                          <span className="text-base font-bold">+</span>
                        </button>
                      </div>
                      
                      <div className="flex items-center gap-2">
                        {/* Add to Cart Button */}
                        {pendingQty > 0 && (
                          <button
                            type="button"
                            onClick={(event) => {
                              event.stopPropagation();
                              handleAddToCart(product.id);
                            }}
                            className="bg-black text-white font-semibold py-2 px-4 rounded-lg hover:bg-black/90 transition-colors text-sm shadow-sm hover:shadow-md"
                          >
                            Agregar
                          </button>
                        )}
                      </div>
                    </div>
                  </div>
                </article>
              );
            })}
          </div>
        ) : (
          <div className="py-16 px-4">
            <EmptyMessage>
              <EmptyMessage.Title>No se encontraron productos</EmptyMessage.Title>
              <EmptyMessage.Description>
                Intenta ajustar los filtros para encontrar lo que buscas.
              </EmptyMessage.Description>
            </EmptyMessage>
          </div>
        )}
      </section>

      {/* Product Detail Modal */}
      {selectedProduct && (
        <ProductDetailModal
          product={selectedProduct}
          quantity={pendingQuantities[selectedProduct.id] || 0}
          onClose={handleCloseModal}
          onQuantityChange={handleModalQuantityChange}
          onAddToCart={() => handleAddToCart(selectedProduct.id)}
        />
      )}

      {/* Cart Modal */}
      {isCartOpen && (
        <CartModal
          cartItems={cartItems}
          onClose={() => setIsCartOpen(false)}
          onCheckout={handleCheckout}
          onRemoveItem={handleCartItemRemove}
          onUpdateQuantity={handleCartItemQuantityChange}
        />
      )}
      <style>{`
        input[type="range"].slider::-webkit-slider-thumb {
          appearance: none;
          width: 18px;
          height: 18px;
          border-radius: 50%;
          background: #000;
          cursor: pointer;
          border: 2px solid #fff;
          box-shadow: 0 2px 4px rgba(0,0,0,0.2);
          transition: transform 0.1s ease;
        }
        input[type="range"].slider::-webkit-slider-thumb:hover {
          transform: scale(1.1);
        }
        input[type="range"].slider::-moz-range-thumb {
          width: 18px;
          height: 18px;
          border-radius: 50%;
          background: #000;
          cursor: pointer;
          border: 2px solid #fff;
          box-shadow: 0 2px 4px rgba(0,0,0,0.2);
          transition: transform 0.1s ease;
        }
        input[type="range"].slider::-moz-range-thumb:hover {
          transform: scale(1.1);
        }
        input[type="range"].slider::-ms-thumb {
          width: 18px;
          height: 18px;
          border-radius: 50%;
          background: #000;
          cursor: pointer;
          border: 2px solid #fff;
          box-shadow: 0 2px 4px rgba(0,0,0,0.2);
        }
        select {
          -webkit-appearance: none;
          -moz-appearance: none;
          appearance: none;
        }
      `}</style>
    </div>
  );
};
