import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/user';

@Component({
    selector: 'app-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
    registerForm!: FormGroup;
    loading = false;
    submitted = false;
    error = '';
    success = '';
    roles = [
        { value: UserRole.Admin, label: 'Admin' },
        { value: UserRole.Cashier, label: 'Cashier' },
        { value: UserRole.Salesperson, label: 'Salesperson' }
    ];

    constructor(
        private formBuilder: FormBuilder,
        private router: Router,
        private authService: AuthService
    ) {
        // Redirect to home if already logged in
        if (this.authService.currentUserValue) {
            this.router.navigate(['/pos']);
        }
    }

    ngOnInit(): void {
        this.registerForm = this.formBuilder.group({
            username: ['', [Validators.required, Validators.minLength(3)]],
            password: ['', [Validators.required, Validators.minLength(6)]],
            confirmPassword: ['', Validators.required],
            role: [UserRole.Cashier, Validators.required]
        }, {
            validators: this.passwordMatchValidator
        });
    }

    passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
        const password = control.get('password');
        const confirmPassword = control.get('confirmPassword');

        if (!password || !confirmPassword) {
            return null;
        }

        return password.value === confirmPassword.value ? null : { passwordMismatch: true };
    }

    get f() {
        return this.registerForm.controls;
    }

    onSubmit(): void {
        this.submitted = true;
        this.error = '';
        this.success = '';

        // Stop if form is invalid
        if (this.registerForm.invalid) {
            return;
        }

        this.loading = true;
        this.authService.register(this.registerForm.value).subscribe({
            next: (response) => {
                this.success = 'Registration successful! Redirecting to login...';
                setTimeout(() => {
                    this.router.navigate(['/login']);
                }, 2000);
            },
            error: error => {
                this.error = error.error?.message || 'Registration failed. Please try again.';
                this.loading = false;
            }
        });
    }

    goToLogin(): void {
        this.router.navigate(['/login']);
    }
}
