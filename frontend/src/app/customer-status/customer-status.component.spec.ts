import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { CustomerStatusComponent } from './customer-status.component';

describe('CustomerStatusComponent', () => {
  beforeEach(async () => {
    vi.useFakeTimers();

    await TestBed.configureTestingModule({
      imports: [CustomerStatusComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: (key: string) => {
                  if (key === 'queueEntryId') {
                    return '10';
                  }

                  if (key === 'trackingToken') {
                    return 'tracking-token';
                  }

                  return null;
                }
              }
            }
          }
        }
      ]
    }).compileComponents();
  });

  afterEach(() => {
    TestBed.inject(HttpTestingController).verify();
    vi.useRealTimers();
  });

  it('should load private status from route parameters', () => {
    const fixture = TestBed.createComponent(CustomerStatusComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();

    flushStatus(http);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h2')?.textContent).toContain('Token 3');
    expect(compiled.textContent).toContain('Queue position');
    expect(compiled.textContent).toContain('Now calling');
  });

  it('should poll the status API every 5 seconds', () => {
    const fixture = TestBed.createComponent(CustomerStatusComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushStatus(http);

    vi.advanceTimersByTime(5000);

    const request = http.expectOne(request =>
      request.url === 'http://localhost:5020/api/queue-entries/10/status'
      && request.params.get('trackingToken') === 'tracking-token');
    expect(request.request.method).toBe('GET');
    request.flush(createStatus());
  });

  it('should update the last updated label locally every second', () => {
    const fixture = TestBed.createComponent(CustomerStatusComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushStatus(http);
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Last updated 0 seconds ago');

    vi.advanceTimersByTime(1000);
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Last updated 1 seconds ago');
  });

  it('should show a refresh failure message while keeping existing status visible', () => {
    const fixture = TestBed.createComponent(CustomerStatusComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushStatus(http);
    fixture.detectChanges();

    vi.advanceTimersByTime(5000);
    const request = http.expectOne(request =>
      request.url === 'http://localhost:5020/api/queue-entries/10/status'
      && request.params.get('trackingToken') === 'tracking-token');
    request.flush({ title: 'Server error' }, { status: 500, statusText: 'Server Error' });
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Token 3');
    expect(fixture.debugElement.query(By.css('[role="alert"]')).nativeElement.textContent)
      .toContain('Could not refresh your queue status');
  });

  function flushStatus(http: HttpTestingController): void {
    const request = http.expectOne(request =>
      request.url === 'http://localhost:5020/api/queue-entries/10/status'
      && request.params.get('trackingToken') === 'tracking-token');
    expect(request.request.method).toBe('GET');
    request.flush(createStatus());
  }

  function createStatus(): object {
    return {
      queueEntryId: 10,
      locationCode: 'AB7K2M9Q',
      businessName: 'Priya Dental Clinic',
      tokenNumber: 3,
      status: 'Waiting',
      queuePosition: 2,
      waitingCount: 4,
      currentCalledTokenNumber: 1,
      createdAt: '2026-06-14T09:15:00Z',
      calledAt: null,
      servedAt: null,
      cancelledAt: null
    };
  }
});
