import { useEffect, useMemo } from 'react';
import { useAppDispatch, useAppSelector } from '../store/hooks';
import {
  fetchCatalog,
  fetchCatalogByType,
  fetchCatalogByBrand,
  setFilterType,
  setFilterBrand,
  clearFilters,
} from '../store/catalogSlice';
import ProductCard from '../components/ProductCard';

export default function CatalogPage() {
  const dispatch = useAppDispatch();
  const { items, status, error, filters } = useAppSelector((state) => state.catalog);

  useEffect(() => {
    dispatch(fetchCatalog());
  }, [dispatch]);

  const uniqueTypes = useMemo(
    () =>
      Array.from(
        new Map(items.map((item) => [item.type.id, item.type])).values(),
      ),
    [items],
  );

  const uniqueBrands = useMemo(
    () =>
      Array.from(
        new Map(items.map((item) => [item.brand.id, item.brand])).values(),
      ),
    [items],
  );

  const handleTypeChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const typeId = e.target.value || null;
    if (typeId) {
      dispatch(setFilterType(typeId));
      dispatch(fetchCatalogByType(typeId));
    } else {
      dispatch(clearFilters());
      dispatch(fetchCatalog());
    }
  };

  const handleBrandChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const brandId = e.target.value || null;
    if (brandId) {
      dispatch(setFilterBrand(brandId));
      dispatch(fetchCatalogByBrand(brandId));
    } else {
      dispatch(clearFilters());
      dispatch(fetchCatalog());
    }
  };

  const handleRetry = () => {
    dispatch(fetchCatalog());
  };

  return (
    <main>
      <h1>Catalog</h1>

      <div role="search" aria-label="Catalog filters">
        <label htmlFor="type-filter">
          Type
          <select
            id="type-filter"
            value={filters.typeId ?? ''}
            onChange={handleTypeChange}
          >
            <option value="">All Types</option>
            {uniqueTypes.map((type) => (
              <option key={type.id} value={type.id}>
                {type.name}
              </option>
            ))}
          </select>
        </label>

        <label htmlFor="brand-filter">
          Brand
          <select
            id="brand-filter"
            value={filters.brandId ?? ''}
            onChange={handleBrandChange}
          >
            <option value="">All Brands</option>
            {uniqueBrands.map((brand) => (
              <option key={brand.id} value={brand.id}>
                {brand.name}
              </option>
            ))}
          </select>
        </label>
      </div>

      {status === 'loading' && (
        <div role="status" aria-label="Loading catalog">
          <p>Loading…</p>
        </div>
      )}

      {status === 'failed' && (
        <div role="alert">
          <p>Error: {error}</p>
          <button type="button" onClick={handleRetry}>
            Retry
          </button>
        </div>
      )}

      {status === 'succeeded' && items.length === 0 && (
        <p>No products found.</p>
      )}

      {status === 'succeeded' && items.length > 0 && (
        <div role="list" aria-label="Product catalog">
          {items.map((item) => (
            <div role="listitem" key={item.id}>
              <ProductCard item={item} />
            </div>
          ))}
        </div>
      )}
    </main>
  );
}
