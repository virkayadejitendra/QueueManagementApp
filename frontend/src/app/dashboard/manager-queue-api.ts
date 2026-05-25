import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiUrl } from '../core/api-url';
import { ManagerQueueStatus, ManagerQueueToday, ManagerWalkInRequest } from './manager-queue.models';

@Injectable({
  providedIn: 'root'
})
export class ManagerQueueApi {
  private readonly http = inject(HttpClient);

  getStatus(): Observable<ManagerQueueStatus> {
    return this.http.get<ManagerQueueStatus>(apiUrl('/api/manager/queue/status'));
  }

  getToday(): Observable<ManagerQueueToday> {
    return this.http.get<ManagerQueueToday>(apiUrl('/api/manager/queue/today'));
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

  addWalkIn(request: ManagerWalkInRequest): Observable<ManagerQueueToday> {
    return this.http.post<ManagerQueueToday>(apiUrl('/api/manager/queue/walk-in'), request);
  }

  callNext(): Observable<ManagerQueueToday> {
    return this.http.post<ManagerQueueToday>(apiUrl('/api/manager/queue/call-next'), null);
  }

  markServed(queueEntryId: number): Observable<ManagerQueueToday> {
    return this.postEntryAction(queueEntryId, 'served');
  }

  markNoResponse(queueEntryId: number): Observable<ManagerQueueToday> {
    return this.postEntryAction(queueEntryId, 'no-response');
  }

  markSkipped(queueEntryId: number): Observable<ManagerQueueToday> {
    return this.postEntryAction(queueEntryId, 'skipped');
  }

  restore(queueEntryId: number): Observable<ManagerQueueToday> {
    return this.postEntryAction(queueEntryId, 'restore');
  }

  cancel(queueEntryId: number): Observable<ManagerQueueToday> {
    return this.postEntryAction(queueEntryId, 'cancel');
  }

  private postEntryAction(queueEntryId: number, action: string): Observable<ManagerQueueToday> {
    return this.http.post<ManagerQueueToday>(
      apiUrl(`/api/manager/queue/entries/${queueEntryId}/${action}`),
      null);
  }
}
