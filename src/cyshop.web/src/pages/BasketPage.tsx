import { useEffect, useState } from 'react';
import { AuthGuard } from '../auth/AuthGuard';
import { useAppDispatch, useAppSelector } from '../store/hooks';
import {
  fetchBasket,
  saveBasket,
  removeItem,
  updateItemQuantity,
} from '../store/basketSlice';
import { setSelectedItems } from '../store/checkoutSlice';
import { Link, useNavigate } from 'react-router';
import type { BasketItem } from '../types/basket';

function BasketContent() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { basket, status, error } = useAppSelector((state) => state.basket);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  useEffect(() => {
    dispatch(fetchBasket());
  }, [dispatch]);

  const handleCheckboxChange = (productId: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(productId)) {
        next.delete(productId);
      } else {
        next.add(productId);
      }
      return next;
    });
  };

  const handleCheckout = () => {
    if (!basket) return;
    const selected: BasketItem[] = basket.items.filter((item) =>
      selectedIds.has(item.productId),
    );
    dispatch(setSelectedItems(selected));
    navigate('/checkout');
  };

  const handleQuantityChange = (productId: string, newQuantity: number) => {
    if (newQuantity < 1 || !basket) return;
    dispatch(updateItemQuantity({ productId, quantity: newQuantity }));
    const updatedItems = basket.items.map((item) =>
      item.productId === productId ? { ...item, quantity: newQuantity } : item,
    );
    dispatch(saveBasket({ ...basket, items: updatedItems }));
  };

  const handleRemove = (productId: string) => {
    if (!basket) return;
    dispatch(removeItem(productId));
    const updatedItems = basket.items.filter(
      (item) => item.productId !== productId,
    );
    dispatch(saveBasket({ ...basket, items: updatedItems }));
  };

  const handleRetry = () => {
    dispatch(fetchBasket());
  };

  if (status === 'loading') {
    return (
      <div role="status" aria-label="Loading basket">
        <p>Loading…</p>
      </div>
    );
  }

  if (status === 'failed') {
    return (
      <div role="alert">
        <p>Error: {error}</p>
        <button type="button" onClick={handleRetry}>
          Retry
        </button>
      </div>
    );
  }

  if (!basket || basket.items.length === 0) {
    return (
      <div>
        <p>Your basket is empty.</p>
        <Link to="/">Continue shopping</Link>
      </div>
    );
  }

  const basketTotal = basket.items.reduce(
    (sum, item) => sum + item.unitPrice * item.quantity,
    0,
  );

  return (
    <div>
      <table aria-label="Basket items">
        <thead>
          <tr>
            <th scope="col">Select</th>
            <th scope="col">Product</th>
            <th scope="col">Unit Price</th>
            <th scope="col">Quantity</th>
            <th scope="col">Line Total</th>
            <th scope="col">Actions</th>
          </tr>
        </thead>
        <tbody>
          {basket.items.map((item) => (
            <tr key={item.productId}>
              <td>
                <input
                  type="checkbox"
                  checked={selectedIds.has(item.productId)}
                  onChange={() => handleCheckboxChange(item.productId)}
                  aria-label={`Select ${item.productName}`}
                />
              </td>
              <td>{item.productName}</td>
              <td>${item.unitPrice.toFixed(2)}</td>
              <td>
                <button
                  type="button"
                  aria-label={`Decrease quantity of ${item.productName}`}
                  onClick={() =>
                    handleQuantityChange(item.productId, item.quantity - 1)
                  }
                  disabled={item.quantity <= 1}
                >
                  −
                </button>
                <span aria-label={`Quantity of ${item.productName}`}>
                  {item.quantity}
                </span>
                <button
                  type="button"
                  aria-label={`Increase quantity of ${item.productName}`}
                  onClick={() =>
                    handleQuantityChange(item.productId, item.quantity + 1)
                  }
                >
                  +
                </button>
              </td>
              <td>${(item.unitPrice * item.quantity).toFixed(2)}</td>
              <td>
                <button
                  type="button"
                  aria-label={`Remove ${item.productName} from basket`}
                  onClick={() => handleRemove(item.productId)}
                >
                  Remove
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <div aria-label="Basket total">
        <strong>Total: ${basketTotal.toFixed(2)}</strong>
      </div>

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: '1rem' }}>
        <Link to="/">Continue shopping</Link>
        <button
          type="button"
          disabled={selectedIds.size === 0}
          onClick={handleCheckout}
          aria-label="Proceed to checkout"
          style={{
            padding: '0.6rem 1.5rem',
            background: selectedIds.size === 0 ? '#a5b4fc' : '#4f46e5',
            color: 'white',
            border: 'none',
            borderRadius: '8px',
            fontWeight: 600,
            fontSize: '0.95rem',
            cursor: selectedIds.size === 0 ? 'not-allowed' : 'pointer',
            transition: 'background 0.15s',
          }}
        >
          Proceed to Checkout
        </button>
      </div>
    </div>
  );
}

export default function BasketPage() {
  return (
    <AuthGuard>
      <main>
        <h1>Your Basket</h1>
        <BasketContent />
      </main>
    </AuthGuard>
  );
}
