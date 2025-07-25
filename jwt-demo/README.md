# JWT Demo - Setup & Running Instructions

## Backend Setup (.NET Core)

1. **Navigate to backend directory:**
   ```bash
   cd jwt-demo/backend
   ```

2. **Restore packages:**
   ```bash
   dotnet restore
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```
   
   The API will be available at: `http://localhost:5000`

## Frontend Setup

1. **Navigate to frontend directory:**
   ```bash
   cd jwt-demo/frontend
   ```

2. **Serve the frontend (using Python):**
   ```bash
   # Python 3
   python -m http.server 3000
   
   # Or Python 2
   python -m SimpleHTTPServer 3000
   ```
   
   The frontend will be available at: `http://localhost:3000`

3. **Alternative: Using Node.js serve:**
   ```bash
   npx serve -p 3000
   ```

## Demo Flow

1. **Start the backend** (runs on port 5000)
2. **Start the frontend** (runs on port 3000)
3. **Open browser** to `http://localhost:3000/login.html`
4. **Login with demo credentials:**
   - admin / password123
   - user / userpass
   - demo / demo123

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login and get JWT token
- `POST /api/auth/validate` - Validate current token

### Protected Endpoints (Require JWT)
- `GET /api/protected/profile` - Get user profile
- `GET /api/protected/data` - Get protected data
- `POST /api/protected/action` - Perform protected action

## Features Demonstrated

1. **JWT Token Generation** - Server creates signed tokens
2. **Token Storage** - Frontend stores tokens in localStorage
3. **Automatic Headers** - API service adds Authorization header
4. **Token Validation** - Server validates token on protected routes
5. **Token Expiry** - Tokens expire after 60 minutes
6. **Error Handling** - Proper error messages and redirects
7. **CORS Configuration** - Cross-origin requests enabled

## Security Notes

- This is a demo implementation
- In production:
  - Use HTTPS everywhere
  - Store tokens in httpOnly cookies
  - Implement refresh token mechanism
  - Use environment variables for secrets
  - Add rate limiting
  - Implement proper user management