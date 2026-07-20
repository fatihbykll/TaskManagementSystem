export interface Category {
  id: string;
  name: string;
  description: string;
  colorCode: string;
  taskCount: number;
}
export interface CreateCategoryRequest {
  name: string;
  description: string;
  colorCode: string;
}
export interface UpdateCategoryRequest {
  name: string;
  description: string;
  colorCode: string;
}
