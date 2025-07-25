// Mock localStorage for testing
global.localStorage = {
  store: {},
  getItem: function(key) {
    return this.store[key] || null;
  },
  setItem: function(key, value) {
    this.store[key] = value.toString();
  },
  removeItem: function(key) {
    delete this.store[key];
  },
  clear: function() {
    this.store = {};
  }
};

// Mock fetch for testing
global.fetch = jest.fn();

// Setup DOM
document.body.innerHTML = `
  <div id="message"></div>
  <div id="username"></div>
  <div id="tokenStatus"></div>
  <div id="tokenExpiry"></div>
`;

// Clear mocks before each test
beforeEach(() => {
  fetch.mockClear();
  localStorage.clear();
  
  // Reset DOM elements
  document.getElementById('message').textContent = '';
  document.getElementById('message').className = 'message';
});