import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthTokenStorage } from './auth-token-storage';

export const authTokenInterceptor: HttpInterceptorFn = (request, next) => {
  const token = inject(AuthTokenStorage).getToken();

  if (!token) {
    return next(request);
  }

  return next(request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  }));
};
