import React from 'react';
import { Link } from 'react-router-dom';
import { Product } from '../types';
import './ProductCard.css';

interface ProductCardProps {
  product: Product;
  onDelete?: (id: number) => void;
}

const ProductCard: React.FC<ProductCardProps> = ({ product, onDelete }) => {
  const handleDelete = () => {
    if (onDelete && window.confirm('Are you sure you want to delete this product?')) {
      onDelete(product.id);
    }
  };

  return (
    <div className="product-card fade-in">
      <div className="product-image-container">
        {product.image ? (
          <img 
            src={product.image} 
            alt={product.name}
            className="product-image"
            onError={(e) => {
              const target = e.target as HTMLImageElement;
              target.src = 'https://via.placeholder.com/300x300?text=No+Image';
            }}
          />
        ) : (
          <div className="product-image-placeholder">
            📦 No Image
          </div>
        )}
      </div>
      
      <div className="product-info">
        <h3 className="product-name">{product.name}</h3>
        <p className="product-description">{product.description}</p>
        <div className="product-price">${product.price.toFixed(2)}</div>
        
        <div className="product-actions">
          <Link to={`/product/${product.id}`} className="btn btn-secondary btn-small">
            View Details
          </Link>
          <Link to={`/edit-product/${product.id}`} className="btn btn-primary btn-small">
            Edit
          </Link>
          {onDelete && (
            <button 
              onClick={handleDelete} 
              className="btn btn-danger btn-small"
            >
              Delete
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

export default ProductCard;