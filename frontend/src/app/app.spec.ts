import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { By } from '@angular/platform-browser';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    }).compileComponents();
  });

  afterEach(() => {
    TestBed.inject(HttpTestingController).verify();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render registration heading', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Register your business queue');
  });

  it('should require either owner email or mobile before submitting registration', () => {
    const fixture = TestBed.createComponent(App);

    fillRegistrationForm(fixture, {
      ownerName: 'Priya Sharma',
      password: 'StrongPass123',
      businessName: 'Priya Dental Clinic',
      address: '12 MG Road, Bengaluru',
      businessMobile: '9876500000'
    });
    submitRegistrationForm(fixture);

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('[role="alert"]')?.textContent).toContain('Enter either the owner email or mobile number.');
  });

  it('should post owner registration details and show created location code', () => {
    const fixture = TestBed.createComponent(App);
    const http = TestBed.inject(HttpTestingController);

    fillRegistrationForm(fixture, {
      ownerName: 'Priya Sharma',
      email: 'priya@example.com',
      password: 'StrongPass123',
      businessName: 'Priya Dental Clinic',
      locationName: 'Main Branch',
      address: '12 MG Road, Bengaluru',
      businessMobile: '9876500000'
    });
    submitRegistrationForm(fixture);

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
    expect(compiled.querySelector('.location-code')?.textContent).toContain('AB7K2M9Q');
    expect(compiled.querySelector('[role="status"]')?.textContent).toContain('Priya Dental Clinic');
  });

  it('should show validation feedback when the registration API rejects the request', () => {
    const fixture = TestBed.createComponent(App);
    const http = TestBed.inject(HttpTestingController);

    fillRegistrationForm(fixture, {
      ownerName: 'Priya Sharma',
      email: 'priya@example.com',
      password: 'StrongPass123',
      businessName: 'Priya Dental Clinic',
      address: '12 MG Road, Bengaluru',
      businessMobile: '9876500000'
    });
    submitRegistrationForm(fixture);

    const request = http.expectOne('http://localhost:5020/api/owners/register');
    request.flush(
      { status: 400, errors: { BusinessName: ['A business with this name already exists.'] } },
      { status: 400, statusText: 'Bad Request' });
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('[role="alert"]')?.textContent).toContain('The registration details are incomplete or invalid.');
  });

  function fillRegistrationForm(
    fixture: ComponentFixture<App>,
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

  function submitRegistrationForm(fixture: ComponentFixture<App>): void {
    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit');
    fixture.detectChanges();
  }
});
