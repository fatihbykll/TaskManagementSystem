import { Routes } from '@angular/router';
export const TASKS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./task-list/task-list.component').then(m => m.TaskListComponent)
  }
];
