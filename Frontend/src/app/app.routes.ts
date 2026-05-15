import { Routes } from '@angular/router';
import { PieceList } from './components/piece-list/piece-list';
import { PieceForm } from './components/piece-form/piece-form';
import { PieceDetail } from './components/piece-detail/piece-detail';
import { PieceKanban } from './components/piece-kanban/piece-kanban';
import { CommercialCatalog } from './components/commercial-catalog/commercial-catalog';
import { Dashboard } from './components/dashboard/dashboard';

export const routes: Routes = [
    { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
    { path: 'pieces', component: PieceList },
    { path: 'pieces/nouveau', component: PieceForm },
    { path: 'pieces/:id', component: PieceDetail },
    { path: 'pieces/:id/edit', component: PieceForm },
    { path: 'kanban', component: PieceKanban },
    { path: 'commercial', component: CommercialCatalog },
    { path: 'dashboard', component: Dashboard}
];
