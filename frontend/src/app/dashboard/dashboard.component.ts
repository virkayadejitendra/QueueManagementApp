import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthTokenStorage } from '../auth/auth-token-storage';

type NavigationItem = {
  label: string;
  route: string;
  isReady: boolean;
};

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent {
  private readonly authTokenStorage = inject(AuthTokenStorage);
  private readonly router = inject(Router);

  protected readonly navigationItems: NavigationItem[] = [
    { label: 'Dashboard', route: '/dashboard', isReady: true },
    { label: 'Today queue', route: '/dashboard', isReady: false },
    { label: 'Walk-in', route: '/dashboard', isReady: false },
    { label: 'Display screen', route: '/dashboard', isReady: false },
    { label: 'Settings', route: '/dashboard', isReady: false }
  ];

  protected signOut(): void {
    this.authTokenStorage.clearToken();
    void this.router.navigateByUrl('/login');
  }
}
