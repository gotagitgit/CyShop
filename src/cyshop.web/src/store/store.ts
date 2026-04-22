import { configureStore } from '@reduxjs/toolkit';
import catalogReducer from './catalogSlice';
import basketReducer from './basketSlice';
import checkoutReducer from './checkoutSlice';
import customerReducer from './customerSlice';
import ordersReducer from './ordersSlice';

export const store = configureStore({
  reducer: {
    catalog: catalogReducer,
    basket: basketReducer,
    checkout: checkoutReducer,
    customer: customerReducer,
    orders: ordersReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
