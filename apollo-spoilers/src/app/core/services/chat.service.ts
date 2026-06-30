import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpContext } from '@angular/common/http';
import { Observable } from 'rxjs';;
import { ChatResponse, ChatMessage } from '../models/models';
import { SKIP_LOADING } from './loading.service';
import { environment } from '../../../environments/environment.prod';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private http = inject(HttpClient);

  /** Context shared by all chat requests so they skip the full-screen Apollo loader. */
  private readonly skipLoaderCtx = new HttpContext().set(SKIP_LOADING, true);

  send(message: string, sessionId?: string): Observable<ChatResponse> {
    return this.http.post<ChatResponse>(
      `${environment.apiUrl}/chat`,
      { message, sessionId },
      { context: this.skipLoaderCtx }
    );
  }

  history(sessionId: string): Observable<ChatMessage[]> {
    return this.http.get<ChatMessage[]>(
      `${environment.apiUrl}/chat/${sessionId}/history`,
      { context: this.skipLoaderCtx }
    );
  }
}
