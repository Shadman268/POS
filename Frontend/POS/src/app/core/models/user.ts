export enum UserRole {
    Admin = 'admin',
    Cashier = 'cashier',
    Salesperson = 'salesperson'
}

export interface User {
    id: string;
    username: string;
    role: UserRole;
}

export interface LoginRequest {
    username: string;
    password: string;
}

export interface LoginResponse {
    accessToken: string;
    user: User;
    // refreshToken is sent via HttpOnly cookie, not in response body
}

export interface RefreshTokenResponse {
    accessToken: string;
    user: User;
}

export interface RegisterRequest {
    username: string;
    password: string;
    confirmPassword: string;
    role: UserRole;
}

export interface RegisterResponse {
    message: string;
    user: User;
}
