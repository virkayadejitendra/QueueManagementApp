export type LoginRequest = {
  identifier: string;
  password: string;
};

export type LoginResponse = {
  accessToken: string;
  tokenType: 'Bearer';
  expiresIn: number;
  userId: number;
  name: string;
  roles: string[];
};
