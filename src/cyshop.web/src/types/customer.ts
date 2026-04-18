export interface CustomerProfile {
  id: string;
  externalId: string;
  firstName: string;
  lastName: string;
  email: string;
  contactNumber: string;
}

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
