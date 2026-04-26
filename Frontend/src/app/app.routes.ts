import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'leads',
    pathMatch: 'full',
  },
  {
    path: 'leads',
    loadChildren: () =>
      import('./pages/leads/leads.routes').then((m) => m.LeadsRoutes),
  },
  {
    path: '**',
    redirectTo: 'leads',
  },
];
