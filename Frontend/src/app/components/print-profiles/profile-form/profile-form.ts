import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Printer } from '../../../models/printer.model';
import { PrintProfileService } from '../../../services/print-profile.service';
import { PrinterService } from '../../../services/printer.service';

@Component({
  selector: 'app-profile-form',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './profile-form.html',
  styleUrl: './profile-form.css',
})
export class ProfileForm implements OnInit {
  profileForm!: FormGroup;
  isEditMode = false;
  profileId?: number;
  printers: Printer[] = [];

  constructor(
    private fb: FormBuilder,
    private profileService: PrintProfileService,
    private printerService: PrinterService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.initForm();
    this.loadPrinters();
    this.profileId = this.route.snapshot.params['id'];
    if (this.profileId) {
      this.isEditMode = true;
      this.loadProfile();
    }
  }
  initForm(): void {
    this.profileForm = this.fb.group({
      nom: ['', Validators.required],
      description: [''],
      printerId: ['', Validators.required],
      materiau: ['PLA'],
      nozzleTemp: [210],
      bedTemp: [60],
      layerHeight: [0.20],
      speed: [60],
      infill: [20],
      infillPattern: ['Gyroid'],
      supports: [false],
      supportType: ['Tree'],
      materialMultiplier: [1.0],
      isDefault: [false],
      isActive: [true]
    });
  }
  loadPrinters(): void {
    this.printerService.getAll().subscribe({
      next: (data) => {
        this.printers = data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erreur lors du chargement des imprimantes', err)
      }
    });
  }
  loadProfile(): void {
    this.profileService.getById(this.profileId!).subscribe({
      next: (data) => {
        this.profileForm.patchValue(data);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erreur lors du chargement du profil d\'impression', err)
      }
    });
  }
  onSubmit(): void {
    if (this.profileForm.invalid) return;

    if (this.isEditMode && this.profileId) {
      this.profileService.update(this.profileId, this.profileForm.value).subscribe(() => this.router.navigate(['/profils-impression']));
    } else {
      this.profileService.create(this.profileForm.value).subscribe(() => this.router.navigate(['/profils-impression']));
    }
  }
}
