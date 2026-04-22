export interface OrderItemData {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
}

export interface ShippingAddressData {
  street: string;
  city: string;
  state: string;
  country: string;
  zipCode: string;
}

export interface CreateOrderRequest {
  customerName: string;
  items: OrderItemData[];
  shippingAddress: ShippingAddressData;
}

export interface OrderSummary {
  orderId: string;
  orderDate: string;
  status: string;
  customerName: string;
  totalAmount: number;
  itemCount: number;
  shippingAddress: ShippingAddressData;
}
