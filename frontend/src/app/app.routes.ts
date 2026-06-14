import { Routes } from '@angular/router';
import { LoginComponent } from './auth/login.component';
import { CustomerJoinComponent } from './customer-join/customer-join.component';
import { CustomerStatusComponent } from './customer-status/customer-status.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { QueueDisplayComponent } from './display/queue-display.component';
import { JoinQrComponent } from './join-qr/join-qr.component';
import { OwnerRegistrationComponent } from './owner-registration/owner-registration.component';
import { TodayQueueComponent } from './today-queue/today-queue.component';
import { WalkInComponent } from './walk-in/walk-in.component';

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
    path: 'today-queue',
    component: TodayQueueComponent
  },
  {
    path: 'walk-in',
    component: WalkInComponent
  },
  {
    path: 'join-qr',
    component: JoinQrComponent
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];
