import { useEffect } from 'react';
import { AuthGuard } from '../auth/AuthGuard';
import { useAppDispatch, useAppSelector } from '../store/hooks';
import { fetchOrders } from '../store/ordersSlice';
import type { OrderSummary } from '../types/order';

function OrderCard({ order }: { order: OrderSummary }) {
  const { street, city, state, country, zipCode } = order.shippingAddress;

  return (
    <div
      style={{
        background: 'var(--color-surface)',
        border: '1px solid var(--color-border)',
        borderRadius: 'var(--radius)',
        boxShadow: 'var(--shadow-sm)',
        padding: '1.25rem',
        transition: 'box-shadow 0.2s',
      }}
      onMouseEnter={(e) => {
        (e.currentTarget as HTMLDivElement).style.boxShadow = 'var(--shadow-md)';
      }}
      onMouseLeave={(e) => {
        (e.currentTarget as HTMLDivElement).style.boxShadow = 'var(--shadow-sm)';
      }}
    >
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem' }}>
        <span style={{ color: 'var(--color-text-muted)', fontSize: '0.85rem' }}>
          {new Date(order.orderDate).toLocaleDateString()}
        </span>
        <span
          style={{
            fontSize: '0.8rem',
            fontWeight: 600,
            padding: '0.2rem 0.6rem',
            borderRadius: 'var(--radius)',
            background: 'var(--color-bg, #f8f9fa)',
            color: 'var(--color-text)',
          }}
        >
          {order.status}
        </span>
      </div>

      <div style={{ fontSize: '1.25rem', fontWeight: 600, color: 'var(--color-text)', marginBottom: '0.5rem' }}>
        ${order.totalAmount.toFixed(2)}
      </div>

      <div style={{ color: 'var(--color-text-muted)', fontSize: '0.9rem', marginBottom: '0.5rem' }}>
        {order.itemCount} {order.itemCount === 1 ? 'item' : 'items'}
      </div>

      <div style={{ color: 'var(--color-text-muted)', fontSize: '0.85rem' }}>
        {street}, {city}, {state}, {country} {zipCode}
      </div>
    </div>
  );
}

function OrdersContent() {
  const dispatch = useAppDispatch();
  const { orders, status, error } = useAppSelector((state) => state.orders);

  useEffect(() => {
    dispatch(fetchOrders());
  }, [dispatch]);

  if (status === 'loading') {
    return <div role="status">Loading orders...</div>;
  }

  if (status === 'failed') {
    return <div role="alert">{error ?? 'Failed to load orders'}</div>;
  }

  if (status === 'succeeded' && orders.length === 0) {
    return <p>You have no orders yet.</p>;
  }

  return (
    <div
      style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))',
        gap: '1.5rem',
      }}
    >
      {orders.map((order) => (
        <OrderCard key={order.orderId} order={order} />
      ))}
    </div>
  );
}

export default function OrdersPage() {
  return (
    <AuthGuard>
      <main>
        <h1>My Orders</h1>
        <OrdersContent />
      </main>
    </AuthGuard>
  );
}
