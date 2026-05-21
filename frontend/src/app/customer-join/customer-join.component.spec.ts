import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { ActivatedRoute, Router, provideRouter } from '@angular/router';
import { CustomerJoinComponent } from './customer-join.component';

describe('CustomerJoinComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CustomerJoinComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
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
  });

  it('should render the public join form', () => {
    const fixture = TestBed.createComponent(CustomerJoinComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Join the queue');
    expect(compiled.querySelector('#join-title')?.textContent).toContain('Customer details');
  });

  it('should post customer details and navigate to the private status url', () => {
    const fixture = TestBed.createComponent(CustomerJoinComponent);
    const http = TestBed.inject(HttpTestingController);
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    fillForm(fixture, {
      customerName: 'Amit Kumar',
      mobile: '9876543210',
      partySize: '2',
      serviceReason: 'Consultation'
    });
    submitForm(fixture);

    const request = http.expectOne('http://localhost:5020/api/locations/AB7K2M9Q/queue-entries');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      customerName: 'Amit Kumar',
      mobile: '9876543210',
      partySize: 2,
      serviceReason: 'Consultation'
    });

    request.flush({
      queueEntryId: 10,
      queueLocationId: 1,
      locationCode: 'AB7K2M9Q',
      tokenNumber: 1,
      status: 'Waiting',
      trackingToken: 'tracking-token',
      statusUrl: '/status/10/tracking-token'
    });

    expect(navigateSpy).toHaveBeenCalledWith('/status/10/tracking-token');
  });

  it('should show closed queue feedback', () => {
    const fixture = TestBed.createComponent(CustomerJoinComponent);
    const http = TestBed.inject(HttpTestingController);

    fillForm(fixture, {
      customerName: 'Amit Kumar'
    });
    submitForm(fixture);

    const request = http.expectOne('http://localhost:5020/api/locations/AB7K2M9Q/queue-entries');
    request.flush(
      { status: 409, title: 'Queue is closed.' },
      { status: 409, statusText: 'Conflict' });
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('[role="alert"]')?.textContent).toContain('This queue is currently closed.');
  });

  function fillForm(
    fixture: ComponentFixture<CustomerJoinComponent>,
    values: Partial<Record<string, string>>): void {
    fixture.detectChanges();

    for (const [controlName, value] of Object.entries(values)) {
      const control = fixture.debugElement.query(By.css(`[formControlName="${controlName}"]`))
        .nativeElement as HTMLInputElement | HTMLTextAreaElement;

      control.value = value ?? '';
      control.dispatchEvent(new Event('input'));
    }

    fixture.detectChanges();
  }

  function submitForm(fixture: ComponentFixture<CustomerJoinComponent>): void {
    fixture.detectChanges();
    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit');
    fixture.detectChanges();
  }
});
