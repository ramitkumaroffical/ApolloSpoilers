import { Injectable, signal, computed } from '@angular/core';
import { HttpContextToken } from '@angular/common/http';

/**
 * Per-request opt-out for the global (full-screen) loading indicator.
 * Set this on any request that has its own inline/ contextual UI — e.g.
 * the AasRa chat, which renders a professional in-panel typing indicator
 * instead of flashing the branded Apollo overlay on every message.
 *
 * Usage:
 *   this.http.post(url, body, { context: new HttpContext().set(SKIP_LOADING, true) })
 */
export const SKIP_LOADING = new HttpContextToken<boolean>(() => false);

/**
 * Tracks the number of in-flight HTTP requests and exposes a reactive
 * `isLoading` signal. The loading interceptor calls `start()` / `stop()`
 * for every request that targets the API.
 */
@Injectable({ providedIn: 'root' })
export class LoadingService {
  private readonly _pending = signal(0);

  /** True when at least one tracked request is in flight. */
  readonly isLoading = computed(() => this._pending() > 0);

  /** Current number of in-flight requests. */
  readonly pendingCount = this._pending.asReadonly();

  start(): void {
    this._pending.update(n => n + 1);
  }

  stop(): void {
    this._pending.update(n => Math.max(0, n - 1));
  }

  /** Reset to zero — useful for route-change cleanup. */
  reset(): void {
    this._pending.set(0);
  }
}
