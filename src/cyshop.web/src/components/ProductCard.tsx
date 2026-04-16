import { Link } from 'react-router';
import type { CatalogItemDto } from '../types/catalog';
import { getCatalogImageUrl } from '../api/catalogApi';
import { useAppDispatch } from '../store/hooks';
import { addItem } from '../store/basketSlice';

interface ProductCardProps {
  item: CatalogItemDto;
}

export default function ProductCard({ item }: ProductCardProps) {
  const dispatch = useAppDispatch();

  const handleAddToBasket = () => {
    dispatch(
      addItem({
        productId: Number(item.id),
        productName: item.name,
        unitPrice: item.price,
        oldUnitPrice: item.price,
        quantity: 1,
        pictureUrl: getCatalogImageUrl(item.id),
      }),
    );
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
