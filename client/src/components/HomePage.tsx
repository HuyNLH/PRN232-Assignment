import React, { useState, useEffect, useCallback } from 'react';
import ProductCard from './ProductCard';
import SearchBar from './SearchBar';
import Pagination from './Pagination';
import { productService } from '../services/api';
import { Product } from '../types';
import './HomePage.css';

const HomePage: React.FC = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [totalItems, setTotalItems] = useState<number>(0);
  const [pageSize] = useState<number>(8);

  const fetchProducts = useCallback(async (search: string = '', page: number = 1) => {
    try {
      setLoading(true);
      setError('');
      const response = await productService.getProducts(search, page, pageSize);
      setProducts(response.products);
      setTotalItems(response.totalCount);
      setCurrentPage(response.page);
    } catch (err) {
      console.error('Error fetching products:', err);
      setError('Failed to load products. Please try again.');
      setProducts([]);
    } finally {
      setLoading(false);
    }
  }, [pageSize]);

  useEffect(() => {
    fetchProducts(searchTerm, currentPage);
  }, [searchTerm, currentPage, fetchProducts]);

  const handleSearch = (search: string) => {
    setSearchTerm(search);
    setCurrentPage(1);
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleDelete = async (id: number) => {
    try {
      await productService.deleteProduct(id);
      // Refresh products after deletion
      fetchProducts(searchTerm, currentPage);
    } catch (err) {
      console.error('Error deleting product:', err);
      setError('Failed to delete product. Please try again.');
    }
  };

  if (loading) {
    return (
      <div className="home-page">
        <div className="loading">
          <div className="loading-spinner"></div>
          <p>Loading products...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="home-page">
      <div className="home-header">
        <h1 className="home-title">Clothing E-commerce</h1>
        <p className="home-subtitle">Discover amazing clothing products</p>
      </div>

      <div className="home-search">
        <SearchBar onSearch={handleSearch} placeholder="Search for clothing..." />
      </div>

      {error && (
        <div className="error">
          {error}
          <button onClick={() => fetchProducts(searchTerm, currentPage)} className="btn btn-primary">
            Try Again
          </button>
        </div>
      )}

      {!error && (
        <>
          <div className="products-header">
            <h2>
              {searchTerm ? `Search results for "${searchTerm}"` : 'All Products'}
              <span className="products-count">({totalItems} items)</span>
            </h2>
          </div>

          {products.length === 0 ? (
            <div className="no-products">
              <div className="no-products-icon">🔍</div>
              <h3>No products found</h3>
              <p>
                {searchTerm 
                  ? 'Try adjusting your search terms or browse all products.'
                  : 'No products available at the moment.'
                }
              </p>
              {searchTerm && (
                <button onClick={() => handleSearch('')} className="btn btn-primary">
                  View All Products
                </button>
              )}
            </div>
          ) : (
            <>
              <div className="products-grid">
                {products.map((product) => (
                  <ProductCard
                    key={product.id}
                    product={product}
                    onDelete={handleDelete}
                  />
                ))}
              </div>

              <Pagination
                currentPage={currentPage}
                totalItems={totalItems}
                itemsPerPage={pageSize}
                onPageChange={handlePageChange}
              />
            </>
          )}
        </>
      )}
    </div>
  );
};

export default HomePage;