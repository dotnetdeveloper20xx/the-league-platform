import { Injectable, signal, computed, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthResponse, AuthUser, UserRole } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'auth_user';
  private readonly platformId = inject(PLATFORM_ID);
  private readonly http = inject(HttpClient);

  private readonly isBrowser = isPlatformBrowser(this.platformId);

  private currentUser = signal<AuthUser | null>(this.loadUser());

  readonly user = this.currentUser.asReadonly();
  readonly userRole = computed(() => this.currentUser()?.role ?? '' as UserRole);

  getAccessToken(): string | null {
    if (!this.isBrowser) return null;
    return localStorage.getItem(this.TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }

  setAuth(response: AuthResponse): void {
    if (!this.isBrowser) return;
    localStorage.setItem(this.TOKEN_KEY, response.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(response.user));
    this.currentUser.set(response.user);
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.isBrowser ? localStorage.getItem(this.REFRESH_TOKEN_KEY) : null;
    return this.http.post<AuthResponse>('/api/auth/refresh', { refreshToken });
  }

  logout(): void {
    if (!this.isBrowser) return;
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUser.set(null);
  }

  private loadUser(): AuthUser | null {
    if (!this.isBrowser) return null;
    const stored = localStorage.getItem(this.USER_KEY);
    return stored ? JSON.parse(stored) : null;
  }
}
