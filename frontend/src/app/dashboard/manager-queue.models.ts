export type ManagerQueueStatus = {
  queueLocationId: number;
  locationCode: string;
  businessName: string;
  isQueueOpen: boolean;
  waitingCount: number;
  currentTokenNumber: number | null;
};

export type ManagerQueueEntry = {
  queueEntryId: number;
  tokenNumber: number;
  customerName: string;
  mobile: string | null;
  partySize: number | null;
  serviceReason: string | null;
  status: string;
  callCount: number;
  skipCount: number;
  createdAt?: string;
  calledAt?: string | null;
  servedAt?: string | null;
  cancelledAt?: string | null;
  lastSkippedAt?: string | null;
};

export type ManagerQueueToday = {
  queueLocationId: number;
  locationCode: string;
  businessName: string;
  isQueueOpen: boolean;
  waitingCount: number;
  currentCalled: ManagerQueueEntry | null;
  waitingEntries: ManagerQueueEntry[];
  skippedEntries: ManagerQueueEntry[];
  recentServedEntries: ManagerQueueEntry[];
};

export type ManagerWalkInRequest = {
  customerName: string;
  mobile?: string | null;
  partySize?: number | null;
  serviceReason?: string | null;
};
