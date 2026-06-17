import { Routes } from '@angular/router';
import { PieceList } from './components/piece-list/piece-list';
import { PieceForm } from './components/piece-form/piece-form';
import { PieceDetail } from './components/piece-detail/piece-detail';
import { PieceKanban } from './components/piece-kanban/piece-kanban';
import { CommercialCatalog } from './components/commercial-catalog/commercial-catalog';
import { Dashboard } from './components/dashboard/dashboard';
import { Login } from './components/login/login';
import { Register } from './components/register/register';
import { AuthGuard } from './guards/auth.guard';
import { ProjectList } from './components/project-list/project-list';
import { ProjetForm } from './components/projet-form/projet-form';
import { ProjetDetail } from './components/projet-detail/projet-detail';
import { AuditLogs } from './components/admin/audit-log/audit-logs/audit-logs';
import { PrinterList } from './components/printers/printer-list/printer-list';
import { PrinterForm } from './components/printers/printer-form/printer-form';
import { PrinterDetail } from './components/printers/printer-detail/printer-detail';
import { PrintJobList } from './components/printers/print-job-list/print-job-list';
import { PrintJobForm } from './components/printers/print-job-form/print-job-form';
import { PrintJobDetail } from './components/printers/print-job-detail/print-job-detail';
import { MaterialstockList } from './components/material/materialstock-list/materialstock-list';
import { MaterialstockForm } from './components/material/materialstock-form/materialstock-form';
import { MaterialstockDetail } from './components/material/materialstock-detail/materialstock-detail';
import { UserList } from './components/admin/user/user-list/user-list';
import { AcceptInvitation } from './features/auth/accept-invitation/accept-invitation';
import { PieceVersions } from './components/piece-versions/piece-versions';
import { NotificationCenter } from './components/notification-center/notification-center';
import { Unauthorized } from './components/unauthorized/unauthorized';
import { NotFound } from './components/not-found/not-found';
import { OrdreForm } from './components/ordres-fabrication/ordre-form/ordre-form';
import { OrdreList } from './components/ordres-fabrication/ordre-list/ordre-list';
import { OrdreDetail } from './components/ordres-fabrication/ordre-detail/ordre-detail';
import { ProfileList } from './components/print-profiles/profile-list/profile-list';
import { ProfileForm } from './components/print-profiles/profile-form/profile-form';
import { ProfileDetail } from './components/print-profiles/profile-detail/profile-detail';
import { ConsumptionList } from './features/consumption/consumption-list/consumption-list';
import { ConsumptionForm } from './features/consumption/consumption-form/consumption-form';
import { MaintenanceList } from './features/maintenance/maintenance-list/maintenance-list';
import { MaintenanceForm } from './features/maintenance/maintenance-form/maintenance-form';
import { MaintenanceDetail } from './features/maintenance/maintenance-detail/maintenance-detail';
import { IncidentList } from './features/incidents/incident-list/incident-list';
import { IncidentForm } from './features/incidents/incident-form/incident-form';
import { IncidentDetail } from './features/incidents/incident-detail/incident-detail';
import { ClientList } from './features/clients/client-list/client-list';
import { ClientForm } from './features/clients/client-form/client-form';
import { ClientDetail } from './features/clients/client-detail/client-detail';
import { DevisList } from './features/devis/devis-list/devis-list';
import { DevisForm } from './features/devis/devis-form/devis-form';
import { DevisDetail } from './features/devis/devis-detail/devis-detail';

export const routes: Routes = [
    { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
    { path: 'login', component: Login },
    { path: 'register', component: Register },
    { path: 'pieces', component: PieceList, canActivate: [AuthGuard] },
    { path: 'pieces/nouveau', component: PieceForm, canActivate: [AuthGuard] },
    { path: 'pieces/:id', component: PieceDetail, canActivate: [AuthGuard] },
    { path: 'pieces/:id/edit', component: PieceForm, canActivate: [AuthGuard] },
    { path: 'kanban', component: PieceKanban, canActivate: [AuthGuard] },
    { path: 'commercial', component: CommercialCatalog, canActivate: [AuthGuard] },
    { path: 'dashboard', component: Dashboard, canActivate: [AuthGuard] },
    { path: 'projets', component: ProjectList, canActivate: [AuthGuard] },
    { path: 'projets/nouveau', component: ProjetForm, canActivate: [AuthGuard] },
    { path: 'projets/:id', component: ProjetDetail, canActivate: [AuthGuard] },
    { path: 'projets/:id/edit', component: ProjetForm, canActivate: [AuthGuard] },
    { path: 'admin/audit-logs', component: AuditLogs, canActivate: [AuthGuard], data: { roles: ['Admin'] } },
    { path: 'printers', component: PrinterList, canActivate: [AuthGuard] },
    { path: 'printers/new', component: PrinterForm, canActivate: [AuthGuard] },
    { path: 'printers/:id', component: PrinterDetail, canActivate: [AuthGuard] },
    { path: 'printers/:id/edit', component: PrinterForm, canActivate: [AuthGuard] },
    { path: 'print-jobs', component: PrintJobList, canActivate: [AuthGuard] },
    { path: 'print-jobs/new', component: PrintJobForm, canActivate: [AuthGuard] },
    { path: 'print-jobs/:id', component: PrintJobDetail, canActivate: [AuthGuard] },
    { path: 'stock', component: MaterialstockList, canActivate: [AuthGuard] },
    { path: 'stock/new', component: MaterialstockForm, canActivate: [AuthGuard] },
    { path: 'stock/:id', component: MaterialstockDetail, canActivate: [AuthGuard] },
    { path: 'stock/:id/edit', component: MaterialstockForm, canActivate: [AuthGuard] },
    { path: 'admin/users', component: UserList, canActivate: [AuthGuard], data: { roles: ['Admin'] } },
    { path: 'accept-invitation', component: AcceptInvitation },
    { path: 'pieces/:id/versions', component: PieceVersions, canActivate: [AuthGuard] },
    { path: 'notifications', component: NotificationCenter, canActivate: [AuthGuard] },
    { path: 'unauthorized', component: Unauthorized },
    { path: 'ordres', component: OrdreList, canActivate: [AuthGuard] },
    { path: 'ordres/nouveau', component: OrdreForm, canActivate: [AuthGuard] },
    { path: 'ordres/:id', component: OrdreDetail, canActivate: [AuthGuard] },
    { path: 'ordres/:id/edit', component: OrdreForm, canActivate: [AuthGuard] },
    { path: 'profils-impression', component: ProfileList, canActivate: [AuthGuard] },
    { path: 'profils-impression/nouveau', component: ProfileForm, canActivate: [AuthGuard] },
    { path: 'profils-impression/:id', component: ProfileDetail, canActivate: [AuthGuard] },
    { path: 'profils-impression/:id/edit', component: ProfileForm, canActivate: [AuthGuard] },
    { path: 'consommations', component: ConsumptionList, canActivate: [AuthGuard] },
    { path: 'consommations/nouveau', component: ConsumptionForm, canActivate: [AuthGuard] },
    { path: 'maintenances', component: MaintenanceList, canActivate: [AuthGuard] },
    { path: 'maintenances/nouveau', component: MaintenanceForm, canActivate: [AuthGuard] },
    { path: 'maintenances/:id/edit', component: MaintenanceForm, canActivate: [AuthGuard] },
    { path: 'maintenances/:id', component: MaintenanceDetail, canActivate: [AuthGuard] },
    { path: 'incidents', component: IncidentList, canActivate: [AuthGuard] },
    { path: 'incidents/nouveau', component: IncidentForm, canActivate: [AuthGuard] },
    { path: 'incidents/:id', component: IncidentDetail, canActivate: [AuthGuard] },
    { path: 'clients', component: ClientList, canActivate: [AuthGuard] },
    { path: 'clients/nouveau', component: ClientForm, canActivate: [AuthGuard] },
    { path: 'clients/:id', component: ClientDetail, canActivate: [AuthGuard] },
    { path: 'clients/:id/edit', component: ClientForm, canActivate: [AuthGuard] },
    { path: 'devis', component: DevisList, canActivate: [AuthGuard] },
    { path: 'devis/nouveau', component: DevisForm, canActivate: [AuthGuard] },
    { path: 'devis/:id', component: DevisDetail, canActivate: [AuthGuard] },
    { path: '**', component: NotFound }
];
