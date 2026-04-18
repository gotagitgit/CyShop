export interface CustomerAddress {
  id: string;
  customerId: string;
  label: string;
  street: string;
  city: string;
  state: string;
  country: string;
  zipCode: string;
  isDefault: boolean;
  createdDateTime: string;
}
