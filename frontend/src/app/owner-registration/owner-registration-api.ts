import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiUrl } from '../core/api-url';
import { OwnerRegistrationRequest, OwnerRegistrationResponse } from './owner-registration.models';

@Injectable({
  providedIn: 'root'
})
export class OwnerRegistrationApi {
  private readonly http = inject(HttpClient);

  register(request: OwnerRegistrationRequest): Observable<OwnerRegistrationResponse> {
    return this.http.post<OwnerRegistrationResponse>(apiUrl('/api/owners/register'), request);
  }
}
