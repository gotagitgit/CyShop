import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { CustomerProfile, CustomerAddress } from '../types/customer';
import {
  fetchProfile as apiFetchProfile,
  createProfile as apiCreateProfile,
  updateProfile as apiUpdateProfile,
  deleteProfile as apiDeleteProfile,
  fetchAddresses as apiFetchAddresses,
  createAddress as apiCreateAddress,
  updateAddress as apiUpdateAddress,
  deleteAddress as apiDeleteAddress,
} from '../api/customersApi';
import type { CreateCustomerPayload, CreateAddressPayload } from '../api/customersApi';

export interface CustomerState {
  profile: CustomerProfile | null;
  addresses: CustomerAddress[];
  profileStatus: 'idle' | 'loading' | 'succeeded' | 'failed' | 'not_found';
  addressStatus: 'idle' | 'loading' | 'succeeded' | 'failed';
  error: string | null;
}

const initialState: CustomerState = {
  profile: null,
  addresses: [],
  profileStatus: 'idle',
  addressStatus: 'idle',
  error: null,
};

export const fetchProfile = createAsyncThunk<CustomerProfile, void>(
  'customer/fetchProfile',
  async (_, { rejectWithValue }) => {
    try {
      return await apiFetchProfile();
    } catch (err: unknown) {
      if (typeof err === 'object' && err !== null && 'status' in err && (err as { status: number }).status === 404) {
        return rejectWithValue('not_found');
      }
      const message =
        err instanceof Error
          ? err.message
          : typeof err === 'object' && err !== null && 'message' in err
            ? String((err as { message: unknown }).message)
            : 'Failed to fetch profile';
      return rejectWithValue(message);
    }
  },
);

export const createProfile = createAsyncThunk<CustomerProfile, CreateCustomerPayload>(
  'customer/createProfile',
  async (data, { rejectWithValue }) => {
    try {
      return await apiCreateProfile(data);
    } catch (err: unknown) {
      const message =
        err instanceof Error
          ? err.message
          : typeof err === 'object' && err !== null && 'message' in err
            ? String((err as { message: unknown }).message)
            : 'Failed to create profile';
      return rejectWithValue(message);
    }
  },
);

export const updateProfile = createAsyncThunk<CustomerProfile, CreateCustomerPayload>(
  'customer/updateProfile',
  async (data, { rejectWithValue }) => {
    try {
      return await apiUpdateProfile(data);
    } catch (err: unknown) {
      const message =
        err instanceof Error
          ? err.message
          : typeof err === 'object' && err !== null && 'message' in err
            ? String((err as { message: unknown }).message)
            : 'Failed to update profile';
      return rejectWithValue(message);
    }
  },
);

export const deleteProfile = createAsyncThunk<void, void>(
  'customer/deleteProfile',
  async (_, { rejectWithValue }) => {
    try {
      await apiDeleteProfile();
    } catch (err: unknown) {
      const message =
        err instanceof Error
          ? err.message
          : typeof err === 'object' && err !== null && 'message' in err
            ? String((err as { message: unknown }).message)
            : 'Failed to delete profile';
      return rejectWithValue(message);
    }
  },
);

export const fetchAddresses = createAsyncThunk<CustomerAddress[], void>(
  'customer/fetchAddresses',
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

export const createAddress = createAsyncThunk<CustomerAddress, CreateAddressPayload>(
  'customer/createAddress',
  async (data, { rejectWithValue }) => {
    try {
      return await apiCreateAddress(data);
    } catch (err: unknown) {
      const message =
        err instanceof Error
          ? err.message
          : typeof err === 'object' && err !== null && 'message' in err
            ? String((err as { message: unknown }).message)
            : 'Failed to create address';
      return rejectWithValue(message);
    }
  },
);

export const updateAddress = createAsyncThunk<CustomerAddress, { id: string; data: CreateAddressPayload }>(
  'customer/updateAddress',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      return await apiUpdateAddress(id, data);
    } catch (err: unknown) {
      const message =
        err instanceof Error
          ? err.message
          : typeof err === 'object' && err !== null && 'message' in err
            ? String((err as { message: unknown }).message)
            : 'Failed to update address';
      return rejectWithValue(message);
    }
  },
);

export const deleteAddress = createAsyncThunk<string, string>(
  'customer/deleteAddress',
  async (id, { rejectWithValue }) => {
    try {
      await apiDeleteAddress(id);
      return id;
    } catch (err: unknown) {
      const message =
        err instanceof Error
          ? err.message
          : typeof err === 'object' && err !== null && 'message' in err
            ? String((err as { message: unknown }).message)
            : 'Failed to delete address';
      return rejectWithValue(message);
    }
  },
);

const customerSlice = createSlice({
  name: 'customer',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    // fetchProfile
    builder
      .addCase(fetchProfile.pending, (state) => {
        state.profileStatus = 'loading';
        state.error = null;
      })
      .addCase(fetchProfile.fulfilled, (state, action) => {
        state.profileStatus = 'succeeded';
        state.profile = action.payload;
        state.error = null;
      })
      .addCase(fetchProfile.rejected, (state, action) => {
        if (action.payload === 'not_found') {
          state.profileStatus = 'not_found';
          state.error = null;
        } else {
          state.profileStatus = 'failed';
          state.error =
            (action.payload as string) ?? action.error.message ?? 'Unknown error';
        }
      });

    // createProfile
    builder
      .addCase(createProfile.pending, (state) => {
        state.profileStatus = 'loading';
        state.error = null;
      })
      .addCase(createProfile.fulfilled, (state, action) => {
        state.profileStatus = 'succeeded';
        state.profile = action.payload;
        state.error = null;
      })
      .addCase(createProfile.rejected, (state, action) => {
        state.profileStatus = 'failed';
        state.error =
          (action.payload as string) ?? action.error.message ?? 'Unknown error';
      });

    // updateProfile
    builder
      .addCase(updateProfile.pending, (state) => {
        state.profileStatus = 'loading';
        state.error = null;
      })
      .addCase(updateProfile.fulfilled, (state, action) => {
        state.profileStatus = 'succeeded';
        state.profile = action.payload;
        state.error = null;
      })
      .addCase(updateProfile.rejected, (state, action) => {
        state.profileStatus = 'failed';
        state.error =
          (action.payload as string) ?? action.error.message ?? 'Unknown error';
      });

    // deleteProfile
    builder
      .addCase(deleteProfile.pending, (state) => {
        state.profileStatus = 'loading';
        state.error = null;
      })
      .addCase(deleteProfile.fulfilled, (state) => {
        state.profileStatus = 'idle';
        state.profile = null;
        state.error = null;
      })
      .addCase(deleteProfile.rejected, (state, action) => {
        state.profileStatus = 'failed';
        state.error =
          (action.payload as string) ?? action.error.message ?? 'Unknown error';
      });

    // fetchAddresses
    builder
      .addCase(fetchAddresses.pending, (state) => {
        state.addressStatus = 'loading';
        state.error = null;
      })
      .addCase(fetchAddresses.fulfilled, (state, action) => {
        state.addressStatus = 'succeeded';
        state.addresses = action.payload;
        state.error = null;
      })
      .addCase(fetchAddresses.rejected, (state, action) => {
        state.addressStatus = 'failed';
        state.error =
          (action.payload as string) ?? action.error.message ?? 'Unknown error';
      });

    // createAddress
    builder
      .addCase(createAddress.pending, (state) => {
        state.addressStatus = 'loading';
        state.error = null;
      })
      .addCase(createAddress.fulfilled, (state, action) => {
        state.addressStatus = 'succeeded';
        state.addresses.push(action.payload);
        state.error = null;
      })
      .addCase(createAddress.rejected, (state, action) => {
        state.addressStatus = 'failed';
        state.error =
          (action.payload as string) ?? action.error.message ?? 'Unknown error';
      });

    // updateAddress
    builder
      .addCase(updateAddress.pending, (state) => {
        state.addressStatus = 'loading';
        state.error = null;
      })
      .addCase(updateAddress.fulfilled, (state, action) => {
        state.addressStatus = 'succeeded';
        const index = state.addresses.findIndex((a) => a.id === action.payload.id);
        if (index !== -1) {
          state.addresses[index] = action.payload;
        }
        state.error = null;
      })
      .addCase(updateAddress.rejected, (state, action) => {
        state.addressStatus = 'failed';
        state.error =
          (action.payload as string) ?? action.error.message ?? 'Unknown error';
      });

    // deleteAddress
    builder
      .addCase(deleteAddress.pending, (state) => {
        state.addressStatus = 'loading';
        state.error = null;
      })
      .addCase(deleteAddress.fulfilled, (state, action) => {
        state.addressStatus = 'succeeded';
        state.addresses = state.addresses.filter((a) => a.id !== action.payload);
        state.error = null;
      })
      .addCase(deleteAddress.rejected, (state, action) => {
        state.addressStatus = 'failed';
        state.error =
          (action.payload as string) ?? action.error.message ?? 'Unknown error';
      });
  },
});

export default customerSlice.reducer;
