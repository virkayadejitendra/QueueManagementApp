import { DOCUMENT } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import QRCode from 'qrcode';
import { ManagerQueueApi } from '../dashboard/manager-queue-api';
import { ManagerQueueToday } from '../dashboard/manager-queue.models';

@Component({
  selector: 'app-join-qr',
  imports: [RouterLink],
  templateUrl: './join-qr.component.html',
  styleUrl: './join-qr.component.scss'
})
export class JoinQrComponent {
  private readonly document = inject(DOCUMENT);
  private readonly managerQueueApi = inject(ManagerQueueApi);

  protected readonly queueToday = signal<ManagerQueueToday | null>(null);
  protected readonly joinUrl = signal<string | null>(null);
  protected readonly qrCodeDataUrl = signal<string | null>(null);
  protected readonly posterDataUrl = signal<string | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.loadQueue();
  }

  protected downloadPoster(): void {
    const posterDataUrl = this.posterDataUrl();
    const today = this.queueToday();

    if (!posterDataUrl || !today) {
      return;
    }

    const anchor = this.document.createElement('a');
    anchor.href = posterDataUrl;
    anchor.download = `join-queue-poster-${today.locationCode}.png`;
    anchor.click();
  }

  protected printPoster(): void {
    const posterDataUrl = this.posterDataUrl();
    const today = this.queueToday();

    if (!posterDataUrl || !today) {
      return;
    }

    const printWindow = globalThis.open('', '_blank', 'width=760,height=900');

    if (!printWindow) {
      this.errorMessage.set('Could not open the print window. Allow pop-ups and try again.');
      return;
    }

    printWindow.document.open();
    printWindow.document.write(`
      <!doctype html>
      <html>
        <head>
          <title>Join Queue QR - ${this.escapeHtml(today.locationCode)}</title>
          <style>
            * { box-sizing: border-box; }
            body {
              margin: 0;
              min-height: 100vh;
              display: grid;
              place-items: center;
              background: #ffffff;
            }
            img {
              width: min(720px, 94vw);
              display: block;
            }
            @media print {
              body { min-height: auto; }
              img { width: 190mm; margin: 0 auto; }
            }
          </style>
        </head>
        <body>
          <img src="${posterDataUrl}" alt="Join queue poster for ${this.escapeHtml(today.businessName)}" />
        </body>
      </html>
    `);
    printWindow.document.close();
    printWindow.focus();
    printWindow.print();
  }

  private loadQueue(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.managerQueueApi.getToday().subscribe({
      next: today => {
        this.queueToday.set(today);
        void this.createPoster(today);
        this.isLoading.set(false);
      },
      error: error => {
        this.errorMessage.set(this.getErrorMessage(error));
        this.isLoading.set(false);
      }
    });
  }

  private async createPoster(today: ManagerQueueToday): Promise<void> {
    const joinUrl = this.createJoinUrl(today.locationCode);
    const qrCodeDataUrl = await QRCode.toDataURL(joinUrl, {
      errorCorrectionLevel: 'M',
      margin: 2,
      width: 520,
      color: {
        dark: '#18211f',
        light: '#ffffff'
      }
    });

    this.joinUrl.set(joinUrl);
    this.qrCodeDataUrl.set(qrCodeDataUrl);
    this.posterDataUrl.set(await this.createPosterDataUrl(today, joinUrl, qrCodeDataUrl));
  }

  private createJoinUrl(locationCode: string): string {
    const baseHref = this.document.querySelector('base')?.getAttribute('href') ?? '/';
    const appBaseUrl = new URL(baseHref, globalThis.location.origin);
    return new URL(`join/${encodeURIComponent(locationCode)}`, appBaseUrl).toString();
  }

  private async createPosterDataUrl(
    today: ManagerQueueToday,
    joinUrl: string,
    qrCodeDataUrl: string): Promise<string> {
    const canvas = this.document.createElement('canvas');
    canvas.width = 1200;
    canvas.height = 1600;

    const context = canvas.getContext('2d');

    if (!context) {
      return qrCodeDataUrl;
    }

    context.fillStyle = '#ffffff';
    context.fillRect(0, 0, canvas.width, canvas.height);
    context.fillStyle = '#18211f';
    this.drawCenteredText(context, today.businessName, 600, 150, 72, 1000, '800');
    this.drawCenteredText(context, 'Scan to join the queue', 600, 245, 42, 960, '700', '#34413d');

    const qrImage = await this.loadImage(qrCodeDataUrl);
    context.drawImage(qrImage, 330, 330, 540, 540);

    context.fillStyle = '#eef7f4';
    this.roundRect(context, 370, 935, 460, 92, 18);
    context.fill();
    this.drawCenteredText(context, `Location ${today.locationCode}`, 600, 990, 38, 420, '800', '#124c59');

    this.drawCenteredText(context, joinUrl, 600, 1125, 32, 980, '700', '#34413d');
    this.drawCenteredText(context, 'No app install required', 600, 1230, 34, 900, '700', '#58635f');

    return canvas.toDataURL('image/png');
  }

  private loadImage(source: string): Promise<HTMLImageElement> {
    return new Promise((resolve, reject) => {
      const image = new Image();
      image.onload = () => resolve(image);
      image.onerror = reject;
      image.src = source;
    });
  }

  private drawCenteredText(
    context: CanvasRenderingContext2D,
    value: string,
    x: number,
    y: number,
    fontSize: number,
    maxWidth: number,
    fontWeight: string,
    color = '#18211f'): void {
    context.fillStyle = color;
    context.font = `${fontWeight} ${fontSize}px Arial, sans-serif`;
    context.textAlign = 'center';
    context.textBaseline = 'middle';

    let text = value;

    while (context.measureText(text).width > maxWidth && text.length > 4) {
      text = `${text.slice(0, -4)}...`;
    }

    context.fillText(text, x, y);
  }

  private roundRect(
    context: CanvasRenderingContext2D,
    x: number,
    y: number,
    width: number,
    height: number,
    radius: number): void {
    context.beginPath();
    context.moveTo(x + radius, y);
    context.lineTo(x + width - radius, y);
    context.quadraticCurveTo(x + width, y, x + width, y + radius);
    context.lineTo(x + width, y + height - radius);
    context.quadraticCurveTo(x + width, y + height, x + width - radius, y + height);
    context.lineTo(x + radius, y + height);
    context.quadraticCurveTo(x, y + height, x, y + height - radius);
    context.lineTo(x, y + radius);
    context.quadraticCurveTo(x, y, x + radius, y);
    context.closePath();
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === 401) {
      return 'Your login has expired. Please sign in again.';
    }

    if (error instanceof HttpErrorResponse && error.status === 404) {
      return 'No queue location is assigned to this account.';
    }

    return 'Could not load the join QR poster. Check that the API is running and try again.';
  }

  private escapeHtml(value: string): string {
    return value
      .replaceAll('&', '&amp;')
      .replaceAll('<', '&lt;')
      .replaceAll('>', '&gt;')
      .replaceAll('"', '&quot;')
      .replaceAll("'", '&#39;');
  }
}
