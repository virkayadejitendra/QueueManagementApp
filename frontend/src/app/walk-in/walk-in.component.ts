import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthTokenStorage } from '../auth/auth-token-storage';
import { managerNavigationItems } from '../dashboard/manager-navigation';
import { ManagerQueueApi } from '../dashboard/manager-queue-api';
import { ManagerQueueToday, ManagerWalkInRequest } from '../dashboard/manager-queue.models';

@Component({
  selector: 'app-walk-in',
  imports: [FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './walk-in.component.html',
  styleUrl: '../dashboard/dashboard.component.scss'
})
export class WalkInComponent {
  private readonly authTokenStorage = inject(AuthTokenStorage);
  private readonly managerQueueApi = inject(ManagerQueueApi);
  private readonly router = inject(Router);

  protected readonly queueToday = signal<ManagerQueueToday | null>(null);
  protected readonly isLoadingQueue = signal(false);
  protected readonly isSavingWalkIn = signal(false);
  protected readonly queueActionErrorMessage = signal<string | null>(null);
  protected readonly navigationItems = managerNavigationItems;
  protected readonly walkInForm: ManagerWalkInRequest = {
    customerName: '',
    mobile: '',
    partySize: null,
    serviceReason: ''
  };

  constructor() {
    this.loadTodayQueue();
  }

  protected addWalkIn(): void {
    if (!this.walkInForm.customerName.trim()) {
      this.queueActionErrorMessage.set('Customer name is required for a walk-in.');
      return;
    }

    this.isSavingWalkIn.set(true);
    this.runTodayAction(
      () => this.managerQueueApi.addWalkIn({
        customerName: this.walkInForm.customerName,
        mobile: this.walkInForm.mobile || null,
        partySize: this.walkInForm.partySize || null,
        serviceReason: this.walkInForm.serviceReason || null
      }),
      () => {
        this.walkInForm.customerName = '';
        this.walkInForm.mobile = '';
        this.walkInForm.partySize = null;
        this.walkInForm.serviceReason = '';
        this.isSavingWalkIn.set(false);
      },
      () => this.isSavingWalkIn.set(false));
  }

  protected signOut(): void {
    this.authTokenStorage.clearToken();
    void this.router.navigateByUrl('/login');
  }

  private loadTodayQueue(): void {
    this.runTodayAction(() => this.managerQueueApi.getToday());
  }

  private runTodayAction(
    action: () => Observable<ManagerQueueToday>,
    onSuccess?: () => void,
    onError?: () => void): void {
    this.isLoadingQueue.set(true);
    this.queueActionErrorMessage.set(null);

    action().subscribe({
      next: today => {
        this.queueToday.set(today);
        onSuccess?.();
        this.isLoadingQueue.set(false);
      },
      error: error => {
        this.queueActionErrorMessage.set(this.getQueueActionErrorMessage(error));
        onError?.();
        this.isLoadingQueue.set(false);
      }
    });
  }

  private getQueueActionErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === 401) {
      return 'Your login has expired. Please sign in again.';
    }

    if (error instanceof HttpErrorResponse && error.status === 404) {
      return 'No queue location is assigned to this account.';
    }

    if (error instanceof HttpErrorResponse && error.status === 409) {
      return error.error?.detail ?? 'This queue is currently closed.';
    }

    if (error instanceof HttpErrorResponse && error.status === 400) {
      return 'Check the customer details and try again.';
    }

    return 'Walk-in customer could not be saved. Check that the API is running and try again.';
  }
}
