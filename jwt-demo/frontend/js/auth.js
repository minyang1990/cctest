class AuthService {
    static TOKEN_KEY = 'jwt-token';
    static TOKEN_DATA_KEY = 'jwt-token-data';

    static async login(username, password) {
        try {
            const response = await fetch('http://localhost:5000/api/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ username, password })
            });

            const data = await response.json();
            
            if (data.success && data.data) {
                // Store token and token data
                localStorage.setItem(this.TOKEN_KEY, data.data.token);
                localStorage.setItem(this.TOKEN_DATA_KEY, JSON.stringify({
                    username: data.data.username,
                    expires: data.data.expires,
                    loginTime: new Date().toISOString()
                }));
                return true;
            } else {
                throw new Error(data.message || 'Login failed');
            }
        } catch (error) {
            console.error('Login error:', error);
            throw error;
        }
    }

    static logout() {
        localStorage.removeItem(this.TOKEN_KEY);
        localStorage.removeItem(this.TOKEN_DATA_KEY);
    }

    static getToken() {
        return localStorage.getItem(this.TOKEN_KEY);
    }

    static getTokenData() {
        const tokenData = localStorage.getItem(this.TOKEN_DATA_KEY);
        return tokenData ? JSON.parse(tokenData) : null;
    }

    static isAuthenticated() {
        const token = this.getToken();
        const tokenData = this.getTokenData();
        
        if (!token || !tokenData) {
            return false;
        }

        // Check if token is expired
        const now = new Date();
        const expiry = new Date(tokenData.expires);
        
        if (now > expiry) {
            this.logout(); // Clean up expired token
            return false;
        }

        return true;
    }

    static getAuthHeaders() {
        const token = this.getToken();
        return token ? { 'Authorization': `Bearer ${token}` } : {};
    }
}