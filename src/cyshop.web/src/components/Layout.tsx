import { useEffect } from 'react';
import { Link, Outlet } from 'react-router';
import { useAuthStatus } from '../auth/useAuthStatus';
import { useAppSelector, useAppDispatch } from '../store/hooks';
import { fetchBasket } from '../store/basketSlice';

export default function Layout() {
  const { isAuthenticated, login, logout } = useAuthStatus();
  const dispatch = useAppDispatch();
  const itemCount = useAppSelector((state) =>
    state.basket.basket?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0,
  );

  useEffect(() => {
    if (isAuthenticated) {
      dispatch(fetchBasket());
    }
  }, [isAuthenticated, dispatch]);

  return (
    <>
      <header>
        <nav aria-label="Main navigation">
          <Link to="/">Catalog</Link>
          <Link to="/basket">
            Basket{itemCount > 0 && <span aria-label={`${itemCount} items in basket`}> ({itemCount})</span>}
          </Link>
          {isAuthenticated ? (
            <button type="button" onClick={logout}>Logout</button>
          ) : (
            <button type="button" onClick={login}>Login</button>
          )}
        </nav>
      </header>
      <main>
        <Outlet />
      </main>
    </>
  );
}
