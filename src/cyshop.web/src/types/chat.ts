export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
  timestamp: string; // ISO 8601
}

export interface ChatRequest {
  messages: { role: string; content: string }[];
  query: string;
}

export interface ChatResponse {
  answer: string;
}
