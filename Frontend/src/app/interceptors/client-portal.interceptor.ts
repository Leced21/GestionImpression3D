import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ClientPortalAuthService } from '../services/client-portal-auth.service';
import { API_BASE_URL } from '../config/api.config';

// N'attache le token du portail client qu'aux endpoints de données du portail
// (devis/factures/commandes) — jamais aux endpoints internes, ni à request-access/consume
// qui sont anonymes par nature (c'est justement ce qu'ils servent à authentifier).
const PORTAL_DATA_PREFIX = `${API_BASE_URL}/client-portal/`;
const PORTAL_ANONYMOUS_PATHS = ['request-access', 'consume'];

@Injectable()
export class ClientPortalInterceptor implements HttpInterceptor {
  constructor(private clientPortalAuth: ClientPortalAuthService) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const isPortalDataRequest =
      req.url.startsWith(PORTAL_DATA_PREFIX) &&
      !PORTAL_ANONYMOUS_PATHS.some((path) => req.url.startsWith(`${PORTAL_DATA_PREFIX}${path}`));

    if (!isPortalDataRequest) {
      return next.handle(req);
    }

    const token = this.clientPortalAuth.getToken();
    const authReq = token
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

    return next.handle(authReq);
  }
}
