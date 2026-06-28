import { Component, inject, signal, ElementRef, ViewChild, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { ChatService } from '../../core/services/chat.service';
import { AuthService } from '../../core/services/auth.service';
import { extractErrorMessage } from '../../core/interceptors/auth.interceptor';

interface UiMessage {
  role: 'user' | 'assistant';
  content: string;
  sources?: { productName?: string; productSlug?: string }[];
  /** Optional action attached to an error message (e.g. retry). */
  retry?: () => void;
}

@Component({
  selector: 'app-aasRa-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, MatButtonModule, MatIconModule, RouterModule],
  templateUrl: './aasRa-chat.component.html',
  styleUrl: './aasRa-chat.component.css',
})
export class AasRaChatComponent implements AfterViewChecked {
  private chatSvc = inject(ChatService);
  public auth = inject(AuthService);

  readonly isOpen = signal(false);
  readonly messages = signal<UiMessage[]>([]);
  readonly loading = signal(false);
  readonly sessionId = signal<string | null>(null);
  draft = '';

  @ViewChild('scrollContainer') scrollContainer?: ElementRef;

  toggle(): void {
    this.isOpen.update(v => !v);
  }

  send(text?: string): void {
    const message = (text ?? this.draft).trim();
    if (!message || this.loading()) return;

    this.messages.update(m => [...m, { role: 'user', content: message }]);
    this.draft = '';
    this.loading.set(true);

    this.chatSvc.send(message, this.sessionId() ?? undefined).subscribe({
      next: res => {
        this.sessionId.set(res.sessionId);
        this.messages.update(m => [...m, {
          role: 'assistant',
          content: res.answer,
          sources: res.sources.map(s => ({ productName: s.productName, productSlug: s.productSlug }))
        }]);
        this.loading.set(false);
      },
      error: e => {
        this.messages.update(m => [...m, {
          role: 'assistant',
          content: "I couldn't reach the server just now. Please check your connection and try again.\n\nDetails: " + extractErrorMessage(e),
          retry: () => this.retryMessage(message),
        }]);
        this.loading.set(false);
      }
    });
  }

  /** Re-send the last failed message. */
  retryMessage(message: string): void {
    // Remove the trailing assistant error message
    this.messages.update(m => m.filter(x => !(x.role === 'assistant' && x.retry)));
    this.send(message);
  }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  private scrollToBottom(): void {
    try {
      const el = this.scrollContainer?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    } catch { /* ignore */ }
  }
}
