import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, tap, shareReplay } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/api-response.model';
import { Category } from '../../models/category.model';
@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Categories`;
  private readonly CACHE_KEY = 'categories_cache';
  
  // RxJS Memory Cache
  private categoriesCache$: Observable<ApiResponse<Category[]>> | null = null;
  getAll(): Observable<ApiResponse<Category[]>> {
    // 1. Memory cache varsa (önceki bir istek shareReplay ile bellekteyse) onu dön
    if (this.categoriesCache$) {
      return this.categoriesCache$;
    }
    // 2. Memory boşsa LocalStorage kontrol et
    const saved = localStorage.getItem(this.CACHE_KEY);
    if (saved) {
      try {
        const parsed = JSON.parse(saved);
        this.categoriesCache$ = of(parsed);
        // Cache'den veri dönerken arkada veriyi yenilemek istersen this.refreshCache() çağrılabilir
        return this.categoriesCache$;
      } catch (e) {
        localStorage.removeItem(this.CACHE_KEY);
      }
    }
    // 3. Hiçbir yerde yoksa API'ye git ve sonuçları cache'le
    return this.fetchFromApi();
  }
  
  private fetchFromApi(): Observable<ApiResponse<Category[]>> {
    this.categoriesCache$ = this.http.get<ApiResponse<Category[]>>(this.baseUrl).pipe(
      tap(res => {
        if (res.success) {
          localStorage.setItem(this.CACHE_KEY, JSON.stringify(res));
        }
      }),
      // shareReplay(1): Birden fazla abone olsa bile sadece 1 HTTP isteği atılmasını sağlar
      shareReplay(1) 
    );
    return this.categoriesCache$;
  }
  refreshCache(): void {
    this.fetchFromApi().subscribe(); // Arka planda veriyi günceller
  }
}
