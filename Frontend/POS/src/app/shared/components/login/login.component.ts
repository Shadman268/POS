import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
    loginForm!: FormGroup;
    loading = false;
    submitted = false;
    error = '';
    returnUrl = '';

    constructor(
        private formBuilder: FormBuilder,
        private route: ActivatedRoute,
        private router: Router,
        private authService: AuthService
    ) {
        // Redirect to home if already logged in
        if (this.authService.currentUserValue) {
            this.router.navigate(['/pos']);
        }
    }

    ngOnInit(): void {
        this.loginForm = this.formBuilder.group({
            username: ['', Validators.required],
            password: ['', Validators.required]
        });

        // Get return url from route parameters or default to '/'
        this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/pos';
    }

    get f() {
        return this.loginForm.controls;
    }

    onSubmit(): void {
        this.submitted = true;
        this.error = '';

        // Stop if form is invalid
        if (this.loginForm.invalid) {
            return;
        }

        this.loading = true;
        this.authService.login(this.loginForm.value).subscribe({
            next: () => {
                this.router.navigate([this.returnUrl]);
            },
            error: error => {
                this.error = error.error?.message || 'Invalid username or password';
                this.loading = false;
            }
        });
    }
}
