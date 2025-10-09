import axios, { AxiosResponse } from 'axios';
import { Product, ProductCreateRequest, ProductUpdateRequest, ProductsResponse } from '../types';

// Ensure the API base URL always includes the `/api` path.
const rawBase = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';
const API_BASE_URL = rawBase.endsWith('/api') || rawBase.endsWith('/api/')
  ? rawBase.replace(/\/$/, '') // strip trailing slash
  : rawBase.replace(/\/$/, '') + '/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add request interceptor to log requests
api.interceptors.request.use(
  (config) => {
    console.log(`Making ${config.method?.toUpperCase()} request to: ${config.url}`);
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Add response interceptor to handle errors
api.interceptors.response.use(
  (response: AxiosResponse) => {
    return response;
  },
  (error) => {
    console.error('API Error:', error);
    if (error.response?.status === 404) {
      console.error('Resource not found');
    } else if (error.response?.status >= 500) {
      console.error('Server error');
    }
    return Promise.reject(error);
  }
);

export const productService = {
  // Get all products with optional search and pagination
  getProducts: async (search: string = '', page: number = 1, pageSize: number = 10): Promise<ProductsResponse> => {
    try {
      const params = new URLSearchParams();
      if (search) params.append('search', search);
      params.append('page', page.toString());
      params.append('pageSize', pageSize.toString());
      
      const response = await api.get<Product[]>(`/products?${params}`);
      
      return {
        products: response.data,
        totalCount: parseInt(response.headers['x-total-count'] || '0'),
        page: parseInt(response.headers['x-page'] || '1'),
        pageSize: parseInt(response.headers['x-page-size'] || '10'),
      };
    } catch (error) {
      console.error('Error fetching products:', error);
      throw error;
    }
  },

  // Get a single product by ID
  getProduct: async (id: number): Promise<Product> => {
    try {
      const response = await api.get<Product>(`/products/${id}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching product:', error);
      throw error;
    }
  },

  // Create a new product
  createProduct: async (productData: ProductCreateRequest): Promise<Product> => {
    try {
      const response = await api.post<Product>('/products', productData);
      return response.data;
    } catch (error) {
      console.error('Error creating product:', error);
      throw error;
    }
  },

  // Update an existing product
  updateProduct: async (id: number, productData: ProductUpdateRequest): Promise<Product> => {
    try {
      const response = await api.put<Product>(`/products/${id}`, { ...productData, id });
      return response.data;
    } catch (error) {
      console.error('Error updating product:', error);
      throw error;
    }
  },

  // Delete a product
  deleteProduct: async (id: number): Promise<{ message: string }> => {
    try {
      const response = await api.delete<{ message: string }>(`/products/${id}`);
      return response.data;
    } catch (error) {
      console.error('Error deleting product:', error);
      throw error;
    }
  },
};

export default api;