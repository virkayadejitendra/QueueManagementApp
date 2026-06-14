import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { Router, provideRouter } from '@angular/router';
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

  it('should render dashboard overview with manager navigation links', () => {
    const fixture = TestBed.createComponent(DashboardComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushTodayQueue(http, false);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('#dashboard-title')?.textContent).toContain('Dashboard');
    expect(compiled.querySelector('aside nav')?.textContent).toContain('Today queue');
    expect(compiled.querySelector('aside nav')?.textContent).toContain('Walk-in');
    expect(compiled.querySelector('aside nav')?.textContent).toContain('Join QR');
    expect(compiled.querySelector('aside nav')?.textContent).toContain('Display screen');
    expect(compiled.querySelector('.sign-out-button')?.textContent).toContain('Sign out');
    expect(compiled.querySelector('.queue-controls')?.textContent).toContain('AB7K2M9Q');
    expect(compiled.querySelector('.queue-controls')?.textContent).toContain('Create printable QR');
    expect(compiled.textContent).not.toContain('Waiting list');
    expect(compiled.textContent).not.toContain('Manual walk-in');
  });

  it('should clear the auth token and navigate to login when signing out', () => {
    globalThis.localStorage?.setItem(authTokenStorageKey, 'header.payload.signature');

    const fixture = TestBed.createComponent(DashboardComponent);
    const http = TestBed.inject(HttpTestingController);
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    fixture.detectChanges();
    flushTodayQueue(http, false);

    fixture.debugElement.query(By.css('.sign-out-button')).triggerEventHandler('click');

    expect(globalThis.localStorage?.getItem(authTokenStorageKey)).toBeNull();
    expect(navigateSpy).toHaveBeenCalledWith('/login');
  });

  it('should open the queue from the dashboard controls and refresh today state', () => {
    const fixture = TestBed.createComponent(DashboardComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushTodayQueue(http, false);
    fixture.detectChanges();

    clickButton(fixture.nativeElement as HTMLElement, 'Open queue');

    const openRequest = http.expectOne('http://localhost:5020/api/manager/queue/open');
    expect(openRequest.request.method).toBe('POST');
    openRequest.flush(createQueueStatus(true));

    flushTodayQueue(http, true);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.status-chip')?.textContent).toContain('Open');
  });

  it('should call next and refresh the current customer', () => {
    const fixture = TestBed.createComponent(DashboardComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushTodayQueue(http, true, { waitingEntries: [createEntry(10, 1, 'Amit Kumar')] });
    fixture.detectChanges();

    clickButton(fixture.nativeElement as HTMLElement, 'Call next');

    const request = http.expectOne('http://localhost:5020/api/manager/queue/call-next');
    expect(request.request.method).toBe('POST');
    request.flush(createTodayQueue(true, {
      currentCalled: createEntry(10, 1, 'Amit Kumar', 'Called')
    }));
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Current token');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('1');
  });

  it('should show a conflict message when call next is rejected', () => {
    const fixture = TestBed.createComponent(DashboardComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushTodayQueue(http, true, { waitingEntries: [createEntry(10, 1, 'Amit Kumar')] });
    fixture.detectChanges();

    clickButton(fixture.nativeElement as HTMLElement, 'Call next');

    const request = http.expectOne('http://localhost:5020/api/manager/queue/call-next');
    request.flush(
      { status: 409, title: 'Queue state conflict.', detail: 'Another customer is already called for this queue.' },
      { status: 409, statusText: 'Conflict' });
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Another customer is already called');
  });

  function flushTodayQueue(
    http: HttpTestingController,
    isQueueOpen: boolean,
    overrides: Partial<ReturnType<typeof createTodayQueue>> = {}): void {
    const request = http.expectOne('http://localhost:5020/api/manager/queue/today');
    expect(request.request.method).toBe('GET');
    request.flush(createTodayQueue(isQueueOpen, overrides));
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

  function createTodayQueue(isQueueOpen: boolean, overrides: object = {}): {
    queueLocationId: number;
    locationCode: string;
    businessName: string;
    isQueueOpen: boolean;
    waitingCount: number;
    currentCalled: object | null;
    waitingEntries: object[];
    skippedEntries: object[];
    recentServedEntries: object[];
  } {
    const queue = {
      queueLocationId: 1,
      locationCode: 'AB7K2M9Q',
      businessName: 'Priya Dental Clinic',
      isQueueOpen,
      waitingCount: 0,
      currentCalled: null,
      waitingEntries: [],
      skippedEntries: [],
      recentServedEntries: [],
      ...overrides
    };

    return {
      ...queue,
      waitingCount: queue.waitingEntries.length
    };
  }

  function createEntry(queueEntryId: number, tokenNumber: number, customerName: string, status = 'Waiting'): object {
    return {
      queueEntryId,
      tokenNumber,
      customerName,
      mobile: null,
      partySize: null,
      serviceReason: null,
      status,
      callCount: status === 'Called' ? 1 : 0,
      skipCount: 0
    };
  }

  function clickButton(compiled: HTMLElement, label: string): void {
    const button = Array.from(compiled.querySelectorAll('button'))
      .find(candidate => candidate.textContent?.includes(label));

    expect(button).toBeTruthy();
    button?.click();
  }
});
