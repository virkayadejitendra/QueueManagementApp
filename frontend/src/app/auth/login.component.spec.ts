import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router, provideRouter } from '@angular/router';
import { By } from '@angular/platform-browser';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  beforeEach(async () => {
    globalThis.localStorage?.clear();

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
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

  it('should render login form', async () => {
    const fixture = TestBed.createComponent(LoginComponent);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.querySelector('#login-title')?.textContent).toContain('Owner or manager login');
    expect(compiled.querySelector('.login-form [formControlName="identifier"]')).not.toBeNull();
    expect(compiled.querySelector('.login-form [formControlName="password"]')).not.toBeNull();
  });

  it('should require login identifier and password before submitting', () => {
    const fixture = TestBed.createComponent(LoginComponent);

    submitForm(fixture, '.login-form');

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.login-form [role="alert"]')?.textContent)
      .toContain('Enter your email or mobile number and password.');
  });

  it('should store login token and navigate to dashboard after successful login', () => {
    const fixture = TestBed.createComponent(LoginComponent);
    const http = TestBed.inject(HttpTestingController);
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

    fillForm(fixture, '.login-form', {
      identifier: 'priya@example.com',
      password: 'StrongPass123'
    });
    submitForm(fixture, '.login-form');

    const request = http.expectOne('http://localhost:5020/api/auth/login');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      identifier: 'priya@example.com',
      password: 'StrongPass123'
    });

    request.flush({
      accessToken: 'header.payload.signature',
      tokenType: 'Bearer',
      expiresIn: 3600,
      userId: 1,
      name: 'Priya Sharma',
      roles: ['Owner']
    });
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(globalThis.localStorage?.getItem('queueManagement.authToken')).toBe('header.payload.signature');
    expect(compiled.querySelector('.login-form [role="status"]')?.textContent)
      .toContain('Priya Sharma');
    expect(navigateSpy).toHaveBeenCalledWith('/dashboard');
  });

  it('should show invalid credential feedback when login is rejected', () => {
    const fixture = TestBed.createComponent(LoginComponent);
    const http = TestBed.inject(HttpTestingController);

    fillForm(fixture, '.login-form', {
      identifier: 'priya@example.com',
      password: 'WrongPass123'
    });
    submitForm(fixture, '.login-form');

    const request = http.expectOne('http://localhost:5020/api/auth/login');
    request.flush(null, { status: 401, statusText: 'Unauthorized' });
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.login-form [role="alert"]')?.textContent)
      .toContain('Email, mobile number, or password is incorrect.');
  });

  function fillForm(
    fixture: ComponentFixture<LoginComponent>,
    formSelector: string,
    values: Partial<Record<string, string>>): void {
    fixture.detectChanges();

    for (const [controlName, value] of Object.entries(values)) {
      const control = fixture.debugElement.query(By.css(`${formSelector} [formControlName="${controlName}"]`))
        .nativeElement as HTMLInputElement;

      control.value = value ?? '';
      control.dispatchEvent(new Event('input'));
    }

    fixture.detectChanges();
  }

  function submitForm(fixture: ComponentFixture<LoginComponent>, formSelector: string): void {
    fixture.detectChanges();
    fixture.debugElement.query(By.css(formSelector)).triggerEventHandler('ngSubmit');
    fixture.detectChanges();
  }
});
