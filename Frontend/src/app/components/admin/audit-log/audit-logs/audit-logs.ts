import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { AuditLogFilters } from '../../audit-log-filters/audit-log-filters/audit-log-filters';
import { AuditLog, AuditLogFilter } from '../../../../models/audit-log.model';
import { AuditLogApiService } from '../../../../services/audit-log-api.service';
import { AuditLogFormatterService } from '../../../../services/audit-log-formatter.service';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [CommonModule, AuditLogFilters],
  templateUrl: './audit-logs.html',
  styleUrl: './audit-logs.css',
})
export class AuditLogs {
  logs: AuditLog[] = [];
  filteredLogs: AuditLog[] = [];
  isLoading = false;
  currentFilter: AuditLogFilter = {};

  constructor(
    private auditLogApi: AuditLogApiService,
    public formatter: AuditLogFormatterService,
  ) { }

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.isLoading = true;
    this.auditLogApi.getRecentLogs(200).subscribe({
      next: (data) => {
        this.logs = data;
        this.applyFilter();
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
      }
    });
  }

  refresh(): void {
    this.loadLogs();
  }

  applyFilters(filter: AuditLogFilter): void {
    this.currentFilter = filter;
    this.applyFilter();
  }

  private applyFilter(): void {
    let result = [...this.logs];

    if (this.currentFilter.action !== undefined) {
      result = result.filter(l => l.action === this.currentFilter.action);
    }
    if (this.currentFilter.entityType !== undefined) {
      result = result.filter(l => l.entityType === this.currentFilter.entityType);
    }
    if (this.currentFilter.entityId) {
      result = result.filter(l => l.entityId === this.currentFilter.entityId);
    }
    if (this.currentFilter.userEmail) {
      const email = this.currentFilter.userEmail.toLowerCase();
      result = result.filter(l => l.userEmail?.toLowerCase().includes(email));
    }

    this.filteredLogs = result;
  }
}
