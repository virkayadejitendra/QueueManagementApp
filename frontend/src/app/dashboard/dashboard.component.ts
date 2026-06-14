import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthTokenStorage } from '../auth/auth-token-storage';
import { ManagerQueueApi } from './manager-queue-api';
import { ManagerQueueEntry, ManagerQueueStatus, ManagerQueueToday, ManagerWalkInRequest } from './manager-queue.models';

type NavigationItem = {
  label: string;
  route: string;
  isReady: boolean;
};

type QueueListTab = 'waiting' | 'skipped' | 'served';

@Component({
  selector: 'app-dashboard',
  imports: [FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent {
  private readonly authTokenStorage = inject(AuthTokenStorage);
  private readonly managerQueueApi = inject(ManagerQueueApi);
  private readonly router = inject(Router);

  protected readonly queueToday = signal<ManagerQueueToday | null>(null);
  protected readonly isLoadingQueue = signal(false);
  protected readonly isSavingWalkIn = signal(false);
  protected readonly queueActionErrorMessage = signal<string | null>(null);
  protected readonly activeQueueListTab = signal<QueueListTab>('waiting');
  protected readonly walkInForm: ManagerWalkInRequest = {
    customerName: '',
    mobile: '',
    partySize: null,
    serviceReason: ''
  };

  protected readonly navigationItems: NavigationItem[] = [
    { label: 'Dashboard', route: '/dashboard', isReady: true },
    { label: 'Today queue', route: '/dashboard', isReady: true },
    { label: 'Walk-in', route: '/dashboard', isReady: true },
    { label: 'Join QR', route: '/join-qr', isReady: true },
    { label: 'Display screen', route: '/dashboard', isReady: false },
    { label: 'Settings', route: '/dashboard', isReady: false }
  ];

  constructor() {
    this.loadTodayQueue();
  }

  protected openQueue(): void {
    this.runStatusAction(() => this.managerQueueApi.open());
  }

  protected closeQueue(): void {
    this.runStatusAction(() => this.managerQueueApi.close());
  }

  protected callNext(): void {
    this.runTodayAction(() => this.managerQueueApi.callNext());
  }

  protected markServed(entry: ManagerQueueEntry): void {
    this.runTodayAction(() => this.managerQueueApi.markServed(entry.queueEntryId));
  }

  protected markNoResponse(entry: ManagerQueueEntry): void {
    this.runTodayAction(() => this.managerQueueApi.markNoResponse(entry.queueEntryId));
  }

  protected markSkipped(entry: ManagerQueueEntry): void {
    this.runTodayAction(() => this.managerQueueApi.markSkipped(entry.queueEntryId));
  }

  protected restore(entry: ManagerQueueEntry): void {
    this.runTodayAction(() => this.managerQueueApi.restore(entry.queueEntryId));
  }

  protected cancel(entry: ManagerQueueEntry): void {
    this.runTodayAction(() => this.managerQueueApi.cancel(entry.queueEntryId));
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

  protected get joinUrl(): string | null {
    const today = this.queueToday();
    return today ? `/join/${today.locationCode}` : null;
  }

  protected get displayUrl(): string | null {
    const today = this.queueToday();
    return today ? `/display/${today.locationCode}` : null;
  }

  protected signOut(): void {
    this.authTokenStorage.clearToken();
    void this.router.navigateByUrl('/login');
  }

  protected showQueueList(tab: QueueListTab): void {
    this.activeQueueListTab.set(tab);
  }

  private loadTodayQueue(): void {
    this.runTodayAction(() => this.managerQueueApi.getToday());
  }

  private runStatusAction(action: () => Observable<ManagerQueueStatus>): void {
    this.isLoadingQueue.set(true);
    this.queueActionErrorMessage.set(null);

    action().subscribe({
      next: () => {
        this.isLoadingQueue.set(false);
        this.loadTodayQueue();
      },
      error: error => {
        this.queueActionErrorMessage.set(this.getQueueActionErrorMessage(error));
        this.isLoadingQueue.set(false);
      }
    });
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
      return 'This queue item could not be found. Refresh and try again.';
    }

    if (error instanceof HttpErrorResponse && error.status === 409) {
      return error.error?.detail ?? 'Another manager already changed this queue state. Refresh and try again.';
    }

    if (error instanceof HttpErrorResponse && error.status === 400) {
      return 'Check the customer details and try again.';
    }

    return 'Queue controls could not be loaded. Check that the API is running and try again.';
  }
}
