import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { productService } from '../services/api';
import { Product } from '../types';
import './ProductDetail.css';

const ProductDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [product, setProduct] = useState<Product | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    const fetchProduct = async () => {
      if (!id) {
        setError('Product ID not found');
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError('');
        const productData = await productService.getProduct(parseInt(id));
        setProduct(productData);
      } catch (err) {
        console.error('Error fetching product:', err);
        setError('Product not found or failed to load');
      } finally {
        setLoading(false);
      }
    };

    fetchProduct();
  }, [id]);

  const handleDelete = async () => {
    if (!product || !window.confirm('Are you sure you want to delete this product?')) {
      return;
    }

    try {
      await productService.deleteProduct(product.id);
      navigate('/', { replace: true });
    } catch (err) {
      console.error('Error deleting product:', err);
      setError('Failed to delete product. Please try again.');
    }
  };

  const handleEdit = () => {
    if (product) {
      navigate(`/edit-product/${product.id}`);
    }
  };

  if (loading) {
    return (
      <div className="product-detail">
        <div className="loading">
          <div className="loading-spinner"></div>
          <p>Loading product...</p>
        </div>
      </div>
    );
  }

  if (error || !product) {
    return (
      <div className="product-detail">
        <div className="error">
          <h2>😕 {error || 'Product not found'}</h2>
          <p>The product you're looking for doesn't exist or has been removed.</p>
          <button onClick={() => navigate('/')} className="btn btn-primary">
            Back to Home
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="product-detail fade-in">
      <div className="product-detail-header">
        <button onClick={() => navigate('/')} className="btn btn-secondary">
          ← Back to Products
        </button>
      </div>

      <div className="product-detail-content">
        <div className="product-detail-image">
          {product.image ? (
            <img 
              src={product.image} 
              alt={product.name}
              onError={(e) => {
                const target = e.target as HTMLImageElement;
                target.src = 'https://via.placeholder.com/500x500?text=No+Image';
              }}
            />
          ) : (
            <div className="product-image-placeholder">
              📦 No Image Available
            </div>
          )}
        </div>

        <div className="product-detail-info">
          <h1 className="product-detail-name">{product.name}</h1>
          
          <div className="product-detail-price">
            ${product.price.toFixed(2)}
          </div>

          <div className="product-detail-description">
            <h3>Description</h3>
            <p>{product.description}</p>
          </div>

          <div className="product-detail-meta">
            <div className="meta-item">
              <strong>Product ID:</strong> {product.id}
            </div>
            <div className="meta-item">
              <strong>Created:</strong> {new Date(product.createdAt).toLocaleDateString()}
            </div>
            <div className="meta-item">
              <strong>Updated:</strong> {new Date(product.updatedAt).toLocaleDateString()}
            </div>
          </div>

          <div className="product-detail-actions">
            <button onClick={handleEdit} className="btn btn-primary">
              Edit Product
            </button>
            <button onClick={handleDelete} className="btn btn-danger">
              Delete Product
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProductDetail;