import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiUrl } from '../core/api-url';
import { ManagerQueueStatus } from './manager-queue.models';

@Injectable({
  providedIn: 'root'
})
export class ManagerQueueApi {
  private readonly http = inject(HttpClient);

  getStatus(): Observable<ManagerQueueStatus> {
    return this.http.get<ManagerQueueStatus>(apiUrl('/api/manager/queue/status'));
  }

  open(): Observable<ManagerQueueStatus> {
    return this.http.post<ManagerQueueStatus>(apiUrl('/api/manager/queue/open'), null);
  }

  close(): Observable<ManagerQueueStatus> {
    return this.http.post<ManagerQueueStatus>(apiUrl('/api/manager/queue/close'), null);
  }

  reset(): Observable<ManagerQueueStatus> {
    return this.http.post<ManagerQueueStatus>(apiUrl('/api/manager/queue/reset'), null);
  }
}
