import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { DashboardPeriod, DashboardSummary, SalesChartPoint } from '../../../core/models/dashboard';
import { DashboardService } from '../../services/dashboard.service';
import { User } from '../../../core/models/user';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  summary: DashboardSummary | null = null;
  currentUser: User | null = null;
  loading = true;
  error = '';
  selectedPeriod: DashboardPeriod = 'today';

  periodOptions: { value: DashboardPeriod; label: string }[] = [
    { value: 'today', label: 'Today' },
    { value: 'week', label: 'This Week' },
    { value: 'month', label: 'This Month' },
    { value: 'year', label: 'This Year' }
  ];

  chartPeriod = 'month';

  constructor(
    private dashboardService: DashboardService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.authService.currentUser.subscribe(user => {
      this.currentUser = user;
    });
    this.loadSummary();
  }

  loadSummary(): void {
    this.loading = true;
    this.error = '';
    this.dashboardService.getSummary(this.selectedPeriod).subscribe({
      next: (data) => {
        this.summary = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load dashboard data';
        this.loading = false;
      }
    });
  }

  onPeriodChange(period: DashboardPeriod): void {
    this.selectedPeriod = period;
    this.loadSummary();
  }

  get displayName(): string {
    const name = this.currentUser?.username || 'User';
    return name.charAt(0).toUpperCase() + name.slice(1);
  }

  formatCurrency(value: number): string {
    return `৳ ${value.toLocaleString('en-BD', { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`;
  }

  formatTrend(value: number): string {
    const sign = value >= 0 ? '+' : '';
    return `${sign}${value.toFixed(1)}% vs last period`;
  }

  getExpiryClass(days: number): string {
    if (days <= 14) return 'critical';
    if (days <= 21) return 'warning';
    return 'caution';
  }

  getStatusClass(status: string): string {
    return status.toLowerCase();
  }

  get chartPoints(): SalesChartPoint[] {
    return this.summary?.salesChart || [];
  }

  get chartPath(): string {
    const points = this.chartPoints;
    if (points.length === 0) return '';

    const width = 600;
    const height = 200;
    const padding = 20;
    const max = Math.max(...points.map(p => p.amount), 1);

    const coords = points.map((point, index) => {
      const x = padding + (index / (points.length - 1 || 1)) * (width - padding * 2);
      const y = height - padding - (point.amount / max) * (height - padding * 2);
      return `${x},${y}`;
    });

    return `M ${coords.join(' L ')}`;
  }

  get chartAreaPath(): string {
    const points = this.chartPoints;
    if (points.length === 0) return '';

    const width = 600;
    const height = 200;
    const padding = 20;
    const max = Math.max(...points.map(p => p.amount), 1);

    const coords = points.map((point, index) => {
      const x = padding + (index / (points.length - 1 || 1)) * (width - padding * 2);
      const y = height - padding - (point.amount / max) * (height - padding * 2);
      return { x, y };
    });

    const line = coords.map(c => `${c.x},${c.y}`).join(' L ');
    const last = coords[coords.length - 1];
    const first = coords[0];
    return `M ${first.x},${height - padding} L ${line} L ${last.x},${height - padding} Z`;
  }
}
