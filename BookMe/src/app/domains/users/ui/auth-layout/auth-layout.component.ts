import { Component, input } from '@angular/core';

/**
 * Auth Layout — Shared split-screen layout for all auth pages.
 *
 * Left side: Hero image with glassmorphism brand overlay.
 * Right side: Content projected via <ng-content>.
 *
 * Follows the "Terra & Tide" editorial design system:
 * - No borders, tonal layering
 * - Manrope headlines, Inter body
 * - Deep forest greens + warm beiges
 */
@Component({
  selector: 'app-auth-layout',
  templateUrl: './auth-layout.component.html',
  styleUrl: './auth-layout.component.scss',
})
export class AuthLayoutComponent {
  /** Tagline displayed on the hero overlay */
  tagline = input<string>('Your next escape is just a moment away.');
}
