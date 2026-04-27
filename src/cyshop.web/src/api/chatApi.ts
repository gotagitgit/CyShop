import type { ChatRequest, ChatResponse } from '../types/chat';
import { apiRequest } from './apiClient';

export function sendChatMessage(request: ChatRequest): Promise<ChatResponse> {
  return apiRequest<ChatResponse>('POST', '/api/chat', {
    body: request,
    requireAuth: true,
  });
}
