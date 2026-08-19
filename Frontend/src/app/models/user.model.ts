export interface User {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  role?: string;
}
export interface LoginRequest {
  email: string;
  password: string;
}
export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role?: string;
}
export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
}
