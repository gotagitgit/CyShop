import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import type { CatalogItemDto } from '../types/catalog';
import {
  fetchCatalogItems,
  fetchCatalogItemsByType,
  fetchCatalogItemsByBrand,
} from '../api/catalogApi';

export interface CatalogState {
  items: CatalogItemDto[];
  selectedItem: CatalogItemDto | null;
  filters: {
    typeId: string | null;
    brandId: string | null;
  };
  status: 'idle' | 'loading' | 'succeeded' | 'failed';
  error: string | null;
}

const initialState: CatalogState = {
  items: [],
  selectedItem: null,
  filters: {
    typeId: null,
    brandId: null,
  },
  status: 'idle',
  error: null,
};

export const fetchCatalog = createAsyncThunk<CatalogItemDto[], void>(
  'catalog/fetchCatalog',
  async (_, { rejectWithValue }) => {
    try {
      return await fetchCatalogItems();
    } catch (err: unknown) {
      const message =
        err instanceof Error
          ? err.message
          : typeof err === 'object' && err !== null && 'message' in err
            ? String((err as { message: unknown }).message)
            : 'Failed to fetch catalog';
      return rejectWithValue(message);
    }
  },
);

export const fetchCatalogByType = createAsyncThunk<CatalogItemDto[], string>(
  'catalog/fetchCatalogByType',
  async (typeId, { rejectWithValue }) => {
    try {
      return await fetchCatalogItemsByType(typeId);
    } catch (err: unknown) {
      const message =
        err instanceof Error
          ? err.message
          : typeof err === 'object' && err !== null && 'message' in err
            ? String((err as { message: unknown }).message)
            : 'Failed to fetch catalog by type';
      return rejectWithValue(message);
    }
  },
);

export const fetchCatalogByBrand = createAsyncThunk<CatalogItemDto[], string>(
  'catalog/fetchCatalogByBrand',
  async (brandId, { rejectWithValue }) => {
    try {
      return await fetchCatalogItemsByBrand(brandId);
    } catch (err: unknown) {
      const message =
        err instanceof Error
          ? err.message
          : typeof err === 'object' && err !== null && 'message' in err
            ? String((err as { message: unknown }).message)
            : 'Failed to fetch catalog by brand';
      return rejectWithValue(message);
    }
  },
);

const catalogSlice = createSlice({
  name: 'catalog',
  initialState,
  reducers: {
    setFilterType(state, action: PayloadAction<string | null>) {
      state.filters.typeId = action.payload;
    },
    setFilterBrand(state, action: PayloadAction<string | null>) {
      state.filters.brandId = action.payload;
    },
    clearFilters(state) {
      state.filters.typeId = null;
      state.filters.brandId = null;
    },
  },
  extraReducers: (builder) => {
    // fetchCatalog
    builder
      .addCase(fetchCatalog.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(fetchCatalog.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.items = action.payload;
        state.error = null;
      })
      .addCase(fetchCatalog.rejected, (state, action) => {
        state.status = 'failed';
        state.error = (action.payload as string) ?? action.error.message ?? 'Unknown error';
      });

    // fetchCatalogByType
    builder
      .addCase(fetchCatalogByType.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(fetchCatalogByType.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.items = action.payload;
        state.error = null;
      })
      .addCase(fetchCatalogByType.rejected, (state, action) => {
        state.status = 'failed';
        state.error = (action.payload as string) ?? action.error.message ?? 'Unknown error';
      });

    // fetchCatalogByBrand
    builder
      .addCase(fetchCatalogByBrand.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(fetchCatalogByBrand.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.items = action.payload;
        state.error = null;
      })
      .addCase(fetchCatalogByBrand.rejected, (state, action) => {
        state.status = 'failed';
        state.error = (action.payload as string) ?? action.error.message ?? 'Unknown error';
      });
  },
});

export const { setFilterType, setFilterBrand, clearFilters } = catalogSlice.actions;
export default catalogSlice.reducer;
