import { describe, it, expect, vi, beforeEach } from 'vitest';
import {
  fetchCatalogItems,
  fetchCatalogItemById,
  fetchCatalogItemsByType,
  fetchCatalogItemsByBrand,
  getCatalogImageUrl,
} from './catalogApi';
import type { CatalogItemDto } from '../types/catalog';

vi.mock('./apiClient', () => ({
  apiRequest: vi.fn(),
}));

import { apiRequest } from './apiClient';

const mockItem: CatalogItemDto = {
  id: 'abc-123',
  type: { id: 'type-1', name: 'Shoes' },
  brand: { id: 'brand-1', name: 'Nike' },
  name: 'Running Shoe',
  description: 'A fast shoe',
  price: 99.99,
  imagePath: '/images/shoe.png',
};

describe('catalogApi', () => {
  beforeEach(() => {
    vi.mocked(apiRequest).mockReset();
  });

  it('fetchCatalogItems calls GET /api/catalog', async () => {
    vi.mocked(apiRequest).mockResolvedValue([mockItem]);

    const result = await fetchCatalogItems();

    expect(apiRequest).toHaveBeenCalledWith('GET', '/api/catalog');
    expect(result).toEqual([mockItem]);
  });

  it('fetchCatalogItemById calls GET /api/catalog/{id}', async () => {
    vi.mocked(apiRequest).mockResolvedValue(mockItem);

    const result = await fetchCatalogItemById('abc-123');

    expect(apiRequest).toHaveBeenCalledWith('GET', '/api/catalog/abc-123');
    expect(result).toEqual(mockItem);
  });

  it('fetchCatalogItemsByType calls GET /api/catalog/type/{typeId}', async () => {
    vi.mocked(apiRequest).mockResolvedValue([mockItem]);

    const result = await fetchCatalogItemsByType('type-1');

    expect(apiRequest).toHaveBeenCalledWith('GET', '/api/catalog/type/type-1');
    expect(result).toEqual([mockItem]);
  });

  it('fetchCatalogItemsByBrand calls GET /api/catalog/brand/{brandId}', async () => {
    vi.mocked(apiRequest).mockResolvedValue([mockItem]);

    const result = await fetchCatalogItemsByBrand('brand-1');

    expect(apiRequest).toHaveBeenCalledWith('GET', '/api/catalog/brand/brand-1');
    expect(result).toEqual([mockItem]);
  });

  it('getCatalogImageUrl returns /api/catalog/{id}/pic', () => {
    expect(getCatalogImageUrl('abc-123')).toBe('/api/catalog/abc-123/pic');
    expect(getCatalogImageUrl('xyz-789')).toBe('/api/catalog/xyz-789/pic');
  });
});
