import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { productService } from '../services/api';
import { ProductCreateRequest } from '../types';
import './ProductForm.css';

const ProductForm: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEditing = Boolean(id);

  const [formData, setFormData] = useState<ProductCreateRequest>({
    name: '',
    description: '',
    price: 0,
    image: '',
  });
  
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string>('');
  const [success, setSuccess] = useState<string>('');

  useEffect(() => {
    if (isEditing && id) {
      const fetchProduct = async () => {
        try {
          setLoading(true);
          const product = await productService.getProduct(parseInt(id));
          setFormData({
            name: product.name,
            description: product.description,
            price: product.price,
            image: product.image || '',
          });
        } catch (err) {
          console.error('Error fetching product:', err);
          setError('Failed to load product for editing');
        } finally {
          setLoading(false);
        }
      };

      fetchProduct();
    }
  }, [id, isEditing]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'price' ? parseFloat(value) || 0 : value,
    }));
  };

  const validateForm = (): boolean => {
    if (!formData.name.trim()) {
      setError('Product name is required');
      return false;
    }
    if (!formData.description.trim()) {
      setError('Product description is required');
      return false;
    }
    if (formData.price <= 0) {
      setError('Price must be greater than 0');
      return false;
    }
    if (formData.image && !isValidUrl(formData.image)) {
      setError('Please enter a valid image URL');
      return false;
    }
    return true;
  };

  const isValidUrl = (string: string): boolean => {
    try {
      new URL(string);
      return true;
    } catch (_) {
      return false;
    }
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (!validateForm()) {
      return;
    }

    try {
      setLoading(true);
      
      if (isEditing && id) {
        await productService.updateProduct(parseInt(id), {
          ...formData,
          id: parseInt(id),
        });
        setSuccess('Product updated successfully!');
      } else {
        await productService.createProduct(formData);
        setSuccess('Product created successfully!');
      }

      setTimeout(() => {
        navigate('/');
      }, 1500);
    } catch (err: any) {
      console.error('Error saving product:', err);
      if (err.response?.data?.errors) {
        const errorMessages = Object.values(err.response.data.errors).flat();
        setError(errorMessages.join(', '));
      } else {
        setError(`Failed to ${isEditing ? 'update' : 'create'} product. Please try again.`);
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="product-form-container fade-in">
      <div className="product-form-header">
        <button onClick={() => navigate('/')} className="btn btn-secondary">
          ← Back to Products
        </button>
        <h1>{isEditing ? 'Edit Product' : 'Add New Product'}</h1>
      </div>

      <div className="product-form-content">
        {error && <div className="error">{error}</div>}
        {success && <div className="success">{success}</div>}

        <form onSubmit={handleSubmit} className="product-form">
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="name">Product Name *</label>
              <input
                type="text"
                id="name"
                name="name"
                value={formData.name}
                onChange={handleInputChange}
                className="form-control"
                placeholder="Enter product name"
                required
                maxLength={100}
              />
            </div>

            <div className="form-group">
              <label htmlFor="price">Price ($) *</label>
              <input
                type="number"
                id="price"
                name="price"
                value={formData.price}
                onChange={handleInputChange}
                className="form-control"
                placeholder="0.00"
                min="0.01"
                step="0.01"
                required
              />
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="description">Description *</label>
            <textarea
              id="description"
              name="description"
              value={formData.description}
              onChange={handleInputChange}
              className="form-control"
              placeholder="Enter product description"
              required
              maxLength={1000}
              rows={4}
            />
            <small className="form-text">
              {formData.description.length}/1000 characters
            </small>
          </div>

          <div className="form-group">
            <label htmlFor="image">Image URL (optional)</label>
            <input
              type="url"
              id="image"
              name="image"
              value={formData.image}
              onChange={handleInputChange}
              className="form-control"
              placeholder="https://example.com/image.jpg"
            />
            <small className="form-text">
              Enter a valid URL to display product image
            </small>
          </div>

          {formData.image && (
            <div className="image-preview">
              <label>Image Preview:</label>
              <div className="preview-container">
                <img 
                  src={formData.image} 
                  alt="Product preview"
                  onError={(e) => {
                    const target = e.target as HTMLImageElement;
                    target.style.display = 'none';
                  }}
                  onLoad={(e) => {
                    const target = e.target as HTMLImageElement;
                    target.style.display = 'block';
                  }}
                />
              </div>
            </div>
          )}

          <div className="form-actions">
            <button
              type="button"
              onClick={() => navigate('/')}
              className="btn btn-secondary"
              disabled={loading}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="btn btn-primary"
              disabled={loading}
            >
              {loading ? 
                (isEditing ? 'Updating...' : 'Creating...') : 
                (isEditing ? 'Update Product' : 'Create Product')
              }
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ProductForm;