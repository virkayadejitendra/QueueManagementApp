import { routes } from './app.routes';
import { LoginComponent } from './auth/login.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { OwnerRegistrationComponent } from './owner-registration/owner-registration.component';

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
  });
});
