import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

import { AuthLayoutComponent } from '../../ui/auth-layout/auth-layout.component';
import { AuthService } from '../../../../core/services/auth.service';
import {
  applyServerErrors,
  getServerErrorMessage,
  hasValidationErrors,
} from '../../../../core/utils/form-error-handler';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    RouterLink,
    AuthLayoutComponent,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  /** Controls password visibility toggle */
  hidePassword = signal(true);

  /** Loading state for the submit button */
  isLoading = signal(false);

  /** General error message (non-field-specific) */
  serverError = signal<string | null>(null);

  /** Reactive form with strict typing */
  loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  togglePasswordVisibility(): void {
    this.hidePassword.update((v) => !v);
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.serverError.set(null);

    this.authService.login(this.loginForm.getRawValue()).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        this.router.navigate(['/']);
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading.set(false);

        if (hasValidationErrors(err)) {
          applyServerErrors(this.loginForm, err);
        } else {
          this.serverError.set(getServerErrorMessage(err));
        }
      },
    });
  }
}
