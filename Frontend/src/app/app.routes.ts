import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';
import { ClientPortalGuard } from './guards/client-portal.guard';
import { Settings } from './features/settings/settings';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./components/login/login').then((m) => m.Login),
  },
  {
    path: 'portail/demande-acces',
    loadComponent: () => import('./features/client-portal/request-access/request-access').then((m) => m.RequestAccess),
  },
  {
    path: 'portail/acces',
    loadComponent: () => import('./features/client-portal/consume/consume').then((m) => m.Consume),
  },
  {
    path: 'portail',
    loadComponent: () => import('./features/client-portal/portal-home/portal-home').then((m) => m.PortalHome),
    canActivate: [ClientPortalGuard],
  },
  {
    path: 'register',
    loadComponent: () => import('./components/register/register').then((m) => m.Register),
  },
  {
    path: 'pieces',
    loadComponent: () => import('./components/piece-list/piece-list').then((m) => m.PieceList),
    canActivate: [AuthGuard],
  },
  {
    path: 'pieces/nouveau',
    loadComponent: () => import('./components/piece-form/piece-form').then((m) => m.PieceForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'pieces/:id',
    loadComponent: () => import('./components/piece-detail/piece-detail').then((m) => m.PieceDetail),
    canActivate: [AuthGuard],
  },
  {
    path: 'pieces/:id/edit',
    loadComponent: () => import('./components/piece-form/piece-form').then((m) => m.PieceForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'kanban',
    loadComponent: () => import('./components/piece-kanban/piece-kanban').then((m) => m.PieceKanban),
    canActivate: [AuthGuard],
  },
  {
    path: 'commercial',
    loadComponent: () => import('./components/commercial-catalog/commercial-catalog').then((m) => m.CommercialCatalog),
    canActivate: [AuthGuard],
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./components/dashboard/dashboard').then((m) => m.Dashboard),
    canActivate: [AuthGuard],
  },
  {
    path: 'projets',
    loadComponent: () => import('./components/project-list/project-list').then((m) => m.ProjectList),
    canActivate: [AuthGuard],
  },
  {
    path: 'projets/nouveau',
    loadComponent: () => import('./components/projet-form/projet-form').then((m) => m.ProjetForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'projets/:id',
    loadComponent: () => import('./components/projet-detail/projet-detail').then((m) => m.ProjetDetail),
    canActivate: [AuthGuard],
  },
  {
    path: 'projets/:id/edit',
    loadComponent: () => import('./components/projet-form/projet-form').then((m) => m.ProjetForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'admin/audit-logs',
    loadComponent: () => import('./components/admin/audit-log/audit-logs/audit-logs').then((m) => m.AuditLogs),
    canActivate: [AuthGuard],
    data: { roles: ['Admin'] },
  },
  {
    path: 'printers',
    loadComponent: () => import('./components/printers/printer-list/printer-list').then((m) => m.PrinterList),
    canActivate: [AuthGuard],
  },
  {
    path: 'printers/new',
    loadComponent: () => import('./components/printers/printer-form/printer-form').then((m) => m.PrinterForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'printers/:id',
    loadComponent: () => import('./components/printers/printer-detail/printer-detail').then((m) => m.PrinterDetail),
    canActivate: [AuthGuard],
  },
  {
    path: 'printers/:id/edit',
    loadComponent: () => import('./components/printers/printer-form/printer-form').then((m) => m.PrinterForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'print-jobs',
    loadComponent: () => import('./components/printers/print-job-list/print-job-list').then((m) => m.PrintJobList),
    canActivate: [AuthGuard],
  },
  {
    path: 'print-jobs/new',
    loadComponent: () => import('./components/printers/print-job-form/print-job-form').then((m) => m.PrintJobForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'print-jobs/:id',
    loadComponent: () => import('./components/printers/print-job-detail/print-job-detail').then((m) => m.PrintJobDetail),
    canActivate: [AuthGuard],
  },
  {
    path: 'stock',
    loadComponent: () => import('./components/material/materialstock-list/materialstock-list').then((m) => m.MaterialstockList),
    canActivate: [AuthGuard],
  },
  {
    path: 'stock/new',
    loadComponent: () => import('./components/material/materialstock-form/materialstock-form').then((m) => m.MaterialstockForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'stock/:id',
    loadComponent: () => import('./components/material/materialstock-detail/materialstock-detail').then((m) => m.MaterialstockDetail),
    canActivate: [AuthGuard],
  },
  {
    path: 'stock/:id/edit',
    loadComponent: () => import('./components/material/materialstock-form/materialstock-form').then((m) => m.MaterialstockForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'admin/users',
    loadComponent: () => import('./components/admin/user/user-list/user-list').then((m) => m.UserList),
    canActivate: [AuthGuard],
    data: { roles: ['Admin'] },
  },
  {
    path: 'accept-invitation',
    loadComponent: () => import('./features/auth/accept-invitation/accept-invitation').then((m) => m.AcceptInvitation),
  },
  {
    path: 'pieces/:id/versions',
    loadComponent: () => import('./components/piece-versions/piece-versions').then((m) => m.PieceVersions),
    canActivate: [AuthGuard],
  },
  {
    path: 'notifications',
    loadComponent: () => import('./components/notification-center/notification-center').then((m) => m.NotificationCenter),
    canActivate: [AuthGuard],
  },
  {
    path: 'unauthorized',
    loadComponent: () => import('./components/unauthorized/unauthorized').then((m) => m.Unauthorized),
  },
  {
    path: 'ordres',
    loadComponent: () => import('./components/ordres-fabrication/ordre-list/ordre-list').then((m) => m.OrdreList),
    canActivate: [AuthGuard],
  },
  {
    path: 'ordres/nouveau',
    loadComponent: () => import('./components/ordres-fabrication/ordre-form/ordre-form').then((m) => m.OrdreForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'ordres/:id',
    loadComponent: () => import('./components/ordres-fabrication/ordre-detail/ordre-detail').then((m) => m.OrdreDetail),
    canActivate: [AuthGuard],
  },
  {
    path: 'ordres/:id/edit',
    loadComponent: () => import('./components/ordres-fabrication/ordre-form/ordre-form').then((m) => m.OrdreForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'profils-impression',
    loadComponent: () => import('./components/print-profiles/profile-list/profile-list').then((m) => m.ProfileList),
    canActivate: [AuthGuard],
  },
  {
    path: 'profils-impression/nouveau',
    loadComponent: () => import('./components/print-profiles/profile-form/profile-form').then((m) => m.ProfileForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'profils-impression/:id',
    loadComponent: () => import('./components/print-profiles/profile-detail/profile-detail').then((m) => m.ProfileDetail),
    canActivate: [AuthGuard],
  },
  {
    path: 'profils-impression/:id/edit',
    loadComponent: () => import('./components/print-profiles/profile-form/profile-form').then((m) => m.ProfileForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'consommations',
    loadComponent: () => import('./features/consumption/consumption-list/consumption-list').then((m) => m.ConsumptionList),
    canActivate: [AuthGuard],
  },
  {
    path: 'consommations/nouveau',
    loadComponent: () => import('./features/consumption/consumption-form/consumption-form').then((m) => m.ConsumptionForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'maintenances',
    loadComponent: () => import('./features/maintenance/maintenance-list/maintenance-list').then((m) => m.MaintenanceList),
    canActivate: [AuthGuard],
  },
  {
    path: 'maintenances/nouveau',
    loadComponent: () => import('./features/maintenance/maintenance-form/maintenance-form').then((m) => m.MaintenanceForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'maintenances/:id/edit',
    loadComponent: () => import('./features/maintenance/maintenance-form/maintenance-form').then((m) => m.MaintenanceForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'maintenances/:id',
    loadComponent: () => import('./features/maintenance/maintenance-detail/maintenance-detail').then((m) => m.MaintenanceDetail),
    canActivate: [AuthGuard],
  },
  {
    path: 'incidents',
    loadComponent: () => import('./features/incidents/incident-list/incident-list').then((m) => m.IncidentList),
    canActivate: [AuthGuard],
  },
  {
    path: 'incidents/nouveau',
    loadComponent: () => import('./features/incidents/incident-form/incident-form').then((m) => m.IncidentForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'incidents/:id',
    loadComponent: () => import('./features/incidents/incident-detail/incident-detail').then((m) => m.IncidentDetail),
    canActivate: [AuthGuard],
  },
  {
    path: 'clients',
    loadComponent: () => import('./features/clients/client-list/client-list').then((m) => m.ClientList),
    canActivate: [AuthGuard],
  },
  {
    path: 'clients/nouveau',
    loadComponent: () => import('./features/clients/client-form/client-form').then((m) => m.ClientForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'clients/:id',
    loadComponent: () => import('./features/clients/client-detail/client-detail').then((m) => m.ClientDetail),
    canActivate: [AuthGuard],
  },
  {
    path: 'clients/:id/edit',
    loadComponent: () => import('./features/clients/client-form/client-form').then((m) => m.ClientForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'devis',
    loadComponent: () => import('./features/devis/devis-list/devis-list').then((m) => m.DevisList),
    canActivate: [AuthGuard],
  },
  {
    path: 'devis/nouveau',
    loadComponent: () => import('./features/devis/devis-form/devis-form').then((m) => m.DevisForm),
    canActivate: [AuthGuard],
  },
  {
    path: 'devis/:id',
    loadComponent: () => import('./features/devis/devis-detail/devis-detail').then((m) => m.DevisDetail),
    canActivate: [AuthGuard],
  },
  {
    path: 'devis/:id/edit',
    loadComponent: () => import('./features/devis/devis-form/devis-form').then((m) => m.DevisForm),
    canActivate: [AuthGuard],
  },
  { path: 'settings', component: Settings, canActivate: [AuthGuard] },
  {
    path: '**',
    loadComponent: () => import('./components/not-found/not-found').then((m) => m.NotFound),
  },
];
