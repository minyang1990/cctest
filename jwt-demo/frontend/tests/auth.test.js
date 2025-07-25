/**
 * @jest-environment jsdom
 */

// Import the AuthService (in a real setup, you'd use ES6 imports or require)
eval(require('fs').readFileSync('./js/auth.js', 'utf8'));

describe('AuthService', () => {
  beforeEach(() => {
    localStorage.clear();
    fetch.mockClear();
  });

  describe('login', () => {
    it('should login successfully with valid credentials', async () => {
      // Arrange
      const mockResponse = {
        success: true,
        data: {
          token: 'mock.jwt.token',
          username: 'testuser',
          expires: new Date(Date.now() + 3600000).toISOString() // 1 hour from now
        }
      };

      fetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResponse)
      });

      // Act
      const result = await AuthService.login('testuser', 'password123');

      // Assert
      expect(result).toBe(true);
      expect(localStorage.getItem('jwt-token')).toBe('mock.jwt.token');
      expect(localStorage.getItem('jwt-token-data')).toBeTruthy();
      
      const tokenData = JSON.parse(localStorage.getItem('jwt-token-data'));
      expect(tokenData.username).toBe('testuser');
      expect(tokenData.expires).toBe(mockResponse.data.expires);
    });

    it('should fail login with invalid credentials', async () => {
      // Arrange
      const mockResponse = {
        success: false,
        message: 'Invalid username or password'
      };

      fetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: () => Promise.resolve(mockResponse)
      });

      // Act & Assert
      await expect(AuthService.login('testuser', 'wrongpassword'))
        .rejects.toThrow('Invalid username or password');
      
      expect(localStorage.getItem('jwt-token')).toBeNull();
      expect(localStorage.getItem('jwt-token-data')).toBeNull();
    });

    it('should handle network errors', async () => {
      // Arrange
      fetch.mockRejectedValueOnce(new Error('Network error'));

      // Act & Assert
      await expect(AuthService.login('testuser', 'password123'))
        .rejects.toThrow('Network error');
    });

    it('should make correct API call', async () => {
      // Arrange
      const mockResponse = {
        success: true,
        data: {
          token: 'mock.jwt.token',
          username: 'testuser',
          expires: new Date().toISOString()
        }
      };

      fetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResponse)
      });

      // Act
      await AuthService.login('testuser', 'password123');

      // Assert
      expect(fetch).toHaveBeenCalledWith('http://localhost:5000/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          username: 'testuser',
          password: 'password123'
        })
      });
    });
  });

  describe('logout', () => {
    it('should clear stored tokens', () => {
      // Arrange
      localStorage.setItem('jwt-token', 'mock.token');
      localStorage.setItem('jwt-token-data', '{"username":"test"}');

      // Act
      AuthService.logout();

      // Assert
      expect(localStorage.getItem('jwt-token')).toBeNull();
      expect(localStorage.getItem('jwt-token-data')).toBeNull();
    });
  });

  describe('getToken', () => {
    it('should return stored token', () => {
      // Arrange
      localStorage.setItem('jwt-token', 'stored.token');

      // Act
      const token = AuthService.getToken();

      // Assert
      expect(token).toBe('stored.token');
    });

    it('should return null when no token stored', () => {
      // Act
      const token = AuthService.getToken();

      // Assert
      expect(token).toBeNull();
    });
  });

  describe('getTokenData', () => {
    it('should return parsed token data', () => {
      // Arrange
      const tokenData = {
        username: 'testuser',
        expires: new Date().toISOString(),
        loginTime: new Date().toISOString()
      };
      localStorage.setItem('jwt-token-data', JSON.stringify(tokenData));

      // Act
      const result = AuthService.getTokenData();

      // Assert
      expect(result).toEqual(tokenData);
    });

    it('should return null when no token data stored', () => {
      // Act
      const result = AuthService.getTokenData();

      // Assert
      expect(result).toBeNull();
    });

    it('should handle invalid JSON gracefully', () => {
      // Arrange
      localStorage.setItem('jwt-token-data', 'invalid-json');

      // Act & Assert
      expect(() => AuthService.getTokenData()).toThrow();
    });
  });

  describe('isAuthenticated', () => {
    it('should return true for valid non-expired token', () => {
      // Arrange
      const futureDate = new Date(Date.now() + 3600000).toISOString(); // 1 hour from now
      localStorage.setItem('jwt-token', 'valid.token');
      localStorage.setItem('jwt-token-data', JSON.stringify({
        username: 'testuser',
        expires: futureDate,
        loginTime: new Date().toISOString()
      }));

      // Act
      const isAuth = AuthService.isAuthenticated();

      // Assert
      expect(isAuth).toBe(true);
    });

    it('should return false for expired token', () => {
      // Arrange
      const pastDate = new Date(Date.now() - 3600000).toISOString(); // 1 hour ago
      localStorage.setItem('jwt-token', 'expired.token');
      localStorage.setItem('jwt-token-data', JSON.stringify({
        username: 'testuser',
        expires: pastDate,
        loginTime: new Date().toISOString()
      }));

      // Act
      const isAuth = AuthService.isAuthenticated();

      // Assert
      expect(isAuth).toBe(false);
      expect(localStorage.getItem('jwt-token')).toBeNull(); // Should clean up
      expect(localStorage.getItem('jwt-token-data')).toBeNull();
    });

    it('should return false when no token exists', () => {
      // Act
      const isAuth = AuthService.isAuthenticated();

      // Assert
      expect(isAuth).toBe(false);
    });

    it('should return false when token exists but no token data', () => {
      // Arrange
      localStorage.setItem('jwt-token', 'token.without.data');

      // Act
      const isAuth = AuthService.isAuthenticated();

      // Assert
      expect(isAuth).toBe(false);
    });
  });

  describe('getAuthHeaders', () => {
    it('should return authorization header with token', () => {
      // Arrange
      localStorage.setItem('jwt-token', 'test.jwt.token');

      // Act
      const headers = AuthService.getAuthHeaders();

      // Assert
      expect(headers).toEqual({
        'Authorization': 'Bearer test.jwt.token'
      });
    });

    it('should return empty object when no token', () => {
      // Act
      const headers = AuthService.getAuthHeaders();

      // Assert
      expect(headers).toEqual({});
    });
  });
});