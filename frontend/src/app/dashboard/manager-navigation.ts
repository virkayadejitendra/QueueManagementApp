export type NavigationItem = {
  label: string;
  route: string;
  isReady: boolean;
};

export const managerNavigationItems: NavigationItem[] = [
  { label: 'Dashboard', route: '/dashboard', isReady: true },
  { label: 'Today queue', route: '/today-queue', isReady: true },
  { label: 'Walk-in', route: '/walk-in', isReady: true },
  { label: 'Join QR', route: '/join-qr', isReady: true },
  { label: 'Display screen', route: '/dashboard', isReady: false },
  { label: 'Settings', route: '/dashboard', isReady: false }
];
