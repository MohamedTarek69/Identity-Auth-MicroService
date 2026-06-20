<h1 align="center">🔐 Identity & Authentication MicroService</h1>

<p align="center">
  <b>ASP.NET Core Web API | ASP.NET Identity | JWT | Refresh Tokens</b>
</p>

<p align="center">
  A dedicated authentication and authorization microservice responsible for user management, secure login, JWT token generation, refresh token handling, and account operations within the MedGuide Healthcare Platform.
</p>

---

## 🏗️ Project Overview

The Identity & Authentication MicroService centralizes authentication and authorization for all MedGuide services.

It provides secure user registration, login, JWT access tokens, refresh tokens, password management, and user administration while ensuring loose coupling between healthcare services.

---

## 🎯 Goals

- Centralize authentication across microservices.
- Manage user accounts securely.
- Generate and validate JWT tokens.
- Support Refresh Token workflows.
- Enable role-based access control.
- Improve security and maintainability.

---

## ✨ Main Features

| Feature | Description |
|----------|-------------|
| 🔐 User Authentication | Secure login and registration |
| 🎫 JWT Tokens | Access token generation and validation |
| 🔄 Refresh Tokens | Token renewal without re-login |
| 👤 User Management | Update, retrieve, and delete users |
| 🔑 Password Management | Change and update passwords |
| 📧 Email Validation | Check existing user emails |
| 🚪 Logout | Token invalidation and logout support |
| 🔗 Microservice Integration | Authentication provider for all services |

---

## 🧱 Architecture

The service follows a microservices-based architecture:

```text
Client
 │
 ▼
Identity MicroService
 │
 ├── ASP.NET Identity
 │
 ├── JWT Authentication
 │
 ├── Refresh Tokens
 │
 └── SQL Server
```

### Benefits

- Centralized Authentication
- Independent Deployment
- Scalability
- Security
- Loose Coupling

---

## 🧰 Tech Stack

| Category | Technology |
|-----------|-------------|
| Backend | ASP.NET Core Web API |
| Authentication | ASP.NET Identity |
| Authorization | JWT Bearer Tokens |
| Token Management | Refresh Tokens |
| Database | SQL Server |
| ORM | Entity Framework Core |
| Documentation | Swagger |
| Architecture | Microservices |
| Version Control | Git & GitHub |

---

## 🔌 API Endpoints

### Register User

```http
POST /Clinic/Authentication/Register
```

Creates a new user account.

---

### Login

```http
POST /Clinic/Authentication/Login
```

Authenticates a user and generates JWT tokens.

---

### Refresh Token

```http
POST /Clinic/Authentication/Refresh
```

Generates a new access token using a valid refresh token.

---

### Logout

```http
POST /Clinic/Authentication/Logout
```

Logs the user out and invalidates refresh tokens.

---

### Check Email Availability

```http
GET /Clinic/Authentication/EmailExists
```

Checks whether an email is already registered.

---

### Get Current User

```http
GET /Clinic/Authentication/CurrentUser
```

Returns authenticated user information.

---

### Get User By Id

```http
GET /Clinic/Authentication/UserById/{id}
```

Returns user details.

---

### Get All Users

```http
GET /Clinic/Authentication/AllUsers
```

Retrieves all registered users.

---

### Update User

```http
PATCH /Clinic/Authentication/UpdateUser/{id}
```

Updates user profile information.

---

### Update Password

```http
PATCH /Clinic/Authentication/UpdatePassword/{id}
```

Changes a user's password securely.

---

### Delete User

```http
DELETE /Clinic/Authentication/DeleteUser
```

Removes a user account.

---

## 🔒 Security Features

### JWT Authentication

- Access Token Generation
- Token Validation
- Claims-Based Authorization

### Refresh Tokens

- Token Renewal
- Secure Session Management
- Extended Authentication Lifetime

### Password Security

- Password Hashing
- Identity Security Policies
- Credential Protection

---

## 📦 Core Models

### RegisterDTO

Used for:

- User Registration
- Account Creation

### LoginDTO

Used for:

- User Authentication
- Token Generation

### RefreshRequestDTO

Used for:

- Refresh Token Requests

### UpdateUserDTO

Used for:

- Profile Updates

### UpdatePasswordDTO

Used for:

- Password Changes

---

## 🗄️ Data Flow

```text
Register/Login
        │
        ▼
Authentication Controller
        │
        ▼
Identity Service
        │
 ┌──────┴──────┐
 ▼             ▼
JWT         Database
Token       User Data
        │
        ▼
Authenticated User
```

---

## 🚀 Getting Started

### Clone Repository

```bash
git clone <repository-url>
```

### Configure Database

Update:

```json
appsettings.json
```

### Apply Migrations

```powershell
Update-Database
```

### Run Application

```bash
dotnet run
```

---

## 📌 Future Enhancements

- OAuth2 Integration
- Google Authentication
- Two-Factor Authentication (2FA)
- Email Verification
- Docker Support
- Kubernetes Deployment

---

## 👨‍💻 Author

**Mohamed Tarek**

- GitHub: https://github.com/MohamedTarek69

---

<p align="center">
🔐 Secure Authentication & Authorization for the MedGuide Microservices Ecosystem.
</p>
