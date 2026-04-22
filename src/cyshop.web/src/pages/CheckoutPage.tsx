import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router';
import { AuthGuard } from '../auth/AuthGuard';
import { useAppDispatch, useAppSelector } from '../store/hooks';
import {
  fetchAddresses,
  setSelectedAddressId,
} from '../store/checkoutSlice';
import { createOrder } from '../store/ordersSlice';
import { fetchProfile } from '../store/customerSlice';

function CheckoutContent() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { selectedItems, addresses, selectedAddressId, addressStatus, addressError } =
    useAppSelector((state) => state.checkout);
  const profile = useAppSelector((state) => state.customer.profile);

  const idempotencyKeyRef = useRef(crypto.randomUUID());
  const [orderLoading, setOrderLoading] = useState(false);
  const [orderError, setOrderError] = useState<string | null>(null);

  useEffect(() => {
    dispatch(fetchAddresses());
    dispatch(fetchProfile());
  }, [dispatch]);

  const selectedAddress = addresses.find((a) => a.id === selectedAddressId);

  const orderTotal = selectedItems.reduce(
    (sum, item) => sum + item.unitPrice * item.quantity,
    0,
  );

  const canPlaceOrder = selectedItems.length > 0 && selectedAddress != null;

  const handlePlaceOrder = async () => {
    if (!canPlaceOrder || !selectedAddress || !profile) return;

    setOrderLoading(true);
    setOrderError(null);

    try {
      await dispatch(
        createOrder({
          request: {
            customerName: `${profile.firstName} ${profile.lastName}`,
            items: selectedItems.map((item) => ({
              productId: item.productId,
              productName: item.productName,
              unitPrice: item.unitPrice,
              quantity: item.quantity,
            })),
            shippingAddress: {
              street: selectedAddress.street,
              city: selectedAddress.city,
              state: selectedAddress.state,
              country: selectedAddress.country,
              zipCode: selectedAddress.zipCode,
            },
          },
          idempotencyKey: idempotencyKeyRef.current,
        }),
      ).unwrap();
      navigate('/orders');
    } catch (err: unknown) {
      const message =
        typeof err === 'string'
          ? err
          : err instanceof Error
            ? err.message
            : 'Failed to place order';
      setOrderError(message);
    } finally {
      setOrderLoading(false);
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '2rem' }}>
      {/* Shipping Address — on top */}
      <section>
        <h2 style={{ fontSize: '1.25rem', fontWeight: 600, marginBottom: '1rem' }}>
          Shipping Address
        </h2>

        {addressStatus === 'loading' && (
          <div role="status" aria-label="Loading addresses">
            <p style={{ color: '#6c757d' }}>Loading addresses…</p>
          </div>
        )}

        {addressStatus === 'failed' && (
          <div role="alert">
            <p>Error loading addresses: {addressError}</p>
          </div>
        )}

        {addressStatus === 'succeeded' && addresses.length === 0 && (
          <p style={{ color: '#6c757d' }}>No shipping addresses available.</p>
        )}

        {addressStatus === 'succeeded' && addresses.length > 0 && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
              <label htmlFor="address-select" style={{ fontWeight: 500, whiteSpace: 'nowrap' }}>
                Address:
              </label>
              <select
                id="address-select"
                value={selectedAddressId ?? ''}
                onChange={(e) => dispatch(setSelectedAddressId(e.target.value))}
                aria-label="Select shipping address"
                style={{
                  padding: '0.5rem 0.75rem',
                  border: '1px solid #dee2e6',
                  borderRadius: '8px',
                  fontSize: '0.95rem',
                  background: '#ffffff',
                  minWidth: '200px',
                }}
              >
                {addresses.map((addr) => (
                  <option key={addr.id} value={addr.id}>
                    {addr.label}
                  </option>
                ))}
              </select>
            </div>

            {selectedAddress && (
              <div
                aria-label="Selected address details"
                style={{
                  padding: '1rem 1.25rem',
                  background: '#f8f9fa',
                  border: '1px solid #dee2e6',
                  borderRadius: '8px',
                  lineHeight: 1.6,
                  color: '#212529',
                }}
              >
                <p style={{ fontWeight: 600, marginBottom: '0.25rem' }}>{selectedAddress.label}</p>
                <p>{selectedAddress.street}</p>
                <p>{selectedAddress.city}, {selectedAddress.state} {selectedAddress.zipCode}</p>
                <p>{selectedAddress.country}</p>
              </div>
            )}
          </div>
        )}
      </section>

      {/* Selected Items — below address */}
      <section>
        <h2 style={{ fontSize: '1.25rem', fontWeight: 600, marginBottom: '1rem' }}>
          Order Items
        </h2>

        {selectedItems.length === 0 ? (
          <p style={{ color: '#6c757d' }}>No items selected for checkout.</p>
        ) : (
          <>
            <table aria-label="Checkout items">
              <thead>
                <tr>
                  <th scope="col">Product</th>
                  <th scope="col">Unit Price</th>
                  <th scope="col">Quantity</th>
                  <th scope="col">Line Total</th>
                </tr>
              </thead>
              <tbody>
                {selectedItems.map((item) => (
                  <tr key={item.productId}>
                    <td>{item.productName}</td>
                    <td>${item.unitPrice.toFixed(2)}</td>
                    <td>{item.quantity}</td>
                    <td>${(item.unitPrice * item.quantity).toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
            </table>

            <div style={{ marginTop: '1.5rem', textAlign: 'right', fontSize: '1.25rem' }}>
              <strong>Total: ${orderTotal.toFixed(2)}</strong>
            </div>
          </>
        )}
      </section>

      {/* Order Error */}
      {orderError && (
        <div role="alert" style={{ color: '#dc3545', padding: '0.75rem', background: '#f8d7da', borderRadius: '8px' }}>
          {orderError}
        </div>
      )}

      {/* Place Order Button */}
      {canPlaceOrder && (
        <div style={{ textAlign: 'right' }}>
          <button
            onClick={handlePlaceOrder}
            disabled={orderLoading}
            style={{
              padding: '0.75rem 2rem',
              fontSize: '1rem',
              fontWeight: 600,
              borderRadius: '8px',
              border: 'none',
              background: orderLoading ? '#6c757d' : '#0d6efd',
              color: '#ffffff',
              cursor: orderLoading ? 'not-allowed' : 'pointer',
            }}
          >
            {orderLoading ? 'Placing Order…' : 'Place Order'}
          </button>
        </div>
      )}
    </div>
  );
}

export default function CheckoutPage() {
  return (
    <AuthGuard>
      <main>
        <h1>Checkout</h1>
        <CheckoutContent />
      </main>
    </AuthGuard>
  );
}
