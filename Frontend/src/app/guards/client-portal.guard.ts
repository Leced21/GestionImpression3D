import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { ClientPortalAuthService } from '../services/client-portal-auth.service';

@Injectable({ providedIn: 'root' })
export class ClientPortalGuard implements CanActivate {
  constructor(private clientPortalAuth: ClientPortalAuthService, private router: Router) {}

  canActivate(): boolean | UrlTree {
    if (!this.clientPortalAuth.isLoggedIn()) {
      return this.router.parseUrl('/portail/demande-acces');
    }

    return true;
  }
}
