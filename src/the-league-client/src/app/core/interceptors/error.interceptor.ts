import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const notifications = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      switch (error.status) {
        case 403:
          router.navigate(['/unauthorized']);
          break;
        case 404:
          notifications.error('Resource not found.');
          break;
        case 429:
          notifications.warning('Too many requests. Please slow down.');
          break;
        case 500:
          notifications.error('An unexpected error occurred. Please try again.');
          break;
        case 0:
          notifications.warning('Network error. Please check your connection.');
          break;
      }
      return throwError(() => error);
    })
  );
};
