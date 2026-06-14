import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CustomerStatusApi } from './customer-status-api';
import { CustomerQueueStatus } from './customer-status.models';

@Component({
  selector: 'app-customer-status',
  templateUrl: './customer-status.component.html',
  styleUrl: './customer-status.component.scss'
})
export class CustomerStatusComponent implements OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly customerStatusApi = inject(CustomerStatusApi);
  private readonly pollInterval = 5000;
  private readonly clockInterval = 1000;
  private readonly queueEntryId = this.route.snapshot.paramMap.get('queueEntryId') ?? '';
  private readonly trackingToken = this.route.snapshot.paramMap.get('trackingToken') ?? '';
  private readonly pollingTimer = setInterval(() => this.loadStatus(), this.pollInterval);
  private readonly clockTimer = setInterval(() => this.now.set(Date.now()), this.clockInterval);

  protected readonly status = signal<CustomerQueueStatus | null>(null);
  protected readonly isInitialLoading = signal(true);
  protected readonly refreshErrorMessage = signal<string | null>(null);
  protected readonly lastUpdatedAt = signal<number | null>(null);
  protected readonly now = signal(Date.now());
  protected readonly secondsSinceLastUpdate = computed(() => {
    const lastUpdatedAt = this.lastUpdatedAt();
    return lastUpdatedAt === null ? null : Math.max(0, Math.floor((this.now() - lastUpdatedAt) / 1000));
  });

  constructor() {
    this.loadStatus();
  }

  ngOnDestroy(): void {
    clearInterval(this.pollingTimer);
    clearInterval(this.clockTimer);
  }

  protected refreshNow(): void {
    this.loadStatus();
  }

  private loadStatus(): void {
    if (!this.queueEntryId || !this.trackingToken) {
      this.refreshErrorMessage.set('This private status link is incomplete.');
      this.isInitialLoading.set(false);
      return;
    }

    this.customerStatusApi.getStatus(this.queueEntryId, this.trackingToken).subscribe({
      next: status => {
        this.status.set(status);
        this.refreshErrorMessage.set(null);
        this.lastUpdatedAt.set(Date.now());
        this.now.set(Date.now());
        this.isInitialLoading.set(false);
      },
      error: error => {
        this.refreshErrorMessage.set(this.getRefreshErrorMessage(error));
        this.isInitialLoading.set(false);
      }
    });
  }

  private getRefreshErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === 404) {
      return 'This private status link could not be found.';
    }

    return 'Could not refresh your queue status. We will retry automatically.';
  }
}
