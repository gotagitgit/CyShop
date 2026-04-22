import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { OrderSummary, CreateOrderRequest } from '../types/order';
import * as ordersApi from '../api/ordersApi';

export interface OrdersState {
  orders: OrderSummary[];
  status: 'idle' | 'loading' | 'succeeded' | 'failed';
  error: string | null;
}

const initialState: OrdersState = {
  orders: [],
  status: 'idle',
  error: null,
};

export const fetchOrders = createAsyncThunk<OrderSummary[], void>(
  'orders/fetchOrders',
  async (_, { rejectWithValue }) => {
    try {
      return await ordersApi.fetchOrders();
    } catch (err: unknown) {
      const message =
        err instanceof Error
          ? err.message
          : typeof err === 'object' && err !== null && 'message' in err
            ? String((err as { message: unknown }).message)
            : 'Failed to fetch orders';
      return rejectWithValue(message);
    }
  },
);

export const createOrder = createAsyncThunk<void, { request: CreateOrderRequest; idempotencyKey: string }>(
  'orders/createOrder',
  async ({ request, idempotencyKey }, { rejectWithValue }) => {
    try {
      await ordersApi.createOrder(request, idempotencyKey);
    } catch (err: unknown) {
      const message =
        err instanceof Error
          ? err.message
          : typeof err === 'object' && err !== null && 'message' in err
            ? String((err as { message: unknown }).message)
            : 'Failed to create order';
      return rejectWithValue(message);
    }
  },
);

const ordersSlice = createSlice({
  name: 'orders',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchOrders.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(fetchOrders.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.orders = action.payload;
        state.error = null;
      })
      .addCase(fetchOrders.rejected, (state, action) => {
        state.status = 'failed';
        state.error =
          (action.payload as string) ?? action.error.message ?? 'Unknown error';
      })
      .addCase(createOrder.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(createOrder.fulfilled, (state) => {
        state.status = 'succeeded';
        state.error = null;
      })
      .addCase(createOrder.rejected, (state, action) => {
        state.status = 'failed';
        state.error =
          (action.payload as string) ?? action.error.message ?? 'Unknown error';
      });
  },
});

export default ordersSlice.reducer;
