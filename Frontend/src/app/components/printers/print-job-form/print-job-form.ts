import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Piece, PieceStatus } from '../../../models/piece.model';
import { PieceService } from '../../../services/piece.service';
import { PrintJobService } from '../../../services/print-job.service';

@Component({
  selector: 'app-print-job-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './print-job-form.html',
  styleUrls: ['./print-job-form.css'],
})
export class PrintJobForm implements OnInit {
  jobForm!: FormGroup;
  pieces: Piece[] = [];

  constructor(
    private fb: FormBuilder,
    private pieceService: PieceService,
    private printJobService: PrintJobService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.initForm();
    this.loadPieces();
  }
  initForm(): void {
    this.jobForm = this.fb.group({
      pieceId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]],
      priority: ['Normal'],
      estimatedDurationMinutes: ['', [Validators.required, Validators.min(1)]],
      estimatedMaterialGrams: ['', [Validators.required, Validators.min(0)]],
      notes: ['']
    });
  }
  loadPieces(): void {
    this.pieceService.getAll().subscribe({
      next: (data) => {
        this.pieces = data.filter(p => p.statut === PieceStatus.Commercialisable);
      }
    });
  }

  onPieceChange(): void {
    const pieceId = this.jobForm.get('pieceId')?.value;
    const piece = this.pieces.find(p => p.id === parseInt(pieceId));
    if (piece) {
      if (!this.jobForm.get('estimatedDurationMinutes')?.value) {
        this.jobForm.patchValue({ estimatedDurationMinutes: 60 });
      }
      if (!this.jobForm.get('estimatedMaterialGrams')?.value) {
        this.jobForm.patchValue({ estimatedMaterialGrams: 50 });
      }
    }
  }
  onSubmit(): void {
    if (this.jobForm.invalid) return;
    this.printJobService.create(this.jobForm.value).subscribe({
      next: () => this.router.navigate(['/print-jobs']),
      error: (err) => console.error('Error creating print job:', err)
    });
  }
}




