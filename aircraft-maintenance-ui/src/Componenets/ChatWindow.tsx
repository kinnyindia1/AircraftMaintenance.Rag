import { useState } from 'react';
import { askQuestion } from '../Services/api';

// Simple message interface for our UI
interface Message {
  id: number;
  sender: 'user' | 'ai';
  text: string;
}

// Helper to render a chat bubble
function ChatBubble({ text, sender }: { text: string; sender: 'user' | 'ai' }) {
  // Align user messages to the right, AI messages to the left
  const bubbleClass = sender === 'user' ? 'bg-primary text-white ms-auto' : 'bg-light text-dark me-auto';

  return (
    <div className={`d-flex p-2 rounded mb-2 ${bubbleClass}`} style={{ maxWidth: '80%' }}>
      <span>{text}</span>
    </div>
  );
}

export default function ChatWindow() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);
  // Counter survives re-renders so each message gets a unique id
  const [nextId, setNextId] = useState(1);

  const handleSend = async () => {
    if (!input.trim() || loading) return;

    const userId = nextId;
    const aiId = nextId + 1;

    const userMessage: Message = { id: userId, sender: 'user', text: input };
    setMessages((prev) => [...prev, userMessage]);
    setInput('');
    setLoading(true);

    try {
      const response = await askQuestion(input);
      const aiMessage: Message = { id: aiId, sender: 'ai', text: response.answer };
      setMessages((prev) => [...prev, aiMessage]);
    } catch (err: any) {
      const errorMessage: Message = {
        id: aiId,
        sender: 'ai',
        text: 'Error: Could not get a response. Please make sure you are logged in and the API is running.',
      };
      setMessages((prev) => [...prev, errorMessage]);
    } finally {
      setLoading(false);
    }

    // Advance the id counter by 2 (user + AI/error message)
    setNextId(aiId + 1);
  };

  return (
    <div className="container-fluid p-4" style={{ maxWidth: '800px' }}>
      <h1 className="text-center mb-4 text-dark fw-bold">Aircraft Maintenance Chat</h1>

      {/* Loading state */}
      {loading && <div className="mb-4 text-center text-muted">Thinking...</div>}

      {/* Messages container */}
      <div className="d-flex flex-column">
        {messages.map((msg) => (
          <ChatBubble key={msg.id} text={msg.text} sender={msg.sender} />
        ))}
      </div>

      {/* Input area */}
      <div className="mt-4 border-top pt-3">
        <div className="d-flex gap-2">
          <input
            type="text"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSend()}
            placeholder="Ask a question about maintenance..."
            className="form-control"
          />
          <button
            onClick={handleSend}
            disabled={loading}
            className="btn btn-primary"
          >
            {loading ? 'Logging in...' : 'Send'}
          </button>
        </div>
      </div>
    </div>
  );
}