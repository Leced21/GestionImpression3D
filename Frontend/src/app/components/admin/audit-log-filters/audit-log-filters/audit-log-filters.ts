import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActionType, AuditLogFilter, EntityType } from '../../../../models/audit-log.model';

@Component({
  selector: 'app-audit-log-filters',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './audit-log-filters.html',
  styleUrls: ['./audit-log-filters.css'],
})
export class AuditLogFilters {
  @Output() filtersChange = new EventEmitter<AuditLogFilter>();

  ActionType = ActionType;
  EntityType = EntityType;

  filters: AuditLogFilter = {};

  onFilterChange(): void {
    this.filtersChange.emit(this.filters);
  }

  clearFilters(): void {
    this.filters = {};
    this.filtersChange.emit(this.filters);
  }
}


