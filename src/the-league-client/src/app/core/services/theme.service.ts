import { Injectable, signal, effect, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export type ThemeMode = 'system' | 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly isBrowser = isPlatformBrowser(this.platformId);
  private readonly _theme = signal<ThemeMode>(this.loadSavedTheme());
  readonly theme = this._theme.asReadonly();

  constructor() {
    if (this.isBrowser) {
      effect(() => {
        this.applyTheme(this._theme());
      });

      // Watch OS preference changes
      window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
        if (this._theme() === 'system') this.applyTheme('system');
      });
    }
  }

  setTheme(theme: ThemeMode): void {
    this._theme.set(theme);
    if (this.isBrowser) {
      localStorage.setItem('theme-preference', theme);
    }
  }

  private applyTheme(mode: ThemeMode): void {
    if (!this.isBrowser) return;

    const isDark = mode === 'dark' ||
      (mode === 'system' && window.matchMedia('(prefers-color-scheme: dark)').matches);

    document.documentElement.setAttribute('data-theme', isDark ? 'league-dark' : 'league-light');
  }

  private loadSavedTheme(): ThemeMode {
    if (!this.isBrowser) return 'system';
    return (localStorage.getItem('theme-preference') as ThemeMode) ?? 'system';
  }
}
