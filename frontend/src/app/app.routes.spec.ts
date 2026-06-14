import { routes } from './app.routes';
import { LoginComponent } from './auth/login.component';
import { CustomerJoinComponent } from './customer-join/customer-join.component';
import { CustomerStatusComponent } from './customer-status/customer-status.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { QueueDisplayComponent } from './display/queue-display.component';
import { JoinQrComponent } from './join-qr/join-qr.component';
import { OwnerRegistrationComponent } from './owner-registration/owner-registration.component';
import { TodayQueueComponent } from './today-queue/today-queue.component';
import { WalkInComponent } from './walk-in/walk-in.component';

describe('routes', () => {
  it('should redirect the empty path to login', () => {
    expect(routes).toContainEqual({
      path: '',
      pathMatch: 'full',
      redirectTo: 'login'
    });
  });

  it('should expose login and registration as separate public routes', () => {
    expect(routes).toContainEqual({
      path: 'login',
      component: LoginComponent
    });
    expect(routes).toContainEqual({
      path: 'register',
      component: OwnerRegistrationComponent
    });
  });

  it('should expose dashboard as the authenticated landing route', () => {
    expect(routes).toContainEqual({
      path: 'dashboard',
      component: DashboardComponent
    });
    expect(routes).toContainEqual({
      path: 'today-queue',
      component: TodayQueueComponent
    });
    expect(routes).toContainEqual({
      path: 'walk-in',
      component: WalkInComponent
    });
    expect(routes).toContainEqual({
      path: 'join-qr',
      component: JoinQrComponent
    });
  });

  it('should expose public customer join and private status routes', () => {
    expect(routes).toContainEqual({
      path: 'join/:locationCode',
      component: CustomerJoinComponent
    });
    expect(routes).toContainEqual({
      path: 'status/:queueEntryId/:trackingToken',
      component: CustomerStatusComponent
    });
    expect(routes).toContainEqual({
      path: 'display/:locationCode',
      component: QueueDisplayComponent
    });
  });
});
