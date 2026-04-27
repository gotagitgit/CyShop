import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router';
import { useAppDispatch, useAppSelector } from '../store/hooks';
import {
  fetchProfile,
  fetchAddresses,
  createProfile,
  updateProfile,
  deleteProfile,
  createAddress,
  updateAddress,
  deleteAddress,
} from '../store/customerSlice';
import type { CreateCustomerPayload, CreateAddressPayload } from '../api/customersApi';

const emptyForm: CreateCustomerPayload = {
  firstName: '',
  lastName: '',
  email: '',
  contactNumber: '',
};

const emptyAddressForm: CreateAddressPayload = {
  label: '',
  street: '',
  city: '',
  state: '',
  country: '',
  zipCode: '',
  isDefault: false,
};

export default function ProfilePage() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const profile = useAppSelector((state) => state.customer.profile);
  const profileStatus = useAppSelector((state) => state.customer.profileStatus);
  const error = useAppSelector((state) => state.customer.error);

  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState<CreateCustomerPayload>(emptyForm);

  const addresses = useAppSelector((state) => state.customer.addresses);
  const addressStatus = useAppSelector((state) => state.customer.addressStatus);
  const [showAddForm, setShowAddForm] = useState(false);
  const [editingAddressId, setEditingAddressId] = useState<string | null>(null);
  const [addressFormData, setAddressFormData] = useState<CreateAddressPayload>(emptyAddressForm);

  useEffect(() => {
    dispatch(fetchProfile());
    dispatch(fetchAddresses());
  }, [dispatch]);

  useEffect(() => {
    if (profile && isEditing) {
      setFormData({
        firstName: profile.firstName,
        lastName: profile.lastName,
        email: profile.email,
        contactNumber: profile.contactNumber,
      });
    }
  }, [profile, isEditing]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    await dispatch(createProfile(formData));
    setFormData(emptyForm);
  };

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    await dispatch(updateProfile(formData));
    setIsEditing(false);
  };

  const handleDelete = async () => {
    if (window.confirm('Are you sure you want to delete your account?')) {
      const result = await dispatch(deleteProfile());
      if (result.meta.requestStatus !== 'rejected') {
        navigate('/');
      }
    }
  };

  const handleEdit = () => {
    if (profile) {
      setFormData({
        firstName: profile.firstName,
        lastName: profile.lastName,
        email: profile.email,
        contactNumber: profile.contactNumber,
      });
    }
    setIsEditing(true);
  };

  const handleCancelEdit = () => {
    setIsEditing(false);
    setFormData(emptyForm);
  };

  const handleAddressChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type, checked } = e.target;
    setAddressFormData((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
  };

  const handleAddAddress = async (e: React.FormEvent) => {
    e.preventDefault();
    await dispatch(createAddress(addressFormData));
    setAddressFormData(emptyAddressForm);
    setShowAddForm(false);
  };

  const handleEditAddress = (address: typeof addresses[0]) => {
    setEditingAddressId(address.id);
    setAddressFormData({
      label: address.label,
      street: address.street,
      city: address.city,
      state: address.state,
      country: address.country,
      zipCode: address.zipCode,
      isDefault: address.isDefault,
    });
  };

  const handleUpdateAddress = async (e: React.FormEvent) => {
    e.preventDefault();
    if (editingAddressId) {
      await dispatch(updateAddress({ id: editingAddressId, data: addressFormData }));
      setEditingAddressId(null);
      setAddressFormData(emptyAddressForm);
    }
  };

  const handleDeleteAddress = async (addressId: string) => {
    if (window.confirm('Are you sure you want to delete this address?')) {
      await dispatch(deleteAddress(addressId));
    }
  };

  const handleCancelAddressEdit = () => {
    setEditingAddressId(null);
    setAddressFormData(emptyAddressForm);
  };

  const handleCancelAddForm = () => {
    setShowAddForm(false);
    setAddressFormData(emptyAddressForm);
  };

  if (profileStatus === 'loading') {
    return (
      <main className="profile-page">
        <h1>My Profile</h1>
        <div role="status">Loading...</div>
      </main>
    );
  }

  return (
    <main className="profile-page">
      <h1>My Profile</h1>

      {error && <div role="alert">{error}</div>}

      {profileStatus === 'not_found' && (
        <section className="profile-card">
          <h2>Create Your Profile</h2>
          <form className="profile-form" onSubmit={handleCreate}>
            <label>
              First Name
              <input
                type="text"
                name="firstName"
                value={formData.firstName}
                onChange={handleChange}
                required
              />
            </label>
            <label>
              Last Name
              <input
                type="text"
                name="lastName"
                value={formData.lastName}
                onChange={handleChange}
                required
              />
            </label>
            <label>
              Email
              <input
                type="email"
                name="email"
                value={formData.email}
                onChange={handleChange}
                required
              />
            </label>
            <label>
              Contact Number
              <input
                type="text"
                name="contactNumber"
                value={formData.contactNumber}
                onChange={handleChange}
                required
              />
            </label>
            <div className="profile-actions">
              <button type="submit">Create Profile</button>
            </div>
          </form>
        </section>
      )}

      {profileStatus === 'succeeded' && profile && !isEditing && (
        <section className="profile-card">
          <h2>Customer Details</h2>
          <p><strong>First Name:</strong> {profile.firstName}</p>
          <p><strong>Last Name:</strong> {profile.lastName}</p>
          <p><strong>Email:</strong> {profile.email}</p>
          <p><strong>Contact Number:</strong> {profile.contactNumber}</p>
          <div className="profile-actions">
            <button type="button" onClick={handleEdit}>Edit</button>
            <button type="button" onClick={handleDelete}>Delete Account</button>
          </div>
        </section>
      )}

      {profileStatus === 'succeeded' && profile && isEditing && (
        <section className="profile-card">
          <h2>Edit Profile</h2>
          <form className="profile-form" onSubmit={handleUpdate}>
            <label>
              First Name
              <input
                type="text"
                name="firstName"
                value={formData.firstName}
                onChange={handleChange}
                required
              />
            </label>
            <label>
              Last Name
              <input
                type="text"
                name="lastName"
                value={formData.lastName}
                onChange={handleChange}
                required
              />
            </label>
            <label>
              Email
              <input
                type="email"
                name="email"
                value={formData.email}
                onChange={handleChange}
                required
              />
            </label>
            <label>
              Contact Number
              <input
                type="text"
                name="contactNumber"
                value={formData.contactNumber}
                onChange={handleChange}
                required
              />
            </label>
            <div className="profile-actions">
              <button type="submit">Save</button>
              <button type="button" onClick={handleCancelEdit}>Cancel</button>
            </div>
          </form>
        </section>
      )}

      {profileStatus === 'succeeded' && (
        <section className="address-section">
          <h2>Addresses</h2>

          {addressStatus === 'failed' && error && <div role="alert">{error}</div>}

          <div className="address-list">
            {addresses.map((address) =>
              editingAddressId === address.id ? (
                <div key={address.id} className="address-card">
                  <form className="address-form" onSubmit={handleUpdateAddress}>
                    <label>
                      Label
                      <input type="text" name="label" value={addressFormData.label} onChange={handleAddressChange} required />
                    </label>
                    <label>
                      Street
                      <input type="text" name="street" value={addressFormData.street} onChange={handleAddressChange} required />
                    </label>
                    <label>
                      City
                      <input type="text" name="city" value={addressFormData.city} onChange={handleAddressChange} required />
                    </label>
                    <label>
                      State
                      <input type="text" name="state" value={addressFormData.state} onChange={handleAddressChange} required />
                    </label>
                    <label>
                      Country
                      <input type="text" name="country" value={addressFormData.country} onChange={handleAddressChange} required />
                    </label>
                    <label>
                      Zip Code
                      <input type="text" name="zipCode" value={addressFormData.zipCode} onChange={handleAddressChange} required />
                    </label>
                    <label>
                      <input type="checkbox" name="isDefault" checked={addressFormData.isDefault} onChange={handleAddressChange} />
                      Default Address
                    </label>
                    <div className="address-actions">
                      <button type="submit">Save</button>
                      <button type="button" onClick={handleCancelAddressEdit}>Cancel</button>
                    </div>
                  </form>
                </div>
              ) : (
                <div key={address.id} className="address-card">
                  <p><strong>{address.label}</strong>{address.isDefault && <span className="default-badge"> (Default)</span>}</p>
                  <p>{address.street}</p>
                  <p>{address.city}, {address.state}</p>
                  <p>{address.country}, {address.zipCode}</p>
                  <div className="address-actions">
                    <button type="button" onClick={() => handleEditAddress(address)}>Edit</button>
                    <button type="button" onClick={() => handleDeleteAddress(address.id)}>Delete</button>
                  </div>
                </div>
              ),
            )}
          </div>

          {!showAddForm && (
            <button type="button" onClick={() => setShowAddForm(true)}>Add Address</button>
          )}

          {showAddForm && (
            <div className="address-card">
              <form className="address-form" onSubmit={handleAddAddress}>
                <h3>New Address</h3>
                <label>
                  Label
                  <input type="text" name="label" value={addressFormData.label} onChange={handleAddressChange} required />
                </label>
                <label>
                  Street
                  <input type="text" name="street" value={addressFormData.street} onChange={handleAddressChange} required />
                </label>
                <label>
                  City
                  <input type="text" name="city" value={addressFormData.city} onChange={handleAddressChange} required />
                </label>
                <label>
                  State
                  <input type="text" name="state" value={addressFormData.state} onChange={handleAddressChange} required />
                </label>
                <label>
                  Country
                  <input type="text" name="country" value={addressFormData.country} onChange={handleAddressChange} required />
                </label>
                <label>
                  Zip Code
                  <input type="text" name="zipCode" value={addressFormData.zipCode} onChange={handleAddressChange} required />
                </label>
                <label>
                  <input type="checkbox" name="isDefault" checked={addressFormData.isDefault} onChange={handleAddressChange} />
                  Default Address
                </label>
                <div className="address-actions">
                  <button type="submit">Add</button>
                  <button type="button" onClick={handleCancelAddForm}>Cancel</button>
                </div>
              </form>
            </div>
          )}
        </section>
      )}
    </main>
  );
}
