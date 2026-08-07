export enum UserRole {
    Admin = 'admin',
    Cashier = 'cashier',
    Salesperson = 'salesperson'
}

export interface User {
    id: string;
    username: string;
    role: UserRole;
    tenantId?: string;
    shopCode?: string;
    tenantName?: string;
}

export interface LoginRequest {
    shopCode: string;
    username: string;
    password: string;
}

export interface LoginResponse {
    accessToken: string;
    user: User;
}

export interface RefreshTokenResponse {
    accessToken: string;
    user: User;
}

export interface RegisterRequest {
    shopCode: string;
    username: string;
    password: string;
    confirmPassword: string;
    role: UserRole;
}

export interface RegisterResponse {
    message: string;
    user: User;
}
