import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { WalkInComponent } from './walk-in.component';

describe('WalkInComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WalkInComponent],
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

  it('should render the walk-in form for the assigned queue', () => {
    const fixture = TestBed.createComponent(WalkInComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushTodayQueue(http, true);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('#walk-in-page-title')?.textContent).toContain('Walk-in');
    expect(compiled.textContent).toContain('Priya Dental Clinic');
    expect(compiled.textContent).toContain('Manual walk-in');
    expect(compiled.querySelector('aside nav')?.textContent).toContain('Today queue');
  });

  it('should submit a walk-in customer', () => {
    const fixture = TestBed.createComponent(WalkInComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushTodayQueue(http, true);
    fixture.detectChanges();

    (fixture.componentInstance as unknown as { walkInForm: { customerName: string } }).walkInForm.customerName = 'Neha Rao';
    fixture.debugElement.query(By.css('.walk-in-form')).triggerEventHandler('ngSubmit');

    const request = http.expectOne('http://localhost:5020/api/manager/queue/walk-in');
    expect(request.request.method).toBe('POST');
    expect(request.request.body.customerName).toBe('Neha Rao');
    request.flush(createTodayQueue(true, {
      waitingEntries: [createEntry(11, 2, 'Neha Rao')]
    }));
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('1 waiting customer');
  });

  it('should show closed queue feedback when walk-in is rejected', () => {
    const fixture = TestBed.createComponent(WalkInComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushTodayQueue(http, true);
    fixture.detectChanges();

    (fixture.componentInstance as unknown as { walkInForm: { customerName: string } }).walkInForm.customerName = 'Neha Rao';
    fixture.debugElement.query(By.css('.walk-in-form')).triggerEventHandler('ngSubmit');

    const request = http.expectOne('http://localhost:5020/api/manager/queue/walk-in');
    request.flush(
      { status: 409, title: 'Queue is closed.', detail: 'This queue is currently closed.' },
      { status: 409, statusText: 'Conflict' });
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('This queue is currently closed.');
  });

  function flushTodayQueue(http: HttpTestingController, isQueueOpen: boolean): void {
    const request = http.expectOne('http://localhost:5020/api/manager/queue/today');
    expect(request.request.method).toBe('GET');
    request.flush(createTodayQueue(isQueueOpen));
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

  function createEntry(queueEntryId: number, tokenNumber: number, customerName: string): object {
    return {
      queueEntryId,
      tokenNumber,
      customerName,
      mobile: null,
      partySize: null,
      serviceReason: null,
      status: 'Waiting',
      callCount: 0,
      skipCount: 0
    };
  }
});
