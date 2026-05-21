export type CustomerJoinQueueRequest = {
  customerName: string;
  mobile: string | null;
  partySize: number | null;
  serviceReason: string | null;
};

export type CustomerJoinQueueResponse = {
  queueEntryId: number;
  queueLocationId: number;
  locationCode: string;
  tokenNumber: number;
  status: string;
  trackingToken: string;
  statusUrl: string;
};
