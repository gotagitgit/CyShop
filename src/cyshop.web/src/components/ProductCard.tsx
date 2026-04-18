import { Link } from 'react-router';
import type { CatalogItemDto } from '../types/catalog';
import { getCatalogImageUrl } from '../api/catalogApi';
import { useAppDispatch, useAppSelector } from '../store/hooks';
import { addItem, saveBasket } from '../store/basketSlice';
import { useAuthStatus } from '../auth/useAuthStatus';

interface ProductCardProps {
  item: CatalogItemDto;
}

export default function ProductCard({ item }: ProductCardProps) {
  const dispatch = useAppDispatch();
  const basket = useAppSelector((state) => state.basket.basket);
  const { isAuthenticated, login } = useAuthStatus();

  const handleAddToBasket = () => {
    if (!isAuthenticated) {
      login();
      return;
    }
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

  return (
    <article aria-label={item.name}>
      <Link to={`/product/${item.id}`}>
        <img
          src={getCatalogImageUrl(item.id)}
          alt={item.name}
        />
      </Link>
      <Link to={`/product/${item.id}`}>
        <h3>{item.name}</h3>
      </Link>
      <p>{item.brand.name} &middot; {item.type.name}</p>
      <p>${item.price.toFixed(2)}</p>
      <button type="button" onClick={handleAddToBasket}>
        Add to basket
      </button>
    </article>
  );
}
