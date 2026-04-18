import { configureStore } from '@reduxjs/toolkit';
import catalogReducer from './catalogSlice';
import basketReducer from './basketSlice';

export const store = configureStore({
  reducer: {
    catalog: catalogReducer,
    basket: basketReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
