export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message: string;
  errors: string[];
}
export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
