import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { PrintJob, PrintJobStatistics } from '../../../models/print-job.model';
import { PrintJobService } from '../../../services/print-job.service';

@Component({
  selector: 'app-print-job-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './print-job-list.html',
  styleUrl: './print-job-list.css',
})
export class PrintJobList implements OnInit {
  jobs: PrintJob[] = [];
  statistics?: PrintJobStatistics;
  activeTab: string = 'all';

  constructor(
    private printJobService: PrintJobService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadJobs();
    this.loadStatistics();
  }
  get filteredJobs(): PrintJob[] {
    switch (this.activeTab) {
      case 'active': return this.jobs.filter(j => j.status === 'Printing' || j.status === 'Paused');
      case 'queue': return this.jobs.filter(j => j.status === 'Pending' || j.status === 'Queued');
      case 'completed': return this.jobs.filter(j => j.status === 'Completed');
      default: return this.jobs;
    }
  }

  loadJobs(): void {
    this.printJobService.getAll().subscribe({
      next: (jobs) => {
        this.jobs = jobs;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error fetching print jobs:', error);
      }
    });
  }
  loadStatistics(): void {
    this.printJobService.getStatistics().subscribe({
      next: (stats) => {
        this.statistics = stats;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error fetching print job statistics:', error);
      }
    });
  }

  getDurationString(minutes?: number): string {
    if (!minutes) return '-';
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return h > 0 ? `${h}h${m.toString().padStart(2, '0')}` : `${m}min`;
  }

  startJob(job: PrintJob): void {
    this.printJobService.start(job.id).subscribe({
      next: () => this.loadJobs(),
      error: (error) => console.error('Error starting print job:', error)
    });
  }

  pauseJob(job: PrintJob): void {
    this.printJobService.pause(job.id).subscribe({
      next: () => this.loadJobs(),
      error: (error) => console.error('Error pausing print job:', error)
    });
  }

  resumeJob(job: PrintJob): void {
    this.printJobService.resume(job.id).subscribe({
      next: () => this.loadJobs(),
      error: (error) => console.error('Error resuming print job:', error)
    });
  }

  completeJob(job: PrintJob): void {
    const duration = prompt('Durée réelle (minutes):', job.estimatedDurationMinutes?.toString());
    const material = prompt('Matériau consommé (grammes):', job.estimatedMaterialGrams?.toString());
    this.printJobService.complete(job.id, duration ? parseInt(duration) : undefined, material ? parseFloat(material) : undefined)
      .subscribe(() => this.loadJobs());
  }
  failJob(job: PrintJob): void {
    const reason = prompt('Raison de l\'échec:');
    if (reason) {
      this.printJobService.fail(job.id, reason).subscribe(() => this.loadJobs());
    }
  }
}
