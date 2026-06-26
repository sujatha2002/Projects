import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { AnalyticsDashboard } from '../models';

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private readonly api = `${environment.apiUrl}/analytics`;

  constructor(private http: HttpClient) {}

  getDashboard() {
    return this.http.get<AnalyticsDashboard>(`${this.api}/dashboard`);
  }
}
