import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import type { BasketItem } from '../types/basket';
import type { CustomerAddress } from '../types/customer';
import { fetchAddresses as apiFetchAddresses } from '../api/customersApi';

export interface CheckoutState {
  selectedItems: BasketItem[];
  addresses: CustomerAddress[];
  selectedAddressId: string | null;
  addressStatus: 'idle' | 'loading' | 'succeeded' | 'failed';
  addressError: string | null;
}

const initialState: CheckoutState = {
  selectedItems: [],
  addresses: [],
  selectedAddressId: null,
  addressStatus: 'idle',
  addressError: null,
};

export const fetchAddresses = createAsyncThunk<CustomerAddress[], void>(
  'checkout/fetchAddresses',
  async (_, { rejectWithValue }) => {
    try {
      return await apiFetchAddresses();
    } catch (err: unknown) {
      const message =
        err instanceof Error
          ? err.message
          : typeof err === 'object' && err !== null && 'message' in err
            ? String((err as { message: unknown }).message)
            : 'Failed to fetch addresses';
      return rejectWithValue(message);
    }
  },
);

const checkoutSlice = createSlice({
  name: 'checkout',
  initialState,
  reducers: {
    setSelectedItems(state, action: PayloadAction<BasketItem[]>) {
      state.selectedItems = action.payload;
    },
    setSelectedAddressId(state, action: PayloadAction<string | null>) {
      state.selectedAddressId = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchAddresses.pending, (state) => {
        state.addressStatus = 'loading';
        state.addressError = null;
      })
      .addCase(fetchAddresses.fulfilled, (state, action) => {
        state.addressStatus = 'succeeded';
        state.addresses = action.payload;
        state.addressError = null;
        const defaultAddr = action.payload.find((a) => a.isDefault);
        if (defaultAddr) {
          state.selectedAddressId = defaultAddr.id;
        }
      })
      .addCase(fetchAddresses.rejected, (state, action) => {
        state.addressStatus = 'failed';
        state.addressError =
          (action.payload as string) ?? action.error.message ?? 'Unknown error';
      });
  },
});

export const { setSelectedItems, setSelectedAddressId } =
  checkoutSlice.actions;
export default checkoutSlice.reducer;
