import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthTokenStorage } from '../auth/auth-token-storage';
import { ManagerQueueApi } from './manager-queue-api';
import { ManagerQueueStatus } from './manager-queue.models';

type NavigationItem = {
  label: string;
  route: string;
  isReady: boolean;
};

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent {
  private readonly authTokenStorage = inject(AuthTokenStorage);
  private readonly managerQueueApi = inject(ManagerQueueApi);
  private readonly router = inject(Router);

  protected readonly queueStatus = signal<ManagerQueueStatus | null>(null);
  protected readonly isLoadingQueue = signal(false);
  protected readonly queueActionErrorMessage = signal<string | null>(null);

  protected readonly navigationItems: NavigationItem[] = [
    { label: 'Dashboard', route: '/dashboard', isReady: true },
    { label: 'Today queue', route: '/dashboard', isReady: false },
    { label: 'Walk-in', route: '/dashboard', isReady: false },
    { label: 'Display screen', route: '/dashboard', isReady: false },
    { label: 'Settings', route: '/dashboard', isReady: false }
  ];

  constructor() {
    this.loadQueueStatus();
  }

  protected openQueue(): void {
    this.runQueueAction(() => this.managerQueueApi.open());
  }

  protected closeQueue(): void {
    this.runQueueAction(() => this.managerQueueApi.close());
  }

  protected resetQueue(): void {
    this.runQueueAction(() => this.managerQueueApi.reset());
  }

  protected get joinUrl(): string | null {
    const status = this.queueStatus();
    return status ? `/join/${status.locationCode}` : null;
  }

  protected signOut(): void {
    this.authTokenStorage.clearToken();
    void this.router.navigateByUrl('/login');
  }

  private loadQueueStatus(): void {
    this.runQueueAction(() => this.managerQueueApi.getStatus());
  }

  private runQueueAction(action: () => ReturnType<ManagerQueueApi['getStatus']>): void {
    this.isLoadingQueue.set(true);
    this.queueActionErrorMessage.set(null);

    action().subscribe({
      next: status => {
        this.queueStatus.set(status);
        this.isLoadingQueue.set(false);
      },
      error: error => {
        this.queueActionErrorMessage.set(this.getQueueActionErrorMessage(error));
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

    return 'Queue controls could not be loaded. Check that the API is running and try again.';
  }
}
