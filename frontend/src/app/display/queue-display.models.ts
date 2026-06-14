export type QueueDisplay = {
  locationCode: string;
  businessName: string;
  isQueueOpen: boolean;
  currentCalledTokenNumber: number | null;
  currentCalledCustomerName: string | null;
  lastServedTokenNumber: number | null;
  lastServedCustomerName: string | null;
  waitingCount: number;
};
