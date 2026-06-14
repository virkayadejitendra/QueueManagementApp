import { Routes } from '@angular/router';
import { LoginComponent } from './auth/login.component';
import { CustomerJoinComponent } from './customer-join/customer-join.component';
import { CustomerStatusComponent } from './customer-status/customer-status.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { QueueDisplayComponent } from './display/queue-display.component';
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
    component: CustomerStatusComponent
  },
  {
    path: 'display/:locationCode',
    component: QueueDisplayComponent
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
