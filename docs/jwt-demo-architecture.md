# JWT Authentication Demo - Architecture Documentation

## Overview

This document outlines the architecture for a simple JWT authentication demonstration system using JavaScript frontend and .NET Core backend.

## Architecture Analysis

The system demonstrates JWT authentication flow between a JavaScript frontend and .NET Core backend, showcasing:

- **Token-based authentication** vs session-based
- **Stateless server design** principles
- **Cross-origin authentication** handling
- **Security token lifecycle** management

The system clearly illustrates the JWT workflow: login → token generation → token usage → token refresh/expiry.

## High-Level Architecture

```
┌─────────────────┐    HTTP/JSON     ┌─────────────────┐
│  JavaScript     │ ◄──────────────► │  .NET Core      │
│  Frontend       │    JWT Tokens    │  Web API        │
│  (SPA)          │                  │                 │
└─────────────────┘                  └─────────────────┘
```

## Core Components

### Frontend (JavaScript/HTML)
- Login form with credential capture
- Token storage mechanism (localStorage/sessionStorage)
- HTTP interceptor for automatic token attachment
- Protected route demonstration
- Token expiry handling

### Backend (.NET Core Web API)
- Authentication controller (`/api/auth/login`)
- JWT token generation service
- Protected endpoints with `[Authorize]` attribute
- Token validation middleware
- User credential validation (in-memory for demo)

## Technology Stack

### Frontend
- **Vanilla JavaScript** or lightweight framework (React/Vue)
- **Fetch API** or Axios for HTTP requests
- **Local storage** for token persistence
- **Simple HTML/CSS** for UI

### Backend
- **.NET Core 6+** Web API template
- **Microsoft.AspNetCore.Authentication.JwtBearer** package
- **System.IdentityModel.Tokens.Jwt** for token generation
- **In-memory user store** for simplicity

## Security Considerations

- HTTPS enforcement in production
- Secure token storage practices
- CORS configuration for cross-origin requests
- Token expiration and refresh strategy

## Implementation Strategy

### Phase 1: Backend Foundation
1. Create .NET Core Web API project
2. Configure JWT authentication middleware
3. Implement authentication controller
4. Add protected demo endpoints

### Phase 2: Frontend Development
1. Create simple HTML interface
2. Implement login functionality
3. Add token storage and retrieval
4. Create protected content areas

### Phase 3: Integration & Testing
1. Configure CORS for frontend-backend communication
2. Test complete authentication flow
3. Demonstrate token expiry scenarios
4. Add error handling and user feedback

## JWT Flow Diagram

```
1. User Login
   Frontend → Backend: POST /api/auth/login { username, password }
   
2. Token Generation
   Backend → Frontend: 200 OK { token, expires }
   
3. Token Storage
   Frontend: localStorage.setItem('jwt-token', token)
   
4. Authenticated Requests
   Frontend → Backend: GET /api/protected
   Headers: { Authorization: "Bearer <token>" }
   
5. Token Validation
   Backend: Validate token signature and expiry
   Backend → Frontend: 200 OK { protected data }
```

## Project Structure

```
jwt-demo/
├── backend/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   └── ProtectedController.cs
│   ├── Services/
│   │   └── JwtService.cs
│   ├── Models/
│   │   └── LoginModel.cs
│   └── Program.cs
└── frontend/
    ├── index.html
    ├── login.html
    ├── js/
    │   ├── auth.js
    │   └── api.js
    └── css/
        └── style.css
```

## Next Steps

1. Initialize project structure
2. Implement backend JWT authentication
3. Create frontend authentication interface
4. Test complete authentication flow
5. Add comprehensive error handling