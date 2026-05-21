import { Injectable } from '@angular/core';

export const authTokenStorageKey = 'queueManagement.authToken';

@Injectable({ providedIn: 'root' })
export class AuthTokenStorage {
  getToken(): string | null {
    return globalThis.localStorage?.getItem(authTokenStorageKey) ?? null;
  }

  setToken(token: string): void {
    globalThis.localStorage?.setItem(authTokenStorageKey, token);
  }

  clearToken(): void {
    globalThis.localStorage?.removeItem(authTokenStorageKey);
  }
}
