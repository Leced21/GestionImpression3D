import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { ClientPortalAuthService } from '../../../services/client-portal-auth.service';

@Component({
  selector: 'app-consume',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './consume.html',
  styleUrl: './consume.css',
})
export class Consume implements OnInit {
  isValidating = true;
  hasError = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private clientPortalAuth: ClientPortalAuthService
  ) {}

  ngOnInit(): void {
    const token = this.route.snapshot.queryParams['token'];
    if (!token) {
      this.isValidating = false;
      this.hasError = true;
      return;
    }

    this.clientPortalAuth.consume(token).subscribe({
      next: () => {
        this.router.navigate(['/portail']);
      },
      error: () => {
        this.isValidating = false;
        this.hasError = true;
      },
    });
  }
}
