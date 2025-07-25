/**
 * @jest-environment jsdom
 */

// Import the services (in a real setup, you'd use ES6 imports)
eval(require('fs').readFileSync('./js/auth.js', 'utf8'));
eval(require('fs').readFileSync('./js/api.js', 'utf8'));

describe('ApiService', () => {
  beforeEach(() => {
    localStorage.clear();
    fetch.mockClear();
    
    // Mock window.location for redirect tests
    delete window.location;
    window.location = { href: '' };
  });

  describe('request', () => {
    it('should make successful API request with authentication headers', async () => {
      // Arrange
      localStorage.setItem('jwt-token', 'test.jwt.token');
      const mockResponse = { success: true, data: 'test data' };
      
      fetch.mockResolvedValueOnce({
        ok: true,
        headers: {
          get: () => 'application/json'
        },
        json: () => Promise.resolve(mockResponse)
      });

      // Act
      const result = await ApiService.request('/test/endpoint');

      // Assert
      expect(fetch).toHaveBeenCalledWith('http://localhost:5000/api/test/endpoint', {
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer test.jwt.token'
        }
      });
      expect(result).toEqual(mockResponse);
    });

    it('should handle non-JSON responses', async () => {
      // Arrange
      fetch.mockResolvedValueOnce({
        ok: true,
        headers: {
          get: () => 'text/plain'
        },
        text: () => Promise.resolve('Plain text response')
      });

      // Act
      const result = await ApiService.request('/test/endpoint');

      // Assert
      expect(result).toEqual({ message: 'Plain text response' });
    });

    it('should handle 401 unauthorized by redirecting to login', async () => {
      // Arrange
      localStorage.setItem('jwt-token', 'expired.token');
      fetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        headers: {
          get: () => 'application/json'
        },
        json: () => Promise.resolve({ message: 'Unauthorized' })
      });

      // Act & Assert
      await expect(ApiService.request('/protected/endpoint'))
        .rejects.toThrow('Authentication required');
      
      expect(localStorage.getItem('jwt-token')).toBeNull();
      expect(window.location.href).toBe('login.html');
    });

    it('should handle other HTTP errors', async () => {
      // Arrange
      fetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        statusText: 'Internal Server Error',
        headers: {
          get: () => 'application/json'
        },
        json: () => Promise.resolve({ message: 'Server error' })
      });

      // Act & Assert
      await expect(ApiService.request('/test/endpoint'))
        .rejects.toThrow('Server error');
    });

    it('should handle network errors', async () => {
      // Arrange
      fetch.mockRejectedValueOnce(new Error('Network error'));

      // Act & Assert
      await expect(ApiService.request('/test/endpoint'))
        .rejects.toThrow('Network error');
    });
  });

  describe('get', () => {
    it('should make GET request', async () => {
      // Arrange
      const mockResponse = { data: 'test' };
      fetch.mockResolvedValueOnce({
        ok: true,
        headers: { get: () => 'application/json' },
        json: () => Promise.resolve(mockResponse)
      });

      // Act
      await ApiService.get('/test');

      // Assert
      expect(fetch).toHaveBeenCalledWith('http://localhost:5000/api/test', {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json'
        }
      });
    });
  });

  describe('post', () => {
    it('should make POST request with body', async () => {
      // Arrange
      const testData = { username: 'test', password: 'pass' };
      const mockResponse = { success: true };
      fetch.mockResolvedValueOnce({
        ok: true,
        headers: { get: () => 'application/json' },
        json: () => Promise.resolve(mockResponse)
      });

      // Act
      await ApiService.post('/auth/login', testData);

      // Assert
      expect(fetch).toHaveBeenCalledWith('http://localhost:5000/api/auth/login', {
        method: 'POST',
        body: JSON.stringify(testData),
        headers: {
          'Content-Type': 'application/json'
        }
      });
    });
  });

  describe('put', () => {
    it('should make PUT request with body', async () => {
      // Arrange
      const testData = { id: 1, name: 'updated' };
      const mockResponse = { success: true };
      fetch.mockResolvedValueOnce({
        ok: true,
        headers: { get: () => 'application/json' },
        json: () => Promise.resolve(mockResponse)
      });

      // Act
      await ApiService.put('/data/1', testData);

      // Assert
      expect(fetch).toHaveBeenCalledWith('http://localhost:5000/api/data/1', {
        method: 'PUT',
        body: JSON.stringify(testData),
        headers: {
          'Content-Type': 'application/json'
        }
      });
    });
  });

  describe('delete', () => {
    it('should make DELETE request', async () => {
      // Arrange
      const mockResponse = { success: true };
      fetch.mockResolvedValueOnce({
        ok: true,
        headers: { get: () => 'application/json' },
        json: () => Promise.resolve(mockResponse)
      });

      // Act
      await ApiService.delete('/data/1');

      // Assert
      expect(fetch).toHaveBeenCalledWith('http://localhost:5000/api/data/1', {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json'
        }
      });
    });
  });
});

describe('ApiUtils', () => {
  beforeEach(() => {
    // Setup DOM elements for testing
    document.body.innerHTML = `
      <button id="testButton">Test Button</button>
      <input id="testInput" value="test">
    `;
  });

  describe('handleError', () => {
    it('should return network error message for fetch errors', () => {
      // Arrange
      const fetchError = new Error('fetch is not defined');

      // Act
      const message = ApiUtils.handleError(fetchError);

      // Assert
      expect(message).toBe('Network error. Please check if the server is running.');
    });

    it('should return error message for regular errors', () => {
      // Arrange
      const error = new Error('Custom error message');

      // Act
      const message = ApiUtils.handleError(error);

      // Assert
      expect(message).toBe('Custom error message');
    });

    it('should return fallback message for errors without message', () => {
      // Arrange
      const error = new Error();
      error.message = '';

      // Act
      const message = ApiUtils.handleError(error, 'Fallback message');

      // Assert
      expect(message).toBe('Fallback message');
    });
  });

  describe('showLoading', () => {
    it('should disable button and show loading text', () => {
      // Arrange
      const button = document.getElementById('testButton');

      // Act
      ApiUtils.showLoading('testButton', true);

      // Assert
      expect(button.disabled).toBe(true);
      expect(button.textContent).toBe('Loading...');
      expect(button.dataset.originalText).toBe('Test Button');
    });

    it('should enable button and restore original text', () => {
      // Arrange
      const button = document.getElementById('testButton');
      button.dataset.originalText = 'Original Text';
      button.disabled = true;
      button.textContent = 'Loading...';

      // Act
      ApiUtils.showLoading('testButton', false);

      // Assert
      expect(button.disabled).toBe(false);
      expect(button.textContent).toBe('Original Text');
    });

    it('should handle non-existent elements gracefully', () => {
      // Act & Assert (should not throw)
      expect(() => ApiUtils.showLoading('nonExistentElement', true)).not.toThrow();
    });

    it('should handle non-button elements gracefully', () => {
      // Act & Assert (should not throw)
      expect(() => ApiUtils.showLoading('testInput', true)).not.toThrow();
    });
  });

  describe('formatDateTime', () => {
    it('should format date string correctly', () => {
      // Arrange
      const dateString = '2023-12-25T10:30:00Z';

      // Act
      const formatted = ApiUtils.formatDateTime(dateString);

      // Assert
      expect(formatted).toBe(new Date(dateString).toLocaleString());
    });
  });

  describe('formatJSON', () => {
    it('should format object as pretty JSON', () => {
      // Arrange
      const obj = { name: 'test', value: 42 };

      // Act
      const formatted = ApiUtils.formatJSON(obj);

      // Assert
      expect(formatted).toBe(JSON.stringify(obj, null, 2));
    });
  });
});