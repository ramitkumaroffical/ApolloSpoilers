import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { AasRaChatComponent } from './aasRa-chat.component';
import { ChatService } from '../../core/services/chat.service';
import { AuthService } from '../../core/services/auth.service';

describe('AasRaChatComponent', () => {
  let mockChat: { send: ReturnType<typeof vi.fn> };
  let mockAuth: { isAuthenticated: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    mockChat = { send: vi.fn() };
    mockAuth = { isAuthenticated: vi.fn().mockReturnValue(true) };

    await TestBed.configureTestingModule({
      imports: [AasRaChatComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: ChatService, useValue: mockChat },
        { provide: AuthService, useValue: mockAuth },
      ],
    }).compileComponents();
  });

  function create() {
    const fixture = TestBed.createComponent(AasRaChatComponent);
    fixture.detectChanges();
    return fixture;
  }

  it('should create', () => {
    const fixture = create();
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('toggles the panel open/closed', () => {
    const fixture = create();
    const cmp = fixture.componentInstance;
    expect(cmp.isOpen()).toBe(false);
    cmp.toggle();
    expect(cmp.isOpen()).toBe(true);
    cmp.toggle();
    expect(cmp.isOpen()).toBe(false);
  });

  it('sends a message and appends the assistant reply', () => {
    mockChat.send.mockReturnValue(of({
      sessionId: 's1',
      answer: 'Hi there',
      sources: [{ productName: 'Wing', productSlug: 'wing' }],
    }));
    const fixture = create();
    const cmp = fixture.componentInstance;

    cmp.send('Hello');
    expect(mockChat.send).toHaveBeenCalledWith('Hello', undefined);
    expect(cmp.messages().length).toBe(2);
    expect(cmp.messages()[0].role).toBe('user');
    expect(cmp.messages()[1].content).toBe('Hi there');
    expect(cmp.sessionId()).toBe('s1');
    expect(cmp.loading()).toBe(false);
  });

  it('ignores empty messages', () => {
    const fixture = create();
    fixture.componentInstance.draft = '   ';
    fixture.componentInstance.send();
    expect(mockChat.send).not.toHaveBeenCalled();
  });
});
