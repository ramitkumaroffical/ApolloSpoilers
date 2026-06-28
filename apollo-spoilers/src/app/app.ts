import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatMenuModule } from '@angular/material/menu';
import { AuthService } from './core/services/auth.service';
import { CartService } from './core/services/cart.service';
import { NotificationService } from './core/services/notification.service';
import { AasRaChatComponent } from './shared/aasRa-chat/aasRa-chat.component';
import { FooterComponent } from './shared/footer/footer.component';
import { AppLoaderComponent } from './shared/components/app-loader/app-loader.component';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet, RouterLink, RouterLinkActive,
    MatToolbarModule, MatButtonModule, MatIconModule, MatBadgeModule, MatMenuModule,
    AasRaChatComponent, FooterComponent, AppLoaderComponent
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly auth = inject(AuthService);
  protected readonly cart = inject(CartService);
  private readonly notify = inject(NotificationService);

  logout(): void {
    this.auth.logout();
    this.notify.info('You have been signed out.');
  }
}
