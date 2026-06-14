import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiUrl } from '../core/api-url';
import { CustomerQueueStatus } from './customer-status.models';

@Injectable({
  providedIn: 'root'
})
export class CustomerStatusApi {
  private readonly http = inject(HttpClient);

  getStatus(queueEntryId: string, trackingToken: string): Observable<CustomerQueueStatus> {
    return this.http.get<CustomerQueueStatus>(
      apiUrl(`/api/queue-entries/${encodeURIComponent(queueEntryId)}/status`),
      {
        params: {
          trackingToken
        }
      });
  }
}
