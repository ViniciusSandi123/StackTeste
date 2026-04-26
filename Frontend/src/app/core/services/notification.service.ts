import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private snackBar = inject(MatSnackBar);

  success(message: string): void {
    this.snackBar.open(message, 'OK', {
      duration: 3000,
      panelClass: ['snackbar-success'],
      horizontalPosition: 'end',
      verticalPosition: 'top',
    });
  }

  error(message: string): void {
    this.snackBar.open(message, 'Fechar', {
      duration: 5000,
      panelClass: ['snackbar-error'],
      horizontalPosition: 'end',
      verticalPosition: 'top',
    });
  }

  extractError(err: any, fallback: string): string {
    const problem = err?.error;
    if (!problem) return fallback;

    if (problem.errors && typeof problem.errors === 'object') {
      const messages = Object.values(problem.errors).flat() as string[];
      if (messages.length > 0) return messages.join(' ');
    }

    return problem.detail || problem.title || problem.message || fallback;
  }
}
