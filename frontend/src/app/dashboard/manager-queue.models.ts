export type ManagerQueueStatus = {
  queueLocationId: number;
  locationCode: string;
  businessName: string;
  isQueueOpen: boolean;
  waitingCount: number;
  currentTokenNumber: number | null;
};
