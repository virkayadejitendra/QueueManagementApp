import { Routes } from '@angular/router';
import { LoginComponent } from './auth/login.component';
import { CustomerJoinComponent } from './customer-join/customer-join.component';
import { CustomerStatusPlaceholderComponent } from './customer-status/customer-status-placeholder.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { OwnerRegistrationComponent } from './owner-registration/owner-registration.component';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'login'
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'register',
    component: OwnerRegistrationComponent
  },
  {
    path: 'join/:locationCode',
    component: CustomerJoinComponent
  },
  {
    path: 'status/:queueEntryId/:trackingToken',
    component: CustomerStatusPlaceholderComponent
  },
  {
    path: 'dashboard',
    component: DashboardComponent
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];
