import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { TodayQueueComponent } from './today-queue.component';

describe('TodayQueueComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TodayQueueComponent],
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

  it('should render today queue lists and current called customer', () => {
    const fixture = TestBed.createComponent(TodayQueueComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushTodayQueue(http, {
      currentCalled: createEntry(10, 1, 'Amit Kumar', 'Called'),
      waitingEntries: [createEntry(11, 2, 'Neha Rao')],
      skippedEntries: [createEntry(12, 3, 'Ravi Mehta', 'Skipped')],
      recentServedEntries: [createEntry(13, 4, 'Sara Khan', 'Served')]
    });
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('#today-queue-title')?.textContent).toContain('Today queue');
    expect(compiled.textContent).toContain('Current called customer');
    expect(compiled.textContent).toContain('Amit Kumar');
    expect(compiled.textContent).toContain('Waiting list');
    expect(compiled.textContent).toContain('Neha Rao');
    expect(compiled.querySelector('aside nav')?.textContent).toContain('Walk-in');
  });

  it('should mark a waiting customer as skipped', () => {
    const fixture = TestBed.createComponent(TodayQueueComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushTodayQueue(http, {
      waitingEntries: [createEntry(11, 2, 'Neha Rao')]
    });
    fixture.detectChanges();

    clickButton(fixture.nativeElement as HTMLElement, 'Skip');

    const request = http.expectOne('http://localhost:5020/api/manager/queue/entries/11/skipped');
    expect(request.request.method).toBe('POST');
    request.flush(createTodayQueue({
      skippedEntries: [createEntry(11, 2, 'Neha Rao', 'Skipped')]
    }));
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Skipped');
  });

  function flushTodayQueue(
    http: HttpTestingController,
    overrides: Partial<ReturnType<typeof createTodayQueue>> = {}): void {
    const request = http.expectOne('http://localhost:5020/api/manager/queue/today');
    expect(request.request.method).toBe('GET');
    request.flush(createTodayQueue(overrides));
  }

  function createTodayQueue(overrides: object = {}): {
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
      isQueueOpen: true,
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
      skipCount: status === 'Skipped' ? 1 : 0
    };
  }

  function clickButton(compiled: HTMLElement, label: string): void {
    const button = Array.from(compiled.querySelectorAll('button'))
      .find(candidate => candidate.textContent?.trim() === label);

    expect(button).toBeTruthy();
    button?.click();
  }
});
