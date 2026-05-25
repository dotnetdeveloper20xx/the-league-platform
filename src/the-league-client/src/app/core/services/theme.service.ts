import { Injectable, signal, effect } from '@angular/core';

export type ThemeMode = 'system' | 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly _theme = signal<ThemeMode>('system');
  readonly theme = this._theme.asReadonly();

  constructor() {
    const saved = localStorage.getItem('theme-preference') as ThemeMode | null;
    if (saved) this._theme.set(saved);

    effect(() => {
      this.applyTheme(this._theme());
    });

    // Watch OS preference changes
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
      if (this._theme() === 'system') this.applyTheme('system');
    });
  }

  setTheme(theme: ThemeMode) {
    this._theme.set(theme);
    localStorage.setItem('theme-preference', theme);
  }

  private applyTheme(mode: ThemeMode) {
    const isDark = mode === 'dark' || (mode === 'system' && window.matchMedia('(prefers-color-scheme: dark)').matches);
    document.documentElement.setAttribute('data-theme', isDark ? 'dark' : 'light');
  }
}
