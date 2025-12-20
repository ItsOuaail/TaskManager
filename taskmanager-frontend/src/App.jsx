import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import Login from './components/Login';
import Register from './components/Register';
import Dashboard from './components/Dashboard';
import ProjectDetail from './components/ProjectDetail';

function App() {
  const [token, setToken] = useState(localStorage.getItem('token'));
  const [user, setUser] = useState(() => {
    const savedUser = localStorage.getItem('user');
    return savedUser ? JSON.parse(savedUser) : null;
  });

  // Save token and user to localStorage when they change
  useEffect(() => {
    if (token && user) {
      localStorage.setItem('token', token);
      localStorage.setItem('user', JSON.stringify(user));
    }
  }, [token, user]);

  const handleLogin = (authToken, userData) => {
    setToken(authToken);
    setUser(userData);
  };

  const handleLogout = () => {
    setToken(null);
    setUser(null);
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  };

  return (
    <BrowserRouter>
      <Routes>
        <Route 
          path="/login" 
          element={
            token ? <Navigate to="/dashboard" /> : <Login onLogin={handleLogin} />
          } 
        />
        <Route 
          path="/register" 
          element={
            token ? <Navigate to="/dashboard" /> : <Register onLogin={handleLogin} />
          } 
        />
        <Route 
          path="/dashboard" 
          element={
            token ? <Dashboard token={token} user={user} onLogout={handleLogout} /> : <Navigate to="/login" />
          } 
        />
        <Route 
          path="/projects/:id" 
          element={
            token ? <ProjectDetail token={token} onLogout={handleLogout} /> : <Navigate to="/login" />
          } 
        />
        <Route path="/" element={<Navigate to="/dashboard" />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;

/*
 * REACT APP STRUCTURE EXPLAINED:
 * 
 * 1. ROUTING:
 *    - BrowserRouter: Enables client-side routing
 *    - Routes: Container for all route definitions
 *    - Route: Defines a path and component to render
 *    - Navigate: Redirects to another route
 * 
 * 2. AUTHENTICATION STATE:
 *    - token: JWT token for API requests
 *    - user: User information (name, email)
 *    - Both stored in state AND localStorage
 * 
 * 3. PROTECTED ROUTES:
 *    - If not logged in, redirect to /login
 *    - If logged in, can access /dashboard and /projects
 * 
 * 4. PROPS:
 *    - onLogin: Function to update auth state
 *    - onLogout: Function to clear auth state
 *    - token: Passed to components that need to call API
 * 
 * 5. localStorage:
 *    - Persists auth data across page refreshes
 *    - localStorage.setItem('key', value)
 *    - localStorage.getItem('key')
 *    - localStorage.removeItem('key')
 */