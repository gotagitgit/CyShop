import { describe, it, expect } from 'vitest';
import catalogReducer, {
  setFilterType,
  setFilterBrand,
  clearFilters,
  fetchCatalog,
  fetchCatalogByType,
  fetchCatalogByBrand,
  type CatalogState,
} from '../store/catalogSlice';
import type { CatalogItemDto } from '../types/catalog';

const initialState: CatalogState = {
  items: [],
  selectedItem: null,
  filters: { typeId: null, brandId: null },
  status: 'idle',
  error: null,
};

const sampleItem: CatalogItemDto = {
  id: '1',
  type: { id: 't1', name: 'Type A' },
  brand: { id: 'b1', name: 'Brand A' },
  name: 'Test Item',
  description: 'A test item',
  price: 9.99,
  imagePath: '/images/test.png',
};

describe('catalogSlice reducers', () => {
  it('should return the initial state', () => {
    expect(catalogReducer(undefined, { type: 'unknown' })).toEqual(initialState);
  });

  it('setFilterType sets typeId', () => {
    const state = catalogReducer(initialState, setFilterType('t1'));
    expect(state.filters.typeId).toBe('t1');
    expect(state.filters.brandId).toBeNull();
  });

  it('setFilterBrand sets brandId', () => {
    const state = catalogReducer(initialState, setFilterBrand('b1'));
    expect(state.filters.brandId).toBe('b1');
    expect(state.filters.typeId).toBeNull();
  });

  it('clearFilters resets both filters to null', () => {
    const withFilters: CatalogState = {
      ...initialState,
      filters: { typeId: 't1', brandId: 'b1' },
    };
    const state = catalogReducer(withFilters, clearFilters());
    expect(state.filters.typeId).toBeNull();
    expect(state.filters.brandId).toBeNull();
  });
});

describe('catalogSlice extraReducers - fetchCatalog', () => {
  it('sets status to loading on pending', () => {
    const state = catalogReducer(initialState, fetchCatalog.pending('requestId'));
    expect(state.status).toBe('loading');
    expect(state.error).toBeNull();
  });

  it('sets status to succeeded and stores items on fulfilled', () => {
    const items = [sampleItem];
    const state = catalogReducer(
      { ...initialState, status: 'loading' },
      fetchCatalog.fulfilled(items, 'requestId'),
    );
    expect(state.status).toBe('succeeded');
    expect(state.items).toEqual(items);
    expect(state.error).toBeNull();
  });

  it('sets status to failed and stores error on rejected', () => {
    const state = catalogReducer(
      { ...initialState, status: 'loading' },
      fetchCatalog.rejected(null, 'requestId', undefined, 'Network error'),
    );
    expect(state.status).toBe('failed');
    expect(state.error).toBe('Network error');
  });

  it('clears previous error on new pending', () => {
    const withError: CatalogState = { ...initialState, status: 'failed', error: 'old error' };
    const state = catalogReducer(withError, fetchCatalog.pending('requestId'));
    expect(state.status).toBe('loading');
    expect(state.error).toBeNull();
  });
});

describe('catalogSlice extraReducers - fetchCatalogByType', () => {
  it('sets status to loading on pending', () => {
    const state = catalogReducer(initialState, fetchCatalogByType.pending('requestId', 't1'));
    expect(state.status).toBe('loading');
    expect(state.error).toBeNull();
  });

  it('sets status to succeeded and replaces items on fulfilled', () => {
    const items = [sampleItem];
    const state = catalogReducer(
      { ...initialState, status: 'loading', items: [] },
      fetchCatalogByType.fulfilled(items, 'requestId', 't1'),
    );
    expect(state.status).toBe('succeeded');
    expect(state.items).toEqual(items);
  });

  it('sets status to failed on rejected', () => {
    const state = catalogReducer(
      { ...initialState, status: 'loading' },
      fetchCatalogByType.rejected(null, 'requestId', 't1', 'Type fetch failed'),
    );
    expect(state.status).toBe('failed');
    expect(state.error).toBe('Type fetch failed');
  });
});

describe('catalogSlice extraReducers - fetchCatalogByBrand', () => {
  it('sets status to loading on pending', () => {
    const state = catalogReducer(initialState, fetchCatalogByBrand.pending('requestId', 'b1'));
    expect(state.status).toBe('loading');
    expect(state.error).toBeNull();
  });

  it('sets status to succeeded and replaces items on fulfilled', () => {
    const items = [sampleItem];
    const state = catalogReducer(
      { ...initialState, status: 'loading' },
      fetchCatalogByBrand.fulfilled(items, 'requestId', 'b1'),
    );
    expect(state.status).toBe('succeeded');
    expect(state.items).toEqual(items);
  });

  it('sets status to failed on rejected', () => {
    const state = catalogReducer(
      { ...initialState, status: 'loading' },
      fetchCatalogByBrand.rejected(null, 'requestId', 'b1', 'Brand fetch failed'),
    );
    expect(state.status).toBe('failed');
    expect(state.error).toBe('Brand fetch failed');
  });
});
