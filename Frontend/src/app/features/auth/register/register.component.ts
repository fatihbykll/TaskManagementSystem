import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service';
/**
 * Şifre eşleşme validatörü.
 * AbstractControl düzeyinde çalışır; iki ayrı field'ı birbirine karşı doğrular.
 * Bu işlem field-level değil form-level'da yapılır; çünkü her iki değere birden ihtiyaç duyulur.
 */
const passwordMatchValidator: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
  const password = group.get('password')?.value;
  const confirmPassword = group.get('confirmPassword')?.value;
  return password === confirmPassword ? null : { passwordMismatch: true };
};
@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  isLoading = false;
  hidePassword = true;
  hideConfirmPassword = true;
  registerForm = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName:  ['', [Validators.required, Validators.minLength(2)]],
    email:     ['', [Validators.required, Validators.email]],
    password:  ['', [Validators.required, Validators.minLength(8),
                     Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).+$/)]],
    confirmPassword: ['', Validators.required]
  }, { validators: passwordMatchValidator });
  get firstName() { return this.registerForm.get('firstName'); }
  get lastName()  { return this.registerForm.get('lastName'); }
  get email()     { return this.registerForm.get('email'); }
  get password()  { return this.registerForm.get('password'); }
  get confirmPassword() { return this.registerForm.get('confirmPassword'); }
  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }
    this.isLoading = true;
    const { firstName, lastName, email, password } = this.registerForm.value;
    this.authService.register({
      firstName: firstName!,
      lastName:  lastName!,
      email:     email!,
      password:  password!
    }).subscribe({
      next: (response) => {
        if (response.success) {
          this.router.navigate(['/tasks']);
        }
      },
      error: () => {
        this.isLoading = false;
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }
}
