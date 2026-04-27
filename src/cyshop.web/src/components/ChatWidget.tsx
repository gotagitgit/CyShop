import { useState, useRef, useEffect } from 'react';
import type { FormEvent } from 'react';
import { useAuthStatus } from '../auth/useAuthStatus';
import { useAppDispatch, useAppSelector } from '../store/hooks';
import { sendMessage } from '../store/chatSlice';

export default function ChatWidget() {
  const { isAuthenticated } = useAuthStatus();
  const dispatch = useAppDispatch();
  const { messages, loading, error } = useAppSelector((state) => state.chat);

  const [isOpen, setIsOpen] = useState(false);
  const [input, setInput] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, loading]);

  if (!isAuthenticated) return null;

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    const trimmed = input.trim();
    if (!trimmed || loading) return;
    dispatch(sendMessage(trimmed));
    setInput('');
  };

  return (
    <>
      {!isOpen && (
        <button
          aria-label="Open chat"
          onClick={() => setIsOpen(true)}
          style={fabStyle}
        >
          💬
        </button>
      )}

      {isOpen && (
        <div style={panelStyle}>
          <div style={headerStyle}>
            <span style={{ fontWeight: 600 }}>Chat Assistant</span>
            <button
              aria-label="Close chat"
              onClick={() => setIsOpen(false)}
              style={closeButtonStyle}
            >
              ✕
            </button>
          </div>

          <div role="log" aria-label="Message list" style={messageListStyle}>
            {messages.map((msg, i) => (
              <div
                key={i}
                style={msg.role === 'user' ? userMessageStyle : assistantMessageStyle}
              >
                <div style={{ fontSize: '0.7rem', color: '#888', marginBottom: 2 }}>
                  {msg.role === 'user' ? 'You' : 'Assistant'}
                </div>
                <div>{msg.content}</div>
              </div>
            ))}

            {loading && (
              <div style={typingIndicatorStyle} aria-label="Assistant is typing">
                <span>●</span> <span>●</span> <span>●</span>
              </div>
            )}

            {error && (
              <div style={errorStyle} role="alert">
                Error: {error}
              </div>
            )}

            <div ref={messagesEndRef} />
          </div>

          <form onSubmit={handleSubmit} style={inputAreaStyle}>
            <input
              aria-label="Message input"
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              disabled={loading}
              placeholder="Type a message…"
              style={inputStyle}
            />
            <button
              aria-label="Send message"
              type="submit"
              disabled={!input.trim() || loading}
              style={sendButtonStyle}
            >
              Send
            </button>
          </form>
        </div>
      )}
    </>
  );
}

const fabStyle: React.CSSProperties = {
  position: 'fixed',
  bottom: 24,
  right: 24,
  width: 56,
  height: 56,
  borderRadius: '50%',
  border: 'none',
  backgroundColor: '#4f46e5',
  color: '#fff',
  fontSize: '1.5rem',
  cursor: 'pointer',
  boxShadow: '0 4px 12px rgba(0,0,0,0.25)',
  zIndex: 1000,
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
};

const panelStyle: React.CSSProperties = {
  position: 'fixed',
  bottom: 24,
  right: 24,
  width: 380,
  height: 520,
  borderRadius: 12,
  boxShadow: '0 8px 30px rgba(0,0,0,0.2)',
  backgroundColor: '#fff',
  display: 'flex',
  flexDirection: 'column',
  zIndex: 1000,
  overflow: 'hidden',
};

const headerStyle: React.CSSProperties = {
  display: 'flex',
  justifyContent: 'space-between',
  alignItems: 'center',
  padding: '12px 16px',
  backgroundColor: '#4f46e5',
  color: '#fff',
};

const closeButtonStyle: React.CSSProperties = {
  background: 'none',
  border: 'none',
  color: '#fff',
  fontSize: '1rem',
  cursor: 'pointer',
};

const messageListStyle: React.CSSProperties = {
  flex: 1,
  overflowY: 'auto',
  padding: 12,
  display: 'flex',
  flexDirection: 'column',
  gap: 8,
};

const userMessageStyle: React.CSSProperties = {
  alignSelf: 'flex-end',
  backgroundColor: '#4f46e5',
  color: '#fff',
  borderRadius: '12px 12px 0 12px',
  padding: '8px 12px',
  maxWidth: '80%',
  wordBreak: 'break-word',
};

const assistantMessageStyle: React.CSSProperties = {
  alignSelf: 'flex-start',
  backgroundColor: '#f3f4f6',
  color: '#111',
  borderRadius: '12px 12px 12px 0',
  padding: '8px 12px',
  maxWidth: '80%',
  wordBreak: 'break-word',
};

const typingIndicatorStyle: React.CSSProperties = {
  alignSelf: 'flex-start',
  color: '#888',
  fontSize: '1.2rem',
  padding: '4px 12px',
  letterSpacing: 4,
};

const errorStyle: React.CSSProperties = {
  alignSelf: 'center',
  backgroundColor: '#fef2f2',
  color: '#dc2626',
  borderRadius: 8,
  padding: '8px 12px',
  fontSize: '0.85rem',
  maxWidth: '90%',
};

const inputAreaStyle: React.CSSProperties = {
  display: 'flex',
  gap: 8,
  padding: 12,
  borderTop: '1px solid #e5e7eb',
};

const inputStyle: React.CSSProperties = {
  flex: 1,
  padding: '8px 12px',
  borderRadius: 8,
  border: '1px solid #d1d5db',
  outline: 'none',
  fontSize: '0.9rem',
};

const sendButtonStyle: React.CSSProperties = {
  padding: '8px 16px',
  borderRadius: 8,
  border: 'none',
  backgroundColor: '#4f46e5',
  color: '#fff',
  cursor: 'pointer',
  fontSize: '0.9rem',
};
