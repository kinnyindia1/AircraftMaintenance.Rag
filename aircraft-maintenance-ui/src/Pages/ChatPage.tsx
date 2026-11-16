import { useNavigate } from 'react-router-dom';
import ChatWindow from '../Componenets/ChatWindow';
import { logout } from '../Services/api';

export default function ChatPage() {
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div>
      <div className="d-flex justify-content-end p-3 border-bottom">
        <button onClick={handleLogout} className="btn btn-outline-danger btn-sm">
          Logout
        </button>
      </div>
      <ChatWindow />
    </div>
  );
}