import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { ChatMessage, ChatResponse } from '../types/chat';
import { sendChatMessage } from '../api/chatApi';
import type { RootState } from './store';

export interface ChatState {
  messages: ChatMessage[];
  loading: boolean;
  error: string | null;
}

const initialState: ChatState = {
  messages: [],
  loading: false,
  error: null,
};

export const sendMessage = createAsyncThunk<ChatResponse, string, { state: RootState }>(
  'chat/sendMessage',
  async (message, { getState, rejectWithValue }) => {
    try {
      const { chat } = getState();
      const history = chat.messages.map((m) => ({ role: m.role, content: m.content }));
      return await sendChatMessage({ messages: history, query: message });
    } catch (err: unknown) {
      const errorMessage =
        err instanceof Error
          ? err.message
          : typeof err === 'object' && err !== null && 'message' in err
            ? String((err as { message: unknown }).message)
            : 'Something went wrong. Please try again.';
      return rejectWithValue(errorMessage);
    }
  },
);

const chatSlice = createSlice({
  name: 'chat',
  initialState,
  reducers: {
    clearChat(state) {
      state.messages = [];
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(sendMessage.pending, (state, action) => {
        state.messages.push({
          role: 'user',
          content: action.meta.arg,
          timestamp: new Date().toISOString(),
        });
        state.loading = true;
        state.error = null;
      })
      .addCase(sendMessage.fulfilled, (state, action) => {
        state.messages.push({
          role: 'assistant',
          content: action.payload.answer,
          timestamp: new Date().toISOString(),
        });
        state.loading = false;
      })
      .addCase(sendMessage.rejected, (state, action) => {
        state.error = (action.payload as string) ?? action.error.message ?? 'Unknown error';
        state.loading = false;
      });
  },
});

export const { clearChat } = chatSlice.actions;
export default chatSlice.reducer;
