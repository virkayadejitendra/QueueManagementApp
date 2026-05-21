import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { By } from '@angular/platform-browser';
import { authTokenStorageKey } from '../auth/auth-token-storage';
import { DashboardComponent } from './dashboard.component';

describe('DashboardComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([])
      ]
    }).compileComponents();
  });

  afterEach(() => {
    TestBed.inject(HttpTestingController).verify();
  });

  it('should render dashboard with side navigation', () => {
    const fixture = TestBed.createComponent(DashboardComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushQueueStatus(http, false);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('#dashboard-title')?.textContent).toContain('Dashboard');
    expect(compiled.querySelector('aside nav')?.textContent).toContain('Today queue');
    expect(compiled.querySelector('aside nav')?.textContent).toContain('Display screen');
    expect(compiled.querySelector('.sign-out-button')?.textContent).toContain('Sign out');
    expect(compiled.querySelector('.queue-controls')?.textContent).toContain('AB7K2M9Q');
  });

  it('should clear the auth token and navigate to login when signing out', () => {
    globalThis.localStorage?.setItem(authTokenStorageKey, 'header.payload.signature');

    const fixture = TestBed.createComponent(DashboardComponent);
    const http = TestBed.inject(HttpTestingController);
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    fixture.detectChanges();
    flushQueueStatus(http, false);

    fixture.debugElement.query(By.css('.sign-out-button')).triggerEventHandler('click');

    expect(globalThis.localStorage?.getItem(authTokenStorageKey)).toBeNull();
    expect(navigateSpy).toHaveBeenCalledWith('/login');
  });

  it('should open the queue from the dashboard controls', () => {
    const fixture = TestBed.createComponent(DashboardComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushQueueStatus(http, false);
    fixture.detectChanges();

    fixture.debugElement.query(By.css('.control-actions button')).triggerEventHandler('click');

    const request = http.expectOne('http://localhost:5020/api/manager/queue/open');
    expect(request.request.method).toBe('POST');
    request.flush(createQueueStatus(true));
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.status-chip')?.textContent).toContain('Open');
  });

  function flushQueueStatus(http: HttpTestingController, isQueueOpen: boolean): void {
    const request = http.expectOne('http://localhost:5020/api/manager/queue/status');
    expect(request.request.method).toBe('GET');
    request.flush(createQueueStatus(isQueueOpen));
  }

  function createQueueStatus(isQueueOpen: boolean): object {
    return {
      queueLocationId: 1,
      locationCode: 'AB7K2M9Q',
      businessName: 'Priya Dental Clinic',
      isQueueOpen,
      waitingCount: 0,
      currentTokenNumber: null
    };
  }
});
