import { environment } from '../../environments/environment';

export function apiUrl(path: string): string {
  const normalizedBaseUrl = environment.apiBaseUrl.replace(/\/$/, '');
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;

  return `${normalizedBaseUrl}${normalizedPath}`;
}
