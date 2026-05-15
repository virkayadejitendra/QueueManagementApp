import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { apiUrl } from './core/api-url';

type OwnerRegistrationResponse = {
  ownerId: number;
  queueLocationId: number;
  locationCode: string;
  businessName: string;
  locationName: string | null;
  role: string;
};

@Component({
  selector: 'app-root',
  imports: [ReactiveFormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly formBuilder = inject(FormBuilder);
  private readonly http = inject(HttpClient);

  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly registration = signal<OwnerRegistrationResponse | null>(null);

  protected readonly registrationForm = this.formBuilder.nonNullable.group({
    ownerName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.email, Validators.maxLength(254)]],
    mobile: ['', [Validators.maxLength(30)]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(100)]],
    businessName: ['', [Validators.required, Validators.maxLength(150)]],
    locationName: ['', [Validators.maxLength(150)]],
    address: ['', [Validators.required, Validators.maxLength(300)]],
    businessMobile: ['', [Validators.required, Validators.maxLength(30)]]
  });

  protected submitRegistration(): void {
    this.errorMessage.set(null);
    this.registration.set(null);

    if (this.registrationForm.invalid) {
      this.registrationForm.markAllAsTouched();
      this.errorMessage.set('Please complete the required fields before creating the location.');
      return;
    }

    const formValue = this.registrationForm.getRawValue();

    if (!formValue.email.trim() && !formValue.mobile.trim()) {
      this.errorMessage.set('Enter either the owner email or mobile number.');
      return;
    }

    this.isSubmitting.set(true);

    this.http.post<OwnerRegistrationResponse>(apiUrl('/api/owners/register'), {
      ownerName: formValue.ownerName,
      email: formValue.email || null,
      mobile: formValue.mobile || null,
      password: formValue.password,
      businessName: formValue.businessName,
      locationName: formValue.locationName || null,
      address: formValue.address,
      businessMobile: formValue.businessMobile
    }).subscribe({
      next: response => {
        this.registration.set(response);
        this.registrationForm.reset();
        this.isSubmitting.set(false);
      },
      error: error => {
        this.errorMessage.set(this.getErrorMessage(error));
        this.isSubmitting.set(false);
      }
    });
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === 400) {
      return 'The registration details are incomplete or invalid.';
    }

    return 'Registration could not be completed. Check that the API is running and try again.';
  }
}
