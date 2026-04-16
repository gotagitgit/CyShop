import { useEffect } from 'react';
import { AuthGuard } from '../auth/AuthGuard';
import { useAppDispatch, useAppSelector } from '../store/hooks';
import {
  fetchBasket,
  saveBasket,
  removeItem,
  updateItemQuantity,
} from '../store/basketSlice';
import { Link } from 'react-router';

function BasketContent() {
  const dispatch = useAppDispatch();
  const { basket, status, error } = useAppSelector((state) => state.basket);

  useEffect(() => {
    dispatch(fetchBasket());
  }, [dispatch]);

  const handleQuantityChange = (productId: number, newQuantity: number) => {
    if (newQuantity < 1 || !basket) return;
    dispatch(updateItemQuantity({ productId, quantity: newQuantity }));
    const updatedItems = basket.items.map((item) =>
      item.productId === productId ? { ...item, quantity: newQuantity } : item,
    );
    dispatch(saveBasket({ ...basket, items: updatedItems }));
  };

  const handleRemove = (productId: number) => {
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

      <Link to="/">Continue shopping</Link>
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
