/**
 * @jest-environment jsdom
 */

// End-to-end frontend integration tests
eval(require('fs').readFileSync('./js/auth.js', 'utf8'));
eval(require('fs').readFileSync('./js/api.js', 'utf8'));

describe('JWT Authentication Integration Tests', () => {
  let mockServer;

  beforeEach(() => {
    localStorage.clear();
    fetch.mockClear();
    
    // Mock window.location for redirect tests
    delete window.location;
    window.location = { href: '' };
  });

  describe('Complete Authentication Flow', () => {
    it('should complete full login to protected data flow', async () => {
      // Step 1: Mock successful login
      const loginResponse = {
        success: true,
        data: {
          token: 'eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ0ZXN0dXNlciIsImlhdCI6MTYzMzA0MjQ0MCwiZXhwIjoxNjMzMDQ2MDQwfQ.test',
          username: 'testuser',
          expires: new Date(Date.now() + 3600000).toISOString()
        }
      };

      fetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(loginResponse)
      });

      // Step 2: Mock successful protected data request
      const protectedResponse = {
        success: true,
        message: 'Protected data accessed successfully',
        data: {
          accessedBy: 'testuser',
          accessTime: new Date().toISOString(),
          secretData: 'This is protected information'
        }
      };

      fetch.mockResolvedValueOnce({
        ok: true,
        headers: { get: () => 'application/json' },
        json: () => Promise.resolve(protectedResponse)
      });

      // Execute flow
      const loginSuccess = await AuthService.login('testuser', 'password123');
      expect(loginSuccess).toBe(true);
      expect(AuthService.isAuthenticated()).toBe(true);

      const protectedData = await ApiService.get('/protected/data');
      expect(protectedData.success).toBe(true);
      expect(protectedData.data.accessedBy).toBe('testuser');

      // Verify API calls
      expect(fetch).toHaveBeenCalledTimes(2);
      expect(fetch).toHaveBeenNthCalledWith(1, 'http://localhost:5000/api/auth/login', expect.any(Object));
      expect(fetch).toHaveBeenNthCalledWith(2, 'http://localhost:5000/api/protected/data', expect.objectContaining({
        headers: expect.objectContaining({
          'Authorization': expect.stringContaining('Bearer')
        })
      }));
    });

    it('should handle token expiry during protected request', async () => {
      // Setup expired token
      const pastDate = new Date(Date.now() - 3600000).toISOString();
      localStorage.setItem('jwt-token', 'expired.token');
      localStorage.setItem('jwt-token-data', JSON.stringify({
        username: 'testuser',
        expires: pastDate,
        loginTime: new Date().toISOString()
      }));

      // Mock 401 response
      fetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        headers: { get: () => 'application/json' },
        json: () => Promise.resolve({ message: 'Token expired' })
      });

      // Execute request
      await expect(ApiService.get('/protected/data'))
        .rejects.toThrow('Authentication required');

      // Verify cleanup and redirect
      expect(localStorage.getItem('jwt-token')).toBeNull();
      expect(window.location.href).toBe('login.html');
    });

    it('should maintain authentication state across page refreshes', () => {
      // Setup valid token
      const futureDate = new Date(Date.now() + 3600000).toISOString();
      localStorage.setItem('jwt-token', 'valid.token');
      localStorage.setItem('jwt-token-data', JSON.stringify({
        username: 'testuser',
        expires: futureDate,
        loginTime: new Date().toISOString()
      }));

      // Simulate page refresh by creating new service instance
      expect(AuthService.isAuthenticated()).toBe(true);
      expect(AuthService.getToken()).toBe('valid.token');
      
      const tokenData = AuthService.getTokenData();
      expect(tokenData.username).toBe('testuser');
    });
  });

  describe('Error Handling Integration', () => {
    it('should handle network errors gracefully', async () => {
      fetch.mockRejectedValueOnce(new Error('Network error'));

      await expect(AuthService.login('user', 'pass'))
        .rejects.toThrow('Network error');

      expect(localStorage.getItem('jwt-token')).toBeNull();
    });

    it('should handle malformed API responses', async () => {
      fetch.mockResolvedValueOnce({
        ok: true,
        headers: { get: () => 'application/json' },
        json: () => Promise.reject(new Error('Invalid JSON'))
      });

      await expect(ApiService.get('/test'))
        .rejects.toThrow('Invalid JSON');
    });

    it('should handle server errors with proper error messages', async () => {
      const errorResponse = {
        success: false,
        message: 'Internal server error occurred'
      };

      fetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        statusText: 'Internal Server Error',
        headers: { get: () => 'application/json' },
        json: () => Promise.resolve(errorResponse)
      });

      await expect(ApiService.get('/test'))
        .rejects.toThrow('Internal server error occurred');
    });
  });

  describe('Security Integration', () => {
    it('should not send tokens in non-authenticated requests', async () => {
      // Ensure no token is stored
      localStorage.clear();

      fetch.mockResolvedValueOnce({
        ok: true,
        headers: { get: () => 'application/json' },
        json: () => Promise.resolve({ data: 'public data' })
      });

      await ApiService.get('/public/data');

      expect(fetch).toHaveBeenCalledWith(
        'http://localhost:5000/api/public/data',
        expect.objectContaining({
          headers: expect.not.objectContaining({
            'Authorization': expect.anything()
          })
        })
      );
    });

    it('should automatically include auth headers when token exists', async () => {
      localStorage.setItem('jwt-token', 'test.token');

      fetch.mockResolvedValueOnce({
        ok: true,
        headers: { get: () => 'application/json' },
        json: () => Promise.resolve({ data: 'protected data' })
      });

      await ApiService.get('/protected/data');

      expect(fetch).toHaveBeenCalledWith(
        'http://localhost:5000/api/protected/data',
        expect.objectContaining({
          headers: expect.objectContaining({
            'Authorization': 'Bearer test.token'
          })
        })
      );
    });

    it('should handle logout and cleanup properly', () => {
      // Setup authenticated state
      localStorage.setItem('jwt-token', 'test.token');
      localStorage.setItem('jwt-token-data', JSON.stringify({
        username: 'testuser',
        expires: new Date(Date.now() + 3600000).toISOString()
      }));

      expect(AuthService.isAuthenticated()).toBe(true);

      // Logout
      AuthService.logout();

      // Verify cleanup
      expect(AuthService.isAuthenticated()).toBe(false);
      expect(AuthService.getToken()).toBeNull();
      expect(AuthService.getTokenData()).toBeNull();
      expect(localStorage.getItem('jwt-token')).toBeNull();
      expect(localStorage.getItem('jwt-token-data')).toBeNull();
    });
  });

  describe('API Endpoint Integration', () => {
    beforeEach(async () => {
      // Setup authenticated state for protected endpoint tests
      localStorage.setItem('jwt-token', 'valid.token');
      localStorage.setItem('jwt-token-data', JSON.stringify({
        username: 'testuser',
        expires: new Date(Date.now() + 3600000).toISOString()
      }));
    });

    it('should handle all CRUD operations with proper headers', async () => {
      const testData = { id: 1, name: 'test' };
      
      // Mock responses for different HTTP methods
      const mockResponse = { success: true, data: testData };
      fetch.mockResolvedValue({
        ok: true,
        headers: { get: () => 'application/json' },
        json: () => Promise.resolve(mockResponse)
      });

      // Test all HTTP methods
      await ApiService.get('/data/1');
      await ApiService.post('/data', testData);
      await ApiService.put('/data/1', testData);
      await ApiService.delete('/data/1');

      // Verify all calls included auth headers
      expect(fetch).toHaveBeenCalledTimes(4);
      
      for (let i = 0; i < 4; i++) {
        const call = fetch.mock.calls[i];
        expect(call[1].headers).toHaveProperty('Authorization', 'Bearer valid.token');
      }
    });
  });
});