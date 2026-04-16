import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import type { CustomerBasket, BasketItem } from '../types/basket';
import {
  fetchBasket as apiFetchBasket,
  updateBasket as apiUpdateBasket,
  deleteBasket as apiDeleteBasket,
} from '../api/basketApi';

export interface BasketState {
  basket: CustomerBasket | null;
  status: 'idle' | 'loading' | 'succeeded' | 'failed';
  error: string | null;
}

const initialState: BasketState = {
  basket: null,
  status: 'idle',
  error: null,
};

export const fetchBasket = createAsyncThunk<CustomerBasket, void>(
  'basket/fetchBasket',
  async (_, { rejectWithValue }) => {
    try {
      return await apiFetchBasket();
    } catch (err: unknown) {
      const message =
        err instanceof Error
          ? err.message
          : typeof err === 'object' && err !== null && 'message' in err
            ? String((err as { message: unknown }).message)
            : 'Failed to fetch basket';
      return rejectWithValue(message);
    }
  },
);

export const saveBasket = createAsyncThunk<CustomerBasket, CustomerBasket>(
  'basket/saveBasket',
  async (basket, { rejectWithValue }) => {
    try {
      return await apiUpdateBasket(basket);
    } catch (err: unknown) {
      const message =
        err instanceof Error
          ? err.message
          : typeof err === 'object' && err !== null && 'message' in err
            ? String((err as { message: unknown }).message)
            : 'Failed to save basket';
      return rejectWithValue(message);
    }
  },
);

export const clearBasket = createAsyncThunk<void, void>(
  'basket/clearBasket',
  async (_, { rejectWithValue }) => {
    try {
      await apiDeleteBasket();
    } catch (err: unknown) {
      const message =
        err instanceof Error
          ? err.message
          : typeof err === 'object' && err !== null && 'message' in err
            ? String((err as { message: unknown }).message)
            : 'Failed to clear basket';
      return rejectWithValue(message);
    }
  },
);

const basketSlice = createSlice({
  name: 'basket',
  initialState,
  reducers: {
    addItem(state, action: PayloadAction<BasketItem>) {
      if (!state.basket) {
        state.basket = { buyerId: '', items: [action.payload] };
        return;
      }
      const existing = state.basket.items.find(
        (i) => i.productId === action.payload.productId,
      );
      if (existing) {
        existing.quantity += action.payload.quantity;
      } else {
        state.basket.items.push(action.payload);
      }
    },
    removeItem(state, action: PayloadAction<number>) {
      if (!state.basket) return;
      state.basket.items = state.basket.items.filter(
        (i) => i.productId !== action.payload,
      );
    },
    updateItemQuantity(
      state,
      action: PayloadAction<{ productId: number; quantity: number }>,
    ) {
      if (!state.basket) return;
      const item = state.basket.items.find(
        (i) => i.productId === action.payload.productId,
      );
      if (item) {
        item.quantity = action.payload.quantity;
      }
    },
  },
  extraReducers: (builder) => {
    // fetchBasket
    builder
      .addCase(fetchBasket.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(fetchBasket.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.basket = action.payload;
        state.error = null;
      })
      .addCase(fetchBasket.rejected, (state, action) => {
        state.status = 'failed';
        state.error =
          (action.payload as string) ?? action.error.message ?? 'Unknown error';
      });

    // saveBasket
    builder
      .addCase(saveBasket.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(saveBasket.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.basket = action.payload;
        state.error = null;
      })
      .addCase(saveBasket.rejected, (state, action) => {
        state.status = 'failed';
        state.error =
          (action.payload as string) ?? action.error.message ?? 'Unknown error';
      });

    // clearBasket
    builder
      .addCase(clearBasket.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(clearBasket.fulfilled, (state) => {
        state.status = 'succeeded';
        state.basket = null;
        state.error = null;
      })
      .addCase(clearBasket.rejected, (state, action) => {
        state.status = 'failed';
        state.error =
          (action.payload as string) ?? action.error.message ?? 'Unknown error';
      });
  },
});

export const { addItem, removeItem, updateItemQuantity } = basketSlice.actions;
export default basketSlice.reducer;
