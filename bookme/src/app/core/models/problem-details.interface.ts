/**
 * RFC 7807 Problem Details interface.
 * This is the standardized error format returned by the .NET Core backend.
 * All API errors follow this structure — never plain text strings.
 *
 * @see https://tools.ietf.org/html/rfc7807
 */
export interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
  /**
   * Validation errors keyed by field name (PascalCase from backend).
   * Each field maps to an array of error messages.
   * Only present on 400 Bad Request with validation failures.
   *
   * Example:
   * {
   *   "Email": ["This email address is already registered."],
   *   "Password": ["Password must contain at least one uppercase letter."]
   * }
   */
  errors?: Record<string, string[]>;
}
