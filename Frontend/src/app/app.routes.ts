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
];
