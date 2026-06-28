import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../../core/services/loading.service';

/**
 * Full-screen branded overlay loader. Visible whenever the global
 * LoadingService reports in-flight requests. Includes the Apollo gauge
 * mark plus a dual spinning ring — matches the brand identity.
 */
@Component({
  selector: 'app-loader',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './app-loader.component.html',
  styleUrl: './app-loader.component.css',
})
export class AppLoaderComponent {
  private readonly loadingService = inject(LoadingService);
  readonly isLoading = this.loadingService.isLoading;
}
