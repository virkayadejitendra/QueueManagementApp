import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthApi } from './auth-api';
import { AuthTokenStorage } from './auth-token-storage';
import { LoginResponse } from './auth.models';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authApi = inject(AuthApi);
  private readonly authTokenStorage = inject(AuthTokenStorage);
  private readonly router = inject(Router);

  protected readonly isLoggingIn = signal(false);
  protected readonly loginErrorMessage = signal<string | null>(null);
  protected readonly login = signal<LoginResponse | null>(null);

  protected readonly loginForm = this.formBuilder.nonNullable.group({
    identifier: ['', [Validators.required, Validators.maxLength(254)]],
    password: ['', [Validators.required, Validators.maxLength(100)]]
  });

  protected submitLogin(): void {
    this.loginErrorMessage.set(null);
    this.login.set(null);

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      this.loginErrorMessage.set('Enter your email or mobile number and password.');
      return;
    }

    const formValue = this.loginForm.getRawValue();
    this.isLoggingIn.set(true);

    this.authApi.login({
      identifier: formValue.identifier.trim(),
      password: formValue.password
    }).subscribe({
      next: response => {
        this.authTokenStorage.setToken(response.accessToken);
        this.login.set(response);
        this.loginForm.reset();
        this.isLoggingIn.set(false);
        void this.router.navigateByUrl('/dashboard');
      },
      error: error => {
        this.loginErrorMessage.set(this.getLoginErrorMessage(error));
        this.isLoggingIn.set(false);
      }
    });
  }

  private getLoginErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === 401) {
      return 'Email, mobile number, or password is incorrect.';
    }

    if (error instanceof HttpErrorResponse && error.status === 400) {
      return 'Enter your email or mobile number and password.';
    }

    return 'Login could not be completed. Check that the API is running and try again.';
  }
}
