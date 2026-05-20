export type OwnerRegistrationRequest = {
  ownerName: string;
  email: string | null;
  mobile: string | null;
  password: string;
  businessName: string;
  locationName: string | null;
  address: string;
  businessMobile: string;
};

export type OwnerRegistrationResponse = {
  ownerId: number;
  queueLocationId: number;
  locationCode: string;
  businessName: string;
  locationName: string | null;
  role: string;
};
