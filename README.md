# POS – Point of Sale System

A full-stack Point of Sale application built with a **.NET REST API** backend and an **Angular** frontend.

---

## What It Does

This POS system allows users to manage products and complete sales transactions through a clean, responsive interface.

**Key Features:**

- **User Authentication** – Register and log in with JWT-based authentication. Access tokens are short-lived, and HttpOnly cookies store refresh tokens for secure, seamless session renewal. Role-based claims are attached to each token.
- **Product Management** – Add products with a name, price, and image upload. New products are broadcast in real time to all connected clients via **SignalR**, so every open session reflects inventory changes instantly.
- **Receipt / Checkout** – Build a cart by selecting products, apply quantities, and complete a sale to generate a receipt. Receipts are persisted and can be retrieved individually or listed in full.
- **Swagger UI** – The API is self-documented via Swagger/OpenAPI, available at `/swagger` during development.
- **SQL Server** – Data is stored in a relational database managed through Entity Framework Core migrations.

---

## AI Tools Used

- **GitHub Copilot** – Used for code completion and boilerplate generation throughout the development of the backend services, repositories, DTOs, and Angular components.
- **ChatGPT** – Used to troubleshoot integration issues (e.g. SignalR CORS setup with `AllowCredentials`, JWT refresh-token cookie configuration) and to explain .NET and Angular concepts during development.

---

## What I Learned

- **ASP.NET Core REST API** – How to structure a .NET Web API project using controllers, services, repositories, and DTOs with a clean separation of concerns.
- **JWT Authentication** – Implementing access token and refresh token flows, storing refresh tokens in HttpOnly cookies, and protecting endpoints with `[Authorize]`.
- **SignalR** – Setting up a real-time hub (`ProductHub`) and broadcasting events to all connected clients so the UI updates without polling.
- **Entity Framework Core** – Defining models, configuring relationships, and managing the database schema through migrations.
- **AutoMapper** – Mapping between domain models and DTOs to keep controllers and services decoupled from the data layer.
- **Angular Fundamentals** – Building components, services, routing, reactive forms, HTTP interceptors for attaching JWT tokens, and consuming a SignalR hub from the frontend.
- **CORS Configuration** – Configuring a strict CORS policy that allows credentials, which is required for both SignalR and cookie-based refresh tokens.

---

## What I Would Improve

- **Robust Search & Filtering** – Add server-side search, filtering by category or price range, and pagination for the product list so the app scales to large inventories.
- **Better Discount System** – Support percentage-based and fixed-amount discounts, coupon codes, and line-item promotions rather than a single flat discount.
- **Offline / Cache Support** – Implement a service worker (PWA) or local storage cache so the cashier can continue processing sales during brief network outages and sync when connectivity is restored.
- **Role-Based Access Control** – Introduce distinct roles (e.g. Admin, Cashier) and restrict sensitive operations like adding products or viewing all receipts to administrators only.
- **Unit & Integration Tests** – Add xUnit tests for the backend services and Jasmine/Karma tests for Angular components to catch regressions early.
- **Containerization** – Dockerize the backend and frontend to simplify local setup and deployment pipelines.
- **Reporting Dashboard** – Add sales summaries, daily revenue charts, and best-selling product reports so managers have at-a-glance business insights.

---

## Tech Stack

| Layer     | Technology                                      |
|-----------|-------------------------------------------------|
| Backend   | ASP.NET Core (.NET), Entity Framework Core, SignalR, JWT Bearer, SQL Server |
| Frontend  | Angular, TypeScript, SCSS                       |
| Tooling   | Swagger / OpenAPI, AutoMapper                   |

---

## Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) (version used by the project)
- [Node.js & npm](https://nodejs.org/)
- SQL Server (local or remote)

### Backend

```bash
cd Backend
# Update the connection string in appsettings.json
dotnet ef database update
dotnet run
```

The API will be available at `https://localhost:{port}` and Swagger UI at `/swagger`.

### Frontend

```bash
cd Frontend/POS
npm install
ng serve
```

The Angular app will be available at `http://localhost:4200`.
