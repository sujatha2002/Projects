import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { AnalyticsService } from '../../../core/services/analytics.service';
import { AnalyticsDashboard } from '../../../core/models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  template: `
    <div class="page">
      <header>
        <h1>Analytics Dashboard</h1>
        <p>AI-powered insights for Foodie Cart</p>
      </header>

      @if (data) {
        <div class="insights-row">
          @for (insight of data.trendingInsights; track insight.dishName) {
            <div class="insight-card">
              <span class="trend" [class]="insight.trend.toLowerCase()">{{ insight.trend }}</span>
              <h3>{{ insight.dishName }}</h3>
              <p>{{ insight.insight }}</p>
              <div class="insight-meta">
                <span>Age: {{ insight.ageGroup }}</span>
                <span>Repeats: {{ insight.repeatOrders }}</span>
              </div>
            </div>
          }
        </div>

        <div class="charts-grid">
          <div class="chart-card">
            <h2>Popular Dishes</h2>
            <canvas baseChart [data]="popularChartData" [options]="barOptions" type="bar"></canvas>
          </div>
          <div class="chart-card">
            <h2>User Activity (7 Days)</h2>
            <canvas baseChart [data]="activityChartData" [options]="lineOptions" type="line"></canvas>
          </div>
          <div class="chart-card">
            <h2>Cart Abandonment by Category</h2>
            <canvas baseChart [data]="abandonmentChartData" [options]="doughnutOptions" type="doughnut"></canvas>
          </div>
        </div>

        <div class="table-section">
          <h2>Popular Dishes Detail</h2>
          <table>
            <thead>
              <tr>
                <th>Dish</th>
                <th>Orders</th>
                <th>Repeat Rate</th>
                <th>Top Age Group</th>
              </tr>
            </thead>
            <tbody>
              @for (dish of data.popularDishes; track dish.name) {
                <tr>
                  <td>{{ dish.name }}</td>
                  <td>{{ dish.orderCount }}</td>
                  <td>{{ dish.repeatRate }}%</td>
                  <td>{{ dish.ageGroup }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      } @else {
        <div class="loading">Loading analytics...</div>
      }
    </div>
  `,
  styles: [`
    .page { max-width: 1200px; margin: 0 auto; padding: 2rem; }
    header { margin-bottom: 2rem; }
    header h1 { color: #2c3e50; margin-bottom: 0.3rem; }
    header p { color: #7f8c8d; }
    .insights-row { display: grid; grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); gap: 1rem; margin-bottom: 2rem; }
    .insight-card { background: white; border-radius: 12px; padding: 1.25rem; box-shadow: 0 2px 10px rgba(0,0,0,0.06); }
    .trend { font-size: 0.75rem; padding: 0.2rem 0.6rem; border-radius: 10px; font-weight: 600; }
    .trend.rising { background: #d5f5e3; color: #27ae60; }
    .trend.stable { background: #ebf5fb; color: #2980b9; }
    .trend.emerging { background: #fef9e7; color: #f39c12; }
    .insight-card h3 { margin: 0.5rem 0; color: #2c3e50; }
    .insight-card p { color: #7f8c8d; font-size: 0.9rem; margin: 0; }
    .insight-meta { display: flex; gap: 1rem; margin-top: 0.75rem; font-size: 0.8rem; color: #95a5a6; }
    .charts-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(350px, 1fr)); gap: 1.5rem; margin-bottom: 2rem; }
    .chart-card { background: white; border-radius: 12px; padding: 1.5rem; box-shadow: 0 2px 10px rgba(0,0,0,0.06); }
    .chart-card h2 { margin: 0 0 1rem; color: #2c3e50; font-size: 1.1rem; }
    .table-section { background: white; border-radius: 12px; padding: 1.5rem; box-shadow: 0 2px 10px rgba(0,0,0,0.06); }
    table { width: 100%; border-collapse: collapse; }
    th, td { padding: 0.75rem; text-align: left; border-bottom: 1px solid #f0f0f0; }
    th { color: #7f8c8d; font-weight: 600; font-size: 0.85rem; }
    .loading { text-align: center; padding: 4rem; color: #95a5a6; }
  `]
})
export class DashboardComponent implements OnInit {
  data: AnalyticsDashboard | null = null;

  popularChartData: ChartConfiguration<'bar'>['data'] = { labels: [], datasets: [] };
  activityChartData: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [] };
  abandonmentChartData: ChartConfiguration<'doughnut'>['data'] = { labels: [], datasets: [] };

  barOptions: ChartConfiguration<'bar'>['options'] = { responsive: true, plugins: { legend: { display: false } } };
  lineOptions: ChartConfiguration<'line'>['options'] = { responsive: true };
  doughnutOptions: ChartConfiguration<'doughnut'>['options'] = { responsive: true };

  constructor(private analyticsService: AnalyticsService) {}

  ngOnInit() {
    this.analyticsService.getDashboard().subscribe(d => {
      this.data = d;
      this.buildCharts(d);
    });
  }

  private buildCharts(d: AnalyticsDashboard) {
    this.popularChartData = {
      labels: d.popularDishes.map(p => p.name),
      datasets: [{ data: d.popularDishes.map(p => p.orderCount), backgroundColor: '#e74c3c' }]
    };

    this.activityChartData = {
      labels: d.userActivity.map(a => a.date),
      datasets: [
        { label: 'Active Users', data: d.userActivity.map(a => a.activeUsers), borderColor: '#3498db', tension: 0.3 },
        { label: 'Orders', data: d.userActivity.map(a => a.orders), borderColor: '#e74c3c', tension: 0.3 },
        { label: 'Signups', data: d.userActivity.map(a => a.newSignups), borderColor: '#27ae60', tension: 0.3 }
      ]
    };

    this.abandonmentChartData = {
      labels: d.cartAbandonment.map(c => c.category),
      datasets: [{
        data: d.cartAbandonment.map(c => c.abandonmentRate),
        backgroundColor: ['#e74c3c', '#f39c12', '#3498db', '#27ae60', '#9b59b6']
      }]
    };
  }
}
