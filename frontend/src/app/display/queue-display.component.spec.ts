import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { QueueDisplayComponent } from './queue-display.component';

vi.mock('qrcode', () => ({
  default: {
    toDataURL: vi.fn().mockResolvedValue('data:image/png;base64,qr-code')
  }
}));

describe('QueueDisplayComponent', () => {
  beforeEach(async () => {
    vi.useFakeTimers();

    await TestBed.configureTestingModule({
      imports: [QueueDisplayComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: (key: string) => key === 'locationCode' ? 'AB7K2M9Q' : null
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

  it('should load display state from the location code route parameter', async () => {
    const baseElement = document.querySelector('base') ?? document.head.appendChild(document.createElement('base'));
    const originalBaseHref = baseElement?.getAttribute('href') ?? '/';
    baseElement?.setAttribute('href', '/QueueManagementApp/');

    const fixture = TestBed.createComponent(QueueDisplayComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();

    flushDisplay(http);
    await Promise.resolve();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Priya Dental Clinic');
    expect(compiled.querySelector('#display-title')?.textContent).toContain('4');
    expect(compiled.textContent).toContain('Amit Kumar');
    expect(compiled.textContent).toContain(`${globalThis.location.origin}/QueueManagementApp/join/AB7K2M9Q`);
    expect(compiled.querySelector('.join-qr-card img')?.getAttribute('src')).toContain('data:image/png');
    expect(compiled.textContent).toContain('Last served');
    expect(compiled.textContent).toContain('Neha Rao');

    baseElement?.setAttribute('href', originalBaseHref);
  });

  it('should poll the display API every 3 seconds', () => {
    const fixture = TestBed.createComponent(QueueDisplayComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushDisplay(http);

    vi.advanceTimersByTime(3000);

    const request = http.expectOne('http://localhost:5020/api/locations/AB7K2M9Q/display');
    expect(request.request.method).toBe('GET');
    request.flush(createDisplay());
  });

  it('should update the last updated label locally every second', () => {
    const fixture = TestBed.createComponent(QueueDisplayComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushDisplay(http);
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Last updated 0 seconds ago');

    vi.advanceTimersByTime(1000);
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Last updated 1 seconds ago');
  });

  it('should show a refresh failure message while keeping existing display visible', () => {
    const fixture = TestBed.createComponent(QueueDisplayComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushDisplay(http);
    fixture.detectChanges();

    vi.advanceTimersByTime(3000);
    const request = http.expectOne('http://localhost:5020/api/locations/AB7K2M9Q/display');
    request.flush({ title: 'Server error' }, { status: 500, statusText: 'Server Error' });
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Priya Dental Clinic');
    expect(fixture.debugElement.query(By.css('[role="alert"]')).nativeElement.textContent)
      .toContain('Could not refresh the display');
  });

  function flushDisplay(http: HttpTestingController): void {
    const request = http.expectOne('http://localhost:5020/api/locations/AB7K2M9Q/display');
    expect(request.request.method).toBe('GET');
    request.flush(createDisplay());
  }

  function createDisplay(): object {
    return {
      locationCode: 'AB7K2M9Q',
      businessName: 'Priya Dental Clinic',
      isQueueOpen: true,
      currentCalledTokenNumber: 4,
      currentCalledCustomerName: 'Amit Kumar',
      lastServedTokenNumber: 3,
      lastServedCustomerName: 'Neha Rao',
      waitingCount: 6
    };
  }
});
