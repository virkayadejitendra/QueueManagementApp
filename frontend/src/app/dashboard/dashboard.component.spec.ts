import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { By } from '@angular/platform-browser';
import { authTokenStorageKey } from '../auth/auth-token-storage';
import { DashboardComponent } from './dashboard.component';

describe('DashboardComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [provideRouter([])]
    }).compileComponents();
  });

  it('should render dashboard with side navigation', () => {
    const fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('#dashboard-title')?.textContent).toContain('Dashboard');
    expect(compiled.querySelector('aside nav')?.textContent).toContain('Today queue');
    expect(compiled.querySelector('aside nav')?.textContent).toContain('Display screen');
    expect(compiled.querySelector('.sign-out-button')?.textContent).toContain('Sign out');
  });

  it('should clear the auth token and navigate to login when signing out', () => {
    globalThis.localStorage?.setItem(authTokenStorageKey, 'header.payload.signature');

    const fixture = TestBed.createComponent(DashboardComponent);
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    fixture.detectChanges();

    fixture.debugElement.query(By.css('.sign-out-button')).triggerEventHandler('click');

    expect(globalThis.localStorage?.getItem(authTokenStorageKey)).toBeNull();
    expect(navigateSpy).toHaveBeenCalledWith('/login');
  });
});
