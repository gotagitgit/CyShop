import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router';
import type { CatalogItemDto } from '../types/catalog';
import { fetchCatalogItemById, getCatalogImageUrl } from '../api/catalogApi';
import { useAppDispatch, useAppSelector } from '../store/hooks';
import { addItem, saveBasket } from '../store/basketSlice';
import { useAuthStatus } from '../auth/useAuthStatus';

export default function ProductDetailPage() {
  const { id } = useParams<{ id: string }>();
  const dispatch = useAppDispatch();
  const basket = useAppSelector((state) => state.basket.basket);
  const { isAuthenticated, login } = useAuthStatus();

  const [item, setItem] = useState<CatalogItemDto | null>(null);
  const [status, setStatus] = useState<'loading' | 'succeeded' | 'failed'>('loading');
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) {
      setStatus('failed');
      setError('Product not found');
      return;
    }

    let cancelled = false;
    setStatus('loading');
    setError(null);

    fetchCatalogItemById(id)
      .then((data) => {
        if (!cancelled) {
          setItem(data);
          setStatus('succeeded');
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Failed to load product');
          setStatus('failed');
        }
      });

    return () => {
      cancelled = true;
    };
  }, [id]);

  const handleAddToBasket = () => {
    if (!isAuthenticated) {
      login();
      return;
    }
    if (!item) return;
    const newItem = {
      productId: item.id,
      productName: item.name,
      unitPrice: item.price,
      oldUnitPrice: item.price,
      quantity: 1,
      pictureUrl: getCatalogImageUrl(item.id),
    };
    dispatch(addItem(newItem));

    // Build the updated items list and persist to the API
    const currentItems = basket?.items ?? [];
    const existing = currentItems.find((i) => i.productId === item.id);
    const updatedItems = existing
      ? currentItems.map((i) =>
          i.productId === item.id
            ? { ...i, quantity: i.quantity + 1 }
            : i,
        )
      : [...currentItems, newItem];
    dispatch(saveBasket({ buyerId: basket?.buyerId ?? '', items: updatedItems }));
  };

  if (status === 'loading') {
    return (
      <main>
        <div role="status" aria-label="Loading product">
          <p>Loading…</p>
        </div>
      </main>
    );
  }

  if (status === 'failed' || !item) {
    return (
      <main>
        <div role="alert">
          <p>{error ?? 'Product not found'}</p>
          <Link to="/">Back to catalog</Link>
        </div>
      </main>
    );
  }

  return (
    <main>
      <Link to="/">&larr; Back to catalog</Link>
      <article aria-label={item.name}>
        <img
          src={getCatalogImageUrl(item.id)}
          alt={item.name}
        />
        <h1>{item.name}</h1>
        <p>{item.brand.name} &middot; {item.type.name}</p>
        <p>${item.price.toFixed(2)}</p>
        <p>{item.description}</p>
        <button type="button" onClick={handleAddToBasket}>
          Add to basket
        </button>
      </article>
    </main>
  );
}
