export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
  timestamp: string; // ISO 8601
}

export interface ChatProduct {
  id: string;
  name: string;
  price: number;
}

export interface ChatRequest {
  messages: { role: string; content: string }[];
  query: string;
}

export interface ChatResponse {
  answer: string;
  products: ChatProduct[];
  suggestedAction: { type: string; data: Record<string, unknown> } | null;
}
