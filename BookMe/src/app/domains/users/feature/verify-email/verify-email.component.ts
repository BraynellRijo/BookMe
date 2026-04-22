import {
  Component,
  inject,
  signal,
  OnInit,
  ViewChildren,
  QueryList,
  ElementRef,
  AfterViewInit,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

import { AuthLayoutComponent } from '../../ui/auth-layout/auth-layout.component';
import { AuthService } from '../../../../core/services/auth.service';
import { getServerErrorMessage } from '../../../../core/utils/form-error-handler';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-verify-email',
  imports: [FormsModule, MatIconModule, RouterLink, AuthLayoutComponent],
  templateUrl: './verify-email.component.html',
  styleUrl: './verify-email.component.scss',
})
export class VerifyEmailComponent implements OnInit, AfterViewInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  @ViewChildren('digitInput') digitInputs!: QueryList<ElementRef<HTMLInputElement>>;

  /** 6-digit code as individual characters */
  digits = signal<string[]>(['', '', '', '', '', '']);

  /** Email passed from the registration flow */
  email = signal<string>('');

  isLoading = signal(false);
  isSuccess = signal(false);
  serverError = signal<string | null>(null);
  resendCooldown = signal(0);

  ngOnInit(): void {
    // Read email from query params
    const emailParam = this.route.snapshot.queryParamMap.get('email');
    if (emailParam) {
      this.email.set(emailParam);
    }
  }

  ngAfterViewInit(): void {
    // Focus the first input on load
    setTimeout(() => {
      const inputs = this.digitInputs.toArray();
      if (inputs.length > 0) {
        inputs[0].nativeElement.focus();
      }
    });
  }

  onDigitInput(index: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;

    // Only allow single digits
    if (value.length > 1) {
      input.value = value.charAt(value.length - 1);
    }

    const currentDigits = [...this.digits()];
    currentDigits[index] = input.value;
    this.digits.set(currentDigits);

    // Auto-advance to next input
    if (value && index < 5) {
      const inputs = this.digitInputs.toArray();
      inputs[index + 1].nativeElement.focus();
    }

    // Auto-submit when all 6 digits are entered
    if (currentDigits.every((d) => d !== '')) {
      this.onSubmit();
    }
  }

  onKeyDown(index: number, event: KeyboardEvent): void {
    // Handle backspace: clear current and go back
    if (event.key === 'Backspace' && index > 0) {
      const currentDigits = [...this.digits()];
      if (!currentDigits[index]) {
        const inputs = this.digitInputs.toArray();
        inputs[index - 1].nativeElement.focus();
        currentDigits[index - 1] = '';
        this.digits.set(currentDigits);
      }
    }
  }

  onPaste(event: ClipboardEvent): void {
    event.preventDefault();
    const pasted = event.clipboardData?.getData('text')?.trim() ?? '';
    const chars = pasted.replace(/\D/g, '').split('').slice(0, 6);

    if (chars.length > 0) {
      const newDigits = ['', '', '', '', '', ''];
      chars.forEach((c, i) => (newDigits[i] = c));
      this.digits.set(newDigits);

      // Update input values and focus last filled
      const inputs = this.digitInputs.toArray();
      newDigits.forEach((d, i) => { inputs[i].nativeElement.value = d; });

      const focusIndex = Math.min(chars.length, 5);
      inputs[focusIndex].nativeElement.focus();

      if (chars.length === 6) {
        this.onSubmit();
      }
    }
  }

  onSubmit(): void {
    const code = this.digits().join('');
    if (code.length !== 6) return;

    this.isLoading.set(true);
    this.serverError.set(null);

    this.authService
      .verifyEmail({ email: this.email(), code })
      .subscribe({
        next: () => {
          this.isLoading.set(false);
          this.isSuccess.set(true);

          // Redirect to login after a brief success animation
          setTimeout(() => {
            this.router.navigate(['/auth/login']);
          }, 2000);
        },
        error: (err: HttpErrorResponse) => {
          this.isLoading.set(false);
          this.serverError.set(getServerErrorMessage(err));

          // Clear digits on error
          this.digits.set(['', '', '', '', '', '']);
          const inputs = this.digitInputs.toArray();
          inputs.forEach((inp) => (inp.nativeElement.value = ''));
          inputs[0].nativeElement.focus();
        },
      });
  }

  resendCode(): void {
    if (this.resendCooldown() > 0) return;

    // Start cooldown
    this.resendCooldown.set(60);
    const interval = setInterval(() => {
      this.resendCooldown.update((v) => {
        if (v <= 1) {
          clearInterval(interval);
          return 0;
        }
        return v - 1;
      });
    }, 1000);

    // TODO: Call resend-code endpoint when available
    // this.authService.resendVerificationCode(this.email()).subscribe();
  }
}
