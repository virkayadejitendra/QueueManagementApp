import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { By } from '@angular/platform-browser';
import { OwnerRegistrationComponent } from './owner-registration.component';

describe('OwnerRegistrationComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OwnerRegistrationComponent],
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

  it('should render registration form', async () => {
    const fixture = TestBed.createComponent(OwnerRegistrationComponent);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.querySelector('h1')?.textContent).toContain('Register your business queue');
    expect(compiled.querySelector('#registration-title')?.textContent).toContain('Owner registration');
  });

  it('should require either owner email or mobile before submitting registration', () => {
    const fixture = TestBed.createComponent(OwnerRegistrationComponent);

    fillForm(fixture, '.registration-form', {
      ownerName: 'Priya Sharma',
      password: 'StrongPass123',
      businessName: 'Priya Dental Clinic',
      address: '12 MG Road, Bengaluru',
      businessMobile: '9876500000'
    });
    submitForm(fixture, '.registration-form');

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('[role="alert"]')?.textContent).toContain('Enter either the owner email or mobile number.');
  });

  it('should post owner registration details and show created location code', () => {
    const fixture = TestBed.createComponent(OwnerRegistrationComponent);
    const http = TestBed.inject(HttpTestingController);

    fillForm(fixture, '.registration-form', {
      ownerName: 'Priya Sharma',
      email: 'priya@example.com',
      password: 'StrongPass123',
      businessName: 'Priya Dental Clinic',
      locationName: 'Main Branch',
      address: '12 MG Road, Bengaluru',
      businessMobile: '9876500000'
    });
    submitForm(fixture, '.registration-form');

    const request = http.expectOne('http://localhost:5020/api/owners/register');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      ownerName: 'Priya Sharma',
      email: 'priya@example.com',
      mobile: null,
      password: 'StrongPass123',
      businessName: 'Priya Dental Clinic',
      locationName: 'Main Branch',
      address: '12 MG Road, Bengaluru',
      businessMobile: '9876500000'
    });

    request.flush({
      ownerId: 1,
      queueLocationId: 2,
      locationCode: 'AB7K2M9Q',
      businessName: 'Priya Dental Clinic',
      locationName: 'Main Branch',
      role: 'Owner'
    });
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.status-chip')?.textContent).toContain('AB7K2M9Q');
    expect(compiled.querySelector('[role="status"]')?.textContent).toContain('Priya Dental Clinic');
  });

  it('should show validation feedback when the registration API rejects the request', () => {
    const fixture = TestBed.createComponent(OwnerRegistrationComponent);
    const http = TestBed.inject(HttpTestingController);

    fillForm(fixture, '.registration-form', {
      ownerName: 'Priya Sharma',
      email: 'priya@example.com',
      password: 'StrongPass123',
      businessName: 'Priya Dental Clinic',
      address: '12 MG Road, Bengaluru',
      businessMobile: '9876500000'
    });
    submitForm(fixture, '.registration-form');

    const request = http.expectOne('http://localhost:5020/api/owners/register');
    request.flush(
      { status: 400, errors: { BusinessName: ['A business with this name already exists.'] } },
      { status: 400, statusText: 'Bad Request' });
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('[role="alert"]')?.textContent).toContain('The registration details are incomplete or invalid.');
  });

  it('should show conflict feedback when the owner contact already exists', () => {
    const fixture = TestBed.createComponent(OwnerRegistrationComponent);
    const http = TestBed.inject(HttpTestingController);

    fillForm(fixture, '.registration-form', {
      ownerName: 'Priya Sharma',
      email: 'priya@example.com',
      password: 'StrongPass123',
      businessName: 'Priya Dental Clinic',
      address: '12 MG Road, Bengaluru',
      businessMobile: '9876500000'
    });
    submitForm(fixture, '.registration-form');

    const request = http.expectOne('http://localhost:5020/api/owners/register');
    request.flush(
      { status: 409, title: 'Owner contact already exists.' },
      { status: 409, statusText: 'Conflict' });
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('[role="alert"]')?.textContent)
      .toContain('An owner with this email or mobile number is already registered.');
  });

  function fillForm(
    fixture: ComponentFixture<OwnerRegistrationComponent>,
    formSelector: string,
    values: Partial<Record<string, string>>): void {
    fixture.detectChanges();

    for (const [controlName, value] of Object.entries(values)) {
      const control = fixture.debugElement.query(By.css(`${formSelector} [formControlName="${controlName}"]`))
        .nativeElement as HTMLInputElement | HTMLTextAreaElement;

      control.value = value ?? '';
      control.dispatchEvent(new Event('input'));
    }

    fixture.detectChanges();
  }

  function submitForm(fixture: ComponentFixture<OwnerRegistrationComponent>, formSelector: string): void {
    fixture.detectChanges();
    fixture.debugElement.query(By.css(formSelector)).triggerEventHandler('ngSubmit');
    fixture.detectChanges();
  }
});
