export type CustomerQueueStatus = {
  queueEntryId: number;
  locationCode: string;
  businessName: string;
  tokenNumber: number;
  status: string;
  queuePosition: number | null;
  waitingCount: number;
  currentCalledTokenNumber: number | null;
  createdAt: string;
  calledAt: string | null;
  servedAt: string | null;
  cancelledAt: string | null;
};
