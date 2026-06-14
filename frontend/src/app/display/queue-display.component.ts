import { HttpErrorResponse } from '@angular/common/http';
import { DOCUMENT } from '@angular/common';
import { Component, OnDestroy, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import QRCode from 'qrcode';
import { QueueDisplayApi } from './queue-display-api';
import { QueueDisplay } from './queue-display.models';

@Component({
  selector: 'app-queue-display',
  templateUrl: './queue-display.component.html',
  styleUrl: './queue-display.component.scss'
})
export class QueueDisplayComponent implements OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly document = inject(DOCUMENT);
  private readonly queueDisplayApi = inject(QueueDisplayApi);
  private readonly locationCode = this.route.snapshot.paramMap.get('locationCode') ?? '';
  private readonly pollInterval = 3000;
  private readonly clockInterval = 1000;
  private readonly pollingTimer = setInterval(() => this.loadDisplay(), this.pollInterval);
  private readonly clockTimer = setInterval(() => this.now.set(Date.now()), this.clockInterval);

  protected readonly display = signal<QueueDisplay | null>(null);
  protected readonly isInitialLoading = signal(true);
  protected readonly refreshErrorMessage = signal<string | null>(null);
  protected readonly joinUrl = signal<string | null>(null);
  protected readonly joinQrCodeDataUrl = signal<string | null>(null);
  protected readonly lastUpdatedAt = signal<number | null>(null);
  protected readonly now = signal(Date.now());
  protected readonly secondsSinceLastUpdate = computed(() => {
    const lastUpdatedAt = this.lastUpdatedAt();
    return lastUpdatedAt === null ? null : Math.max(0, Math.floor((this.now() - lastUpdatedAt) / 1000));
  });

  constructor() {
    this.loadDisplay();
  }

  ngOnDestroy(): void {
    clearInterval(this.pollingTimer);
    clearInterval(this.clockTimer);
  }

  protected refreshNow(): void {
    this.loadDisplay();
  }

  private loadDisplay(): void {
    if (!this.locationCode) {
      this.refreshErrorMessage.set('This display link is missing a location code.');
      this.isInitialLoading.set(false);
      return;
    }

    this.queueDisplayApi.getDisplay(this.locationCode).subscribe({
      next: display => {
        this.display.set(display);
        void this.updateJoinQrCode(display.locationCode);
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
      return 'This display location could not be found.';
    }

    return 'Could not refresh the display. We will retry automatically.';
  }

  private async updateJoinQrCode(locationCode: string): Promise<void> {
    const baseHref = this.document.querySelector('base')?.getAttribute('href') ?? '/';
    const appBaseUrl = new URL(baseHref, globalThis.location.origin);
    const joinUrl = new URL(`join/${encodeURIComponent(locationCode)}`, appBaseUrl).toString();

    this.joinUrl.set(joinUrl);
    this.joinQrCodeDataUrl.set(await QRCode.toDataURL(joinUrl, {
      errorCorrectionLevel: 'M',
      margin: 2,
      width: 220,
      color: {
        dark: '#18211f',
        light: '#ffffff'
      }
    }));
  }
}
