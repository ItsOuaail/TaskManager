import { useState } from 'react';
import { Link } from 'react-router-dom';
import axios from 'axios';
import { LogIn } from 'lucide-react';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5290/api';

function Login({ onLogin }) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await axios.post(`${API_URL}/auth/login`, {
        email,
        password
      });

      // Call onLogin with token and user data
      onLogin(response.data.token, {
        name: response.data.name,
        email: response.data.email,
        userId: response.data.userId
      });
    } catch (err) {
      setError(err.response?.data?.message || 'Login failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-500 to-purple-600 flex items-center justify-center p-4">
      <div className="bg-white rounded-lg shadow-2xl p-8 w-full max-w-md">
        <div className="flex items-center justify-center mb-8">
          <LogIn className="w-12 h-12 text-blue-600 mr-3" />
          <h1 className="text-3xl font-bold text-gray-800">Task Manager</h1>
        </div>

        <h2 className="text-2xl font-semibold text-gray-700 mb-6 text-center">
          Welcome Back
        </h2>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
              Email Address
            </label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none transition"
              placeholder="you@example.com"
              disabled={loading}
            />
          </div>

          <div>
            <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">
              Password
            </label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none transition"
              placeholder="••••••••"
              disabled={loading}
            />
          </div>

          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg text-sm">
              {error}
            </div>
          )}

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 transition font-medium disabled:bg-blue-300 disabled:cursor-not-allowed"
          >
            {loading ? 'Logging in...' : 'Log In'}
          </button>
        </form>

        <div className="mt-6 text-center">
          <p className="text-gray-600">
            Don't have an account?{' '}
            <Link to="/register" className="text-blue-600 hover:text-blue-700 font-medium">
              Sign up
            </Link>
          </p>
        </div>

        <div className="mt-6 bg-blue-50 border border-blue-200 rounded-lg p-4">
          <p className="text-sm text-blue-800 font-medium mb-2">Test Credentials:</p>
          <p className="text-sm text-blue-700">Email: admin@test.com</p>
          <p className="text-sm text-blue-700">Password: password123</p>
        </div>
      </div>
    </div>
  );
}

export default Login;

/*
 * LOGIN COMPONENT EXPLAINED:
 * 
 * 1. STATE MANAGEMENT:
 *    - email, password: Form inputs
 *    - error: Error message to display
 *    - loading: Shows loading state during API call
 * 
 * 2. FORM HANDLING:
 *    - e.preventDefault(): Stops page refresh
 *    - Controlled inputs: Value tied to state
 *    - onChange updates state on every keystroke
 * 
 * 3. API REQUEST:
 *    - axios.post sends POST request
 *    - API_URL + /auth/login = http://localhost:5290/api/auth/login
 *    - Sends email and password in request body
 *    - Response contains token and user data
 * 
 * 4. ERROR HANDLING:
 *    - try/catch for network errors
 *    - err.response?.data?.message gets error from API
 *    - Display error in red box
 * 
 * 5. TAILWIND CLASSES:
 *    - min-h-screen: Full viewport height
 *    - bg-gradient-to-br: Gradient background
 *    - flex items-center justify-center: Center content
 *    - rounded-lg: Rounded corners
 *    - shadow-2xl: Large shadow
 *    - focus:ring-2: Blue ring on focus
 * 
 * 6. ACCESSIBILITY:
 *    - htmlFor links label to input
 *    - disabled state for loading
 *    - Required fields marked
 */