import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { PieceVersion } from '../../models/piece-version.model';
import { PieceVersionService } from '../../services/pieceversion.service';

@Component({
  selector: 'app-piece-versions',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './piece-versions.html',
  styleUrls: ['./piece-versions.css'],
})
export class PieceVersions implements OnInit {
  @Input() pieceId!: number;
  versions: PieceVersion[] = [];
  showNewVersionModal = false;
  changeLog = '';
  isPrototype = true;

  constructor(
    private versionService: PieceVersionService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadVersions();
  }

  loadVersions(): void {
    this.versionService.getVersionsByPiece(this.pieceId).subscribe({
      next: (data) => {
        this.versions = data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err)
        this.cdr.detectChanges();}
    });
  }

  createVersion(): void {
    if (!this.changeLog) return;
    
    this.versionService.createVersion(this.pieceId, this.changeLog, this.isPrototype).subscribe({
      next: () => {
        this.loadVersions();
        this.closeModal();
      },
      error: (err) => console.error(err)
    });
  }

  promoteToProduction(versionId: number): void {
    this.versionService.promoteToProduction(versionId).subscribe({
      next: () => this.loadVersions(),
      error: (err) => console.error(err)
    });
  }

  compareVersions(v1Id: number, v2Id: number): void {
    this.versionService.compareVersions(v1Id, v2Id).subscribe({
      next: (result) => {
        if (result.hasChanges) {
          alert('Des différences ont été détectées entre les versions');
        } else {
          alert('Les versions sont identiques');
        }
      },
      error: (err) => console.error(err)
    });
  }

  closeModal(): void {
    this.showNewVersionModal = false;
    this.changeLog = '';
    this.isPrototype = true;
  }

  closeModalOnOverlay(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal')) {
      this.closeModal();
    }
  }
}


