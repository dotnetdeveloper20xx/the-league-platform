export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: AuthUser;
}

export interface AuthUser {
  id: string;
  email: string;
  name: string;
  role: UserRole;
  clubId?: string;
  memberId?: string;
}

export type UserRole = 'SuperAdmin' | 'ClubManager' | 'Member' | 'Coach' | 'Staff';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}
