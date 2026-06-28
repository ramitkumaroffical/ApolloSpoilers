import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { LoadingService, SKIP_LOADING } from '../services/loading.service';
import { environment } from '../../../environments/environment';

/**
 * Auto-tracks every request that targets the API URL, keeping the
 * global LoadingService in sync. Requests to other origins (fonts,
 * analytics, etc.) are ignored. Requests that set the SKIP_LOADING
 * context token are also ignored — these own their own inline UI
 * (e.g. the AasRa chat panel's typing indicator).
 */
export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loading = inject(LoadingService);

  if (!req.url.startsWith(environment.apiUrl) || req.context.get(SKIP_LOADING)) {
    return next(req);
  }

  loading.start();
  return next(req).pipe(finalize(() => loading.stop()));
};
