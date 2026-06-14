import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthTokenStorage } from '../auth/auth-token-storage';
import { managerNavigationItems } from '../dashboard/manager-navigation';
import { ManagerQueueApi } from '../dashboard/manager-queue-api';
import { ManagerQueueEntry, ManagerQueueToday } from '../dashboard/manager-queue.models';

type QueueListTab = 'waiting' | 'skipped' | 'served';

@Component({
  selector: 'app-today-queue',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './today-queue.component.html',
  styleUrl: '../dashboard/dashboard.component.scss'
})
export class TodayQueueComponent {
  private readonly authTokenStorage = inject(AuthTokenStorage);
  private readonly managerQueueApi = inject(ManagerQueueApi);
  private readonly router = inject(Router);

  protected readonly queueToday = signal<ManagerQueueToday | null>(null);
  protected readonly isLoadingQueue = signal(false);
  protected readonly queueActionErrorMessage = signal<string | null>(null);
  protected readonly activeQueueListTab = signal<QueueListTab>('waiting');
  protected readonly navigationItems = managerNavigationItems;

  constructor() {
    this.loadTodayQueue();
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

  protected showQueueList(tab: QueueListTab): void {
    this.activeQueueListTab.set(tab);
  }

  protected signOut(): void {
    this.authTokenStorage.clearToken();
    void this.router.navigateByUrl('/login');
  }

  private loadTodayQueue(): void {
    this.runTodayAction(() => this.managerQueueApi.getToday());
  }

  private runTodayAction(action: () => Observable<ManagerQueueToday>): void {
    this.isLoadingQueue.set(true);
    this.queueActionErrorMessage.set(null);

    action().subscribe({
      next: today => {
        this.queueToday.set(today);
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
      return 'This queue item could not be found. Refresh and try again.';
    }

    if (error instanceof HttpErrorResponse && error.status === 409) {
      return error.error?.detail ?? 'Another manager already changed this queue state. Refresh and try again.';
    }

    return 'Queue list could not be loaded. Check that the API is running and try again.';
  }
}
