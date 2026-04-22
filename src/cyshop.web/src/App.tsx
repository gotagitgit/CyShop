import { BrowserRouter, Routes, Route } from 'react-router';
import Layout from './components/Layout';
import CatalogPage from './pages/CatalogPage';
import ProductDetailPage from './pages/ProductDetailPage';
import BasketPage from './pages/BasketPage';
import CheckoutPage from './pages/CheckoutPage';
import NotFoundPage from './pages/NotFoundPage';
import AuthCallbackPage from './pages/AuthCallbackPage';
import ProfilePage from './pages/ProfilePage';
import OrdersPage from './pages/OrdersPage';
import { AuthGuard } from './auth/AuthGuard';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route index element={<CatalogPage />} />
          <Route path="product/:id" element={<ProductDetailPage />} />
          <Route path="basket" element={<BasketPage />} />
          <Route path="checkout" element={<CheckoutPage />} />
          <Route path="auth/callback" element={<AuthCallbackPage />} />
          <Route path="profile" element={<AuthGuard><ProfilePage /></AuthGuard>} />
          <Route path="orders" element={<OrdersPage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
