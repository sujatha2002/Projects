import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Recipe, RecipeSuggestion } from '../models';

@Injectable({ providedIn: 'root' })
export class RecipeService {
  private readonly api = `${environment.apiUrl}/recipes`;

  constructor(private http: HttpClient) {}

  getAll(category?: string) {
    const params = category ? `?category=${category}` : '';
    return this.http.get<Recipe[]>(`${this.api}${params}`);
  }

  getById(id: number) {
    return this.http.get<Recipe>(`${this.api}/${id}`);
  }

  getSuggestions(category?: string) {
    const params = category ? `?category=${category}` : '';
    return this.http.get<RecipeSuggestion[]>(`${this.api}/suggestions${params}`);
  }

  getTrending() {
    return this.http.get<RecipeSuggestion[]>(`${this.api}/trending`);
  }
}
