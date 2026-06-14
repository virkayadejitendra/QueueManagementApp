import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiUrl } from '../core/api-url';
import { QueueDisplay } from './queue-display.models';

@Injectable({
  providedIn: 'root'
})
export class QueueDisplayApi {
  private readonly http = inject(HttpClient);

  getDisplay(locationCode: string): Observable<QueueDisplay> {
    return this.http.get<QueueDisplay>(
      apiUrl(`/api/locations/${encodeURIComponent(locationCode)}/display`));
  }
}
