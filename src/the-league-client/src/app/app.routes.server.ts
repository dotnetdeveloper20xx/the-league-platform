import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  {
    path: 'match/:id',
    renderMode: RenderMode.Client
  },
  {
    path: 'admin/**',
    renderMode: RenderMode.Client
  },
  {
    path: 'club/**',
    renderMode: RenderMode.Client
  },
  {
    path: 'portal/**',
    renderMode: RenderMode.Client
  },
  {
    path: 'auth/**',
    renderMode: RenderMode.Client
  },
  {
    path: 'onboarding',
    renderMode: RenderMode.Client
  },
  {
    path: 'unauthorized',
    renderMode: RenderMode.Client
  },
  {
    path: '**',
    renderMode: RenderMode.Prerender
  }
];
