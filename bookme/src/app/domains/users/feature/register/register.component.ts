import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';

import { AuthLayoutComponent } from '../../ui/auth-layout/auth-layout.component';
import { AuthService } from '../../../../core/services/auth.service';
import {
  applyServerErrors,
  getServerErrorMessage,
  hasValidationErrors,
} from '../../../../core/utils/form-error-handler';
import { HttpErrorResponse } from '@angular/common/http';
import { Gender } from '../../../../core/models/auth.interface';

/**
 * Custom validator: checks that "confirmPassword" matches "password".
 */
function passwordsMatch(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password');
  const confirm = control.get('confirmPassword');

  if (password && confirm && password.value !== confirm.value) {
    confirm.setErrors({ passwordMismatch: true });
    return { passwordMismatch: true };
  }
  return null;
}

@Component({
  selector: 'app-register',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatSelectModule,
    RouterLink,
    AuthLayoutComponent,
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  readonly Gender = Gender;

  hidePassword = signal(true);
  hideConfirm = signal(true);
  isLoading = signal(false);
  serverError = signal<string | null>(null);

  registerForm = this.fb.nonNullable.group(
    {
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      phoneNumber: ['', [Validators.required, Validators.pattern(/^[0-9]{7,15}$/)]],
      gender: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordsMatch }
  );

  togglePasswordVisibility(): void {
    this.hidePassword.update((v) => !v);
  }

  toggleConfirmVisibility(): void {
    this.hideConfirm.update((v) => !v);
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.serverError.set(null);

    const { confirmPassword, ...registerData } = this.registerForm.getRawValue();

    this.authService.register(registerData).subscribe({
      next: () => {
        this.isLoading.set(false);
        // Redirect to verify-email with the email as state
        this.router.navigate(['/auth/verify-email'], {
          queryParams: { email: this.registerForm.controls.email.value },
        });
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading.set(false);

        if (hasValidationErrors(err)) {
          applyServerErrors(this.registerForm, err);
        } else {
          this.serverError.set(getServerErrorMessage(err));
        }
      },
    });
  }
}
