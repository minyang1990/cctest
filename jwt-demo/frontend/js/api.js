class ApiService {
    static BASE_URL = 'http://localhost:5000/api';

    static async request(endpoint, options = {}) {
        const url = `${this.BASE_URL}${endpoint}`;
        const headers = {
            'Content-Type': 'application/json',
            ...AuthService.getAuthHeaders(),
            ...options.headers
        };

        try {
            const response = await fetch(url, {
                ...options,
                headers
            });

            // Handle different response types
            let data;
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                data = await response.json();
            } else {
                data = { message: await response.text() };
            }

            if (!response.ok) {
                // Handle authentication errors
                if (response.status === 401) {
                    AuthService.logout();
                    window.location.href = 'login.html';
                    throw new Error('Authentication required');
                }
                
                throw new Error(data.message || `HTTP ${response.status}: ${response.statusText}`);
            }

            return data;
        } catch (error) {
            console.error('API request error:', error);
            throw error;
        }
    }

    static async get(endpoint, options = {}) {
        return this.request(endpoint, {
            method: 'GET',
            ...options
        });
    }

    static async post(endpoint, body, options = {}) {
        return this.request(endpoint, {
            method: 'POST',
            body: JSON.stringify(body),
            ...options
        });
    }

    static async put(endpoint, body, options = {}) {
        return this.request(endpoint, {
            method: 'PUT',
            body: JSON.stringify(body),
            ...options
        });
    }

    static async delete(endpoint, options = {}) {
        return this.request(endpoint, {
            method: 'DELETE',
            ...options
        });
    }
}

// Utility functions for common API operations
class ApiUtils {
    static handleError(error, fallbackMessage = 'An error occurred') {
        console.error('API Error:', error);
        
        if (error.message.includes('fetch')) {
            return 'Network error. Please check if the server is running.';
        }
        
        return error.message || fallbackMessage;
    }

    static showLoading(elementId, isLoading = true) {
        const element = document.getElementById(elementId);
        if (element) {
            element.disabled = isLoading;
            if (element.tagName === 'BUTTON') {
                if (isLoading) {
                    element.dataset.originalText = element.textContent;
                    element.textContent = 'Loading...';
                } else {
                    element.textContent = element.dataset.originalText || element.textContent;
                }
            }
        }
    }

    static formatDateTime(dateString) {
        return new Date(dateString).toLocaleString();
    }

    static formatJSON(obj) {
        return JSON.stringify(obj, null, 2);
    }
}