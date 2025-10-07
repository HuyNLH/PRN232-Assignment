export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  image?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ProductCreateRequest {
  name: string;
  description: string;
  price: number;
  image?: string;
}

export interface ProductUpdateRequest extends ProductCreateRequest {
  id: number;
}

export interface ProductsResponse {
  products: Product[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ApiError {
  message: string;
  errors?: { [key: string]: string[] };
}