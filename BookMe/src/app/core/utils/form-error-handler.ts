import { HttpErrorResponse } from '@angular/common/http';
import { FormGroup } from '@angular/forms';

import { ProblemDetails } from '../models/problem-details.interface';

/**
 * Maps server-side validation errors (RFC 7807 ProblemDetails)
 * to Angular Reactive Form control errors.
 *
 * The backend returns field names in PascalCase (e.g., "Email", "FirstName"),
 * while Angular forms typically use camelCase (e.g., "email", "firstName").
 * This utility handles the conversion automatically.
 *
 * Usage:
 * ```typescript
 * this.authService.register(formValue).subscribe({
 *   error: (err) => applyServerErrors(this.registerForm, err)
 * });
 * ```
 *
 * @param form The Angular FormGroup to apply errors to.
 * @param errorResponse The HttpErrorResponse from the backend.
 */
export function applyServerErrors(
  form: FormGroup,
  errorResponse: HttpErrorResponse
): void {
  const problem = errorResponse.error as ProblemDetails | null;

  if (!problem?.errors) return;

  for (const [field, messages] of Object.entries(problem.errors)) {
    // Convert PascalCase → camelCase (e.g., "FirstName" → "firstName")
    const controlName = toCamelCase(field);
    const control = form.get(controlName);

    if (control && messages.length > 0) {
      // Set the first error message as a custom serverError
      control.setErrors({ serverError: messages[0] });
      // Mark as touched so the error shows immediately
      control.markAsTouched();
    }
  }
}

/**
 * Extract a general error message from a ProblemDetails response.
 * Useful for displaying non-field-specific errors (e.g., "Invalid credentials").
 *
 * @param errorResponse The HttpErrorResponse from the backend.
 * @returns The error detail/title string, or a fallback message.
 */
export function getServerErrorMessage(errorResponse: HttpErrorResponse): string {
  const problem = errorResponse.error as ProblemDetails | null;

  if (problem?.detail) return problem.detail;
  if (problem?.title) return problem.title;

  return 'An unexpected error occurred. Please try again.';
}

/**
 * Check if an HttpErrorResponse contains field-level validation errors.
 */
export function hasValidationErrors(errorResponse: HttpErrorResponse): boolean {
  const problem = errorResponse.error as ProblemDetails | null;
  return !!problem?.errors && Object.keys(problem.errors).length > 0;
}

// ─── Private Helpers ──────────────────────────────────────────────

/** Convert a PascalCase string to camelCase. */
function toCamelCase(str: string): string {
  return str.charAt(0).toLowerCase() + str.slice(1);
}
