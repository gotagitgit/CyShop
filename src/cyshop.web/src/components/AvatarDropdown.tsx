import { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router';
import { useAuthStatus } from '../auth/useAuthStatus';

export default function AvatarDropdown() {
  const { isAuthenticated, login, logout } = useAuthStatus();
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;

    function handleClickOutside(event: MouseEvent) {
      if (
        containerRef.current &&
        !containerRef.current.contains(event.target as Node)
      ) {
        setOpen(false);
      }
    }

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [open]);

  if (!isAuthenticated) {
    return (
      <button type="button" onClick={login}>
        Login
      </button>
    );
  }

  return (
    <div className="avatar-dropdown" ref={containerRef}>
      <button
        type="button"
        className="avatar-button"
        aria-haspopup="true"
        aria-expanded={open}
        aria-label="User menu"
        onClick={() => setOpen((prev) => !prev)}
      >
        <span aria-hidden="true">👤</span>
      </button>

      {open && (
        <div className="avatar-menu" role="menu">
          <button
            type="button"
            className="avatar-menu-item"
            role="menuitem"
            onClick={() => {
              setOpen(false);
              navigate('/profile');
            }}
          >
            Profile
          </button>
          <button
            type="button"
            className="avatar-menu-item"
            role="menuitem"
            onClick={() => {
              setOpen(false);
              logout();
            }}
          >
            Logout
          </button>
        </div>
      )}
    </div>
  );
}
