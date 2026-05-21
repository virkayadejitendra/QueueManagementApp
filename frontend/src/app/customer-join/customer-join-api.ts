import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiUrl } from '../core/api-url';
import { CustomerJoinQueueRequest, CustomerJoinQueueResponse } from './customer-join.models';

@Injectable({
  providedIn: 'root'
})
export class CustomerJoinApi {
  private readonly http = inject(HttpClient);

  join(locationCode: string, request: CustomerJoinQueueRequest): Observable<CustomerJoinQueueResponse> {
    return this.http.post<CustomerJoinQueueResponse>(
      apiUrl(`/api/locations/${encodeURIComponent(locationCode)}/queue-entries`),
      request);
  }
}
