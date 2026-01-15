import React from 'react';
import { Button } from '@openai/apps-sdk-ui/components/Button';

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

interface CartModalProps {
  cartItems: CartItem[];
  onClose: () => void;
  onCheckout: () => void;
  onRemoveItem: (productId: string) => void;
  onUpdateQuantity: (productId: string, delta: number) => void;
}

export const CartModal: React.FC<CartModalProps> = ({
  cartItems,
  onClose,
  onCheckout,
  onRemoveItem,
  onUpdateQuantity,
}) => {
  const subtotal = cartItems.reduce((sum, item) => sum + item.totalPrice, 0);
  const tax = subtotal * 0.1; // 10% tax
  const shipping = subtotal > 50 ? 0 : 5.99; // Free shipping over $50
  const total = subtotal + tax + shipping;

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
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-black/10 bg-white">
          <h2 className="text-2xl font-bold text-black">Carrito de Compras</h2>
          <button
            onClick={onClose}
            className="flex h-8 w-8 items-center justify-center rounded-full bg-black/5 text-black/70 transition-colors hover:bg-black/10 hover:text-black"
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
        </div>

        {/* Cart Items */}
        <div className="flex-1 overflow-y-auto p-6">
          {cartItems.length === 0 ? (
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
              {cartItems.map((item) => (
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
                    {/* Quantity Controls */}
                    <div className="flex items-center rounded-full bg-black/[0.04] px-3 py-1.5 border border-gray-200">
                      <button
                        type="button"
                        className="flex h-7 w-7 items-center justify-center rounded-full transition-colors opacity-50 hover:bg-slate-200 hover:opacity-100"
                        aria-label="Decrease quantity"
                        onClick={() => onUpdateQuantity(item.productId, -1)}
                      >
                        <span className="text-base font-medium text-slate-900">−</span>
                      </button>
                      <span className="min-w-[32px] px-3 text-center text-base font-bold text-slate-900">
                        {item.quantity}
                      </span>
                      <button
                        type="button"
                        className="flex h-7 w-7 items-center justify-center rounded-full transition-colors opacity-50 hover:bg-slate-200 hover:opacity-100"
                        aria-label="Increase quantity"
                        onClick={() => onUpdateQuantity(item.productId, 1)}
                      >
                        <span className="text-base font-medium text-slate-900">+</span>
                      </button>
                    </div>

                    {/* Total Price */}
                    <div className="text-right min-w-[80px]">
                      <p className="text-xl font-bold text-black">
                        ${item.totalPrice.toFixed(2)}
                      </p>
                      <p className="text-xs text-black/60 font-medium">Total</p>
                    </div>

                    {/* Remove Button */}
                    <button
                      type="button"
                      className="flex h-9 w-9 items-center justify-center rounded-full text-black/40 transition-colors hover:bg-red-50 hover:text-red-600 flex-shrink-0"
                      aria-label="Remove item"
                      onClick={() => onRemoveItem(item.productId)}
                    >
                      <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                      </svg>
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Footer with Summary */}
        {cartItems.length > 0 && (
          <div className="border-t border-black/5 p-6 space-y-4">
            {/* Summary */}
            <div className="space-y-2">
              <div className="flex justify-between text-sm text-black/70">
                <span>Subtotal</span>
                <span>${subtotal.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-sm text-black/70">
                <span>Impuesto</span>
                <span>${tax.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-sm text-black/70">
                <span>Envío</span>
                <span>{shipping === 0 ? 'Gratis' : `$${shipping.toFixed(2)}`}</span>
              </div>
              <div className="flex justify-between text-lg font-bold text-black pt-2 border-t border-black/10">
                <span>Total</span>
                <span>${total.toFixed(2)}</span>
              </div>
            </div>

            {/* Checkout Button */}
            <Button
              color="primary"
              variant="solid"
              size="lg"
              className="w-full"
              onClick={onCheckout}
            >
              Comprar
            </Button>
          </div>
        )}
      </div>
    </div>
  );
};
