import React, { useState, useMemo, useCallback } from 'react';
import { useWidgetProps } from '../../hooks/useWidgetProps';
import { useWidgetState } from '../../hooks/useWidgetState';
import { useMaxHeight } from '../../hooks/useMaxHeight';
import { useDisplayMode } from '../../hooks/useDisplayMode';

interface CartItem {
  productId: string;
  productName: string;
  productSku: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  imageUrl?: string;
  category?: string;
  brand?: string;
  sellerName?: string;
  shopKey?: string;
}

interface CartWidgetProps extends Record<string, unknown> {
  cartId?: string;
  items?: CartItem[];
  subTotal?: number;
  tax?: number;
  shipping?: number;
  total?: number;
  totalItems?: number;
  status?: "loading" | "completed" | "error";
}

interface CartWidgetState extends Record<string, unknown> {
  cartId?: string;
  items?: CartItem[];
}

const createDefaultWidgetState = (): CartWidgetState => ({
  cartId: undefined,
  items: [],
});

export const CartWidget: React.FC = () => {
  // Layout hooks
  const maxHeight = useMaxHeight() ?? undefined;
  const displayMode = useDisplayMode();

  // Widget props and state
  const widgetProps = useWidgetProps<CartWidgetProps>(() => ({}));
  const [widgetState, setWidgetState] = useWidgetState<CartWidgetState>(
    createDefaultWidgetState
  );

  // Resolve cart data with priority: widgetState > widgetProps.widgetState > widgetProps > default
  const resolvedCartData = useMemo(() => {
    // Extract structuredContent if available
    const structuredContent = widgetProps && 'structuredContent' in widgetProps
      ? (widgetProps as CartWidgetProps & { structuredContent?: CartWidgetProps }).structuredContent
      : widgetProps;

    return {
      cartId: structuredContent?.cartId ?? widgetProps?.cartId ?? widgetState?.cartId,
      items: structuredContent?.items ?? widgetProps?.items ?? widgetState?.items ?? [],
      subTotal: structuredContent?.subTotal ?? widgetProps?.subTotal ?? 0,
      tax: structuredContent?.tax ?? widgetProps?.tax ?? 0,
      shipping: structuredContent?.shipping ?? widgetProps?.shipping ?? 0,
      total: structuredContent?.total ?? widgetProps?.total ?? 0,
      totalItems: structuredContent?.totalItems ?? widgetProps?.totalItems ?? 0,
      status: structuredContent?.status ?? widgetProps?.status ?? "loading",
    };
  }, [widgetProps, widgetState]);

  const { cartId, items, subTotal, tax, shipping, total, totalItems, status } = resolvedCartData;
  const isLoading = status === "loading";

  // Calculate totals if not provided
  const calculatedSubTotal = subTotal > 0 ? subTotal : items.reduce((sum, item) => sum + item.totalPrice, 0);
  const calculatedTax = tax > 0 ? tax : calculatedSubTotal * 0.1; // 10% tax
  const calculatedShipping = shipping !== undefined ? shipping : (calculatedSubTotal > 50 ? 0 : 5.99);
  const calculatedTotal = total > 0 ? total : calculatedSubTotal + calculatedTax + calculatedShipping;
  const calculatedTotalItems = totalItems > 0 ? totalItems : items.reduce((sum, item) => sum + item.quantity, 0);

  // Note: Cart updates are handled by ChatGPT calling MCP tools
  // The widget displays the current state from toolOutput.structuredContent
  // Users can request updates through ChatGPT, which will call update_cart_item or remove_from_cart tools

  const handleCheckout = useCallback(() => {
    // Placeholder for checkout functionality
    console.log('Checkout clicked', { cartId, total: calculatedTotal });
  }, [cartId, calculatedTotal]);

  return (
    <div
      className="w-full h-full flex flex-col bg-white"
      style={{ maxHeight: maxHeight ? `${maxHeight}px` : undefined }}
    >
      {/* Header */}
      <div className="flex items-center justify-between p-6 border-b border-black/10 bg-white flex-shrink-0">
        <h2 className="text-2xl font-bold text-black">Carrito de Compras</h2>
        {cartId && (
          <span className="text-sm text-black/60">ID: {cartId.substring(0, 8)}...</span>
        )}
      </div>

      {/* Cart Items */}
      <div className="flex-1 overflow-y-auto p-6">
        {isLoading ? (
          <div className="space-y-4">
            {[...Array(3)].map((_, index) => (
              <div
                key={`skeleton-${index}`}
                className="flex flex-col sm:flex-row items-start sm:items-center gap-4 p-4 rounded-lg border border-black/5 bg-white animate-pulse"
              >
                {/* Skeleton Image */}
                <div className="relative h-24 w-24 sm:h-20 sm:w-20 flex-shrink-0 overflow-hidden rounded-lg bg-gray-200"></div>
                
                {/* Skeleton Product Info */}
                <div className="flex-1 min-w-0 w-full sm:w-auto">
                  <div className="h-5 w-3/4 bg-gray-200 rounded mb-2"></div>
                  <div className="h-4 w-1/2 bg-gray-200 rounded mb-2"></div>
                  <div className="h-5 w-24 bg-gray-200 rounded"></div>
                </div>
                
                {/* Skeleton Quantity and Total */}
                <div className="flex items-center gap-4 w-full sm:w-auto justify-between sm:justify-end">
                  <div className="h-8 w-24 bg-gray-200 rounded-full"></div>
                  <div className="h-6 w-20 bg-gray-200 rounded"></div>
                  <div className="h-9 w-9 bg-gray-200 rounded-full"></div>
                </div>
              </div>
            ))}
          </div>
        ) : items.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-12">
            <svg
              className="h-16 w-16 text-black/20 mb-4"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z"
              />
            </svg>
            <p className="text-lg font-medium text-black/60">Tu carrito está vacío</p>
            <p className="text-sm text-black/40 mt-1">Agrega productos para comenzar</p>
          </div>
        ) : (
          <div className="space-y-4">
            {items.map((item) => (
              <div
                key={item.productId}
                className="flex flex-col sm:flex-row items-start sm:items-center gap-4 p-4 rounded-lg border border-black/5 bg-white"
              >
                {/* Product Image */}
                <div className="relative h-24 w-24 sm:h-20 sm:w-20 flex-shrink-0 overflow-hidden rounded-lg bg-black/[0.02]">
                  {item.imageUrl && item.imageUrl.trim() !== '' ? (
                    <img
                      src={item.imageUrl}
                      alt={item.productName}
                      className="h-full w-full object-cover"
                    />
                  ) : (
                    <div className="flex h-full w-full items-center justify-center">
                      <svg className="h-8 w-8 text-black/20" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                      </svg>
                    </div>
                  )}
                </div>

                {/* Product Info - Expanded */}
                <div className="flex-1 min-w-0 w-full sm:w-auto">
                  <div className="flex items-center gap-2 mb-1">
                    {/* {item.sellerName && (
                      <span className="text-xs px-2 py-0.5 rounded bg-blue-50 text-blue-700 font-semibold">
                        {item.sellerName}
                      </span>
                    )} */}
                    {item.brand && item.brand.trim() !== '' && (
                      <span className="text-xs px-2 py-0.5 rounded bg-gray-100 text-gray-700 font-medium">
                        {item.brand}
                      </span>
                    )}
                  </div>
                  <h3 className="text-lg font-bold text-black mb-2 break-words">
                    {item.productName}
                  </h3>
                  <p className="text-sm text-black/60 mb-2">SKU: {item.productSku}</p>
                  <div className="flex items-center gap-2 mb-2">
                    <p className="text-lg font-bold text-black">
                      ${item.unitPrice.toFixed(2)}
                    </p>
                    <p className="text-sm text-black/60">c/u</p>
                  </div>
                </div>

                {/* Quantity Controls and Actions */}
                <div className="flex items-center gap-4 w-full sm:w-auto justify-between sm:justify-end">
                  {/* Quantity Display (read-only, updates via ChatGPT) */}
                  <div className="flex items-center rounded-full bg-black/[0.04] px-3 py-1.5 border border-gray-200">
                    <span className="min-w-[32px] px-3 text-center text-base font-bold text-slate-900">
                      {item.quantity}
                    </span>
                    <span className="text-xs text-black/60 ml-2">unidades</span>
                  </div>

                  {/* Total Price */}
                  <div className="text-right min-w-[80px]">
                    <p className="text-xl font-bold text-black">
                      ${item.totalPrice.toFixed(2)}
                    </p>
                    <p className="text-xs text-black/60 font-medium">Total</p>
                  </div>

                  {/* Item Info Badge */}
                  <div className="flex h-9 w-9 items-center justify-center rounded-full bg-black/5 text-black/40 flex-shrink-0">
                    <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Footer with Summary */}
      {items.length > 0 && (
        <div className="border-t border-black/5 p-6 space-y-4 flex-shrink-0">
          {/* Summary */}
          <div className="space-y-2">
            <div className="flex justify-between text-sm text-black/70">
              <span>Subtotal</span>
              <span>${calculatedSubTotal.toFixed(2)}</span>
            </div>
            <div className="flex justify-between text-sm text-black/70">
              <span>Impuesto</span>
              <span>${calculatedTax.toFixed(2)}</span>
            </div>
            <div className="flex justify-between text-sm text-black/70">
              <span>Envío</span>
              <span>{calculatedShipping === 0 ? 'Gratis' : `$${calculatedShipping.toFixed(2)}`}</span>
            </div>
            <div className="flex justify-between text-lg font-bold text-black pt-2 border-t border-black/10">
              <span>Total</span>
              <span>${calculatedTotal.toFixed(2)}</span>
            </div>
          </div>

          {/* Checkout Button */}
          <button
            type="button"
            onClick={handleCheckout}
            className="w-full bg-black text-white font-semibold py-3 px-6 rounded-lg hover:bg-black/90 transition-colors text-base"
          >
            Pagar
          </button>
        </div>
      )}
    </div>
  );
};
