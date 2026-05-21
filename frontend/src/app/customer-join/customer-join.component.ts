import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CustomerJoinApi } from './customer-join-api';

@Component({
  selector: 'app-customer-join',
  imports: [ReactiveFormsModule],
  templateUrl: './customer-join.component.html',
  styleUrl: './customer-join.component.scss'
})
export class CustomerJoinComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly customerJoinApi = inject(CustomerJoinApi);

  protected readonly locationCode = this.route.snapshot.paramMap.get('locationCode') ?? '';
  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly joinForm = this.formBuilder.nonNullable.group({
    customerName: ['', [Validators.required, Validators.maxLength(100)]],
    mobile: ['', [Validators.maxLength(30)]],
    partySize: [null as number | null, [Validators.min(1), Validators.max(50)]],
    serviceReason: ['', [Validators.maxLength(200)]]
  });

  protected joinQueue(): void {
    this.errorMessage.set(null);

    if (!this.locationCode) {
      this.errorMessage.set('This join link is missing a location code.');
      return;
    }

    if (this.joinForm.invalid) {
      this.joinForm.markAllAsTouched();
      this.errorMessage.set('Please enter your name and check the party size.');
      return;
    }

    const formValue = this.joinForm.getRawValue();
    this.isSubmitting.set(true);

    this.customerJoinApi.join(this.locationCode, {
      customerName: formValue.customerName.trim(),
      mobile: this.normalizeOptional(formValue.mobile),
      partySize: this.normalizePartySize(formValue.partySize),
      serviceReason: this.normalizeOptional(formValue.serviceReason)
    }).subscribe({
      next: response => {
        this.isSubmitting.set(false);
        void this.router.navigateByUrl(response.statusUrl);
      },
      error: error => {
        this.errorMessage.set(this.getErrorMessage(error));
        this.isSubmitting.set(false);
      }
    });
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === 400) {
      return 'Please enter valid queue details.';
    }

    if (error instanceof HttpErrorResponse && error.status === 404) {
      return 'This queue location could not be found.';
    }

    if (error instanceof HttpErrorResponse && error.status === 409) {
      return 'This queue is currently closed.';
    }

    return 'Could not join the queue. Check that the API is running and try again.';
  }

  private normalizeOptional(value: string): string | null {
    const trimmedValue = value.trim();
    return trimmedValue ? trimmedValue : null;
  }

  private normalizePartySize(value: number | null): number | null {
    return value === null ? null : Number(value);
  }
}
