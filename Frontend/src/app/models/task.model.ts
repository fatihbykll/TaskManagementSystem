export enum TaskStatus {
  Pending = 0, InProgress = 1, Completed = 2, Cancelled = 3
}
export enum TaskPriority {
  Low = 0, Medium = 1, High = 2, Critical = 3
}
export interface TaskItem {
  id: string; title: string; description: string;
  status: TaskStatus; priority: TaskPriority;
  dueDate: string | null; categoryId: string | null;
  createdAt: string; updatedAt: string | null;
}
export interface CreateTaskRequest {
  title: string; description: string; priority: TaskPriority;
  dueDate: string | null; categoryId: string | null;
}
export interface UpdateTaskRequest {
  title: string; description: string; priority: TaskPriority;
  dueDate: string | null; categoryId: string | null;
}
export interface TaskFilter {
  searchTerm?: string;
  status?: TaskStatus;
  priority?: TaskPriority;
  startDate?: string;
  endDate?: string;
  pageNumber: number;
  pageSize: number;
  sortBy?: 'title' | 'dueDate' | 'priority' | 'createdAt';
  sortDirection?: 'asc' | 'desc';
}
export interface TaskStatistics {
  totalTasks: number; pendingCount: number; inProgressCount: number;
  completedCount: number; cancelledCount: number;
  overdueCount: number; completedThisMonth: number;
}
