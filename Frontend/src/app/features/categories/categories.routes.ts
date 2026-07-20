import { Routes } from '@angular/router';
export const CATEGORIES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./category-list/category-list.component').then(m => m.CategoryListComponent)
  }
];
