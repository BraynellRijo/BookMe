import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toastService = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'Ha ocurrido un error inesperado';

      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = `Error: ${error.error.message}`;
      } else {
        // Server-side error
        // Try to extract the most descriptive message
        if (error.error && typeof error.error === 'object') {
          errorMessage = error.error.message || error.error.detail || error.error.title || errorMessage;
          
          // Handle ASP.NET Core Validation Errors (Problem Details)
          if (error.error.errors && typeof error.error.errors === 'object') {
             const validationErrors = Object.values(error.error.errors).flat();
             if (validationErrors.length > 0) {
               errorMessage = validationErrors[0] as string;
             }
          }
        } else if (typeof error.error === 'string' && error.error.length < 200) {
          errorMessage = error.error;
        } else if (error.status === 401) {
          // Ignore 401 for public-facing detail endpoints to avoid annoying toasts for guests
          const isPublicRoute = ['/Listing-Rating', '/Listing-Reviews', '/Listing-Blocks', '/images'].some(p => req.url.includes(p));
          if (isPublicRoute) {
            console.warn(`Silencing 401 for public route: ${req.url}`);
            return throwError(() => error);
          }
          errorMessage = 'Sesión expirada o no autorizada';
        } else if (error.status === 403) {
          errorMessage = 'No tienes permisos para realizar esta acción';
        } else if (error.status === 404) {
          errorMessage = 'El recurso solicitado no fue encontrado';
        } else if (error.status === 500) {
          errorMessage = 'Error interno del servidor. Por favor, intenta más tarde';
        }
      }

      toastService.error(errorMessage);
      console.error(`API Error [${error.status}]:`, error);
      
      return throwError(() => error);
    })
  );
};
