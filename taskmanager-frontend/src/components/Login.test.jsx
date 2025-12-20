/**
 * FRONTEND UNIT TESTS TUTORIAL
 * 
 * This file tests the Login component
 * Read comments to understand each part
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import Login from './Login';
import axios from 'axios';

// Mock axios (fake HTTP requests)
vi.mock('axios');

/**
 * TEST SUITE: Login Component
 * 
 * describe() groups related tests together
 * Like a folder for tests about the same component
 */
describe('Login Component', () => {
  
  // Mock function for onLogin callback
  let mockOnLogin;
  
  /**
   * beforeEach runs before EACH test
   * Use it to set up fresh mocks
   */
  beforeEach(() => {
    mockOnLogin = vi.fn();  // Create a spy function
    vi.clearAllMocks();     // Clear all mocks before each test
  });
  
  /**
   * HELPER FUNCTION: Render Login component
   * 
   * Why BrowserRouter?
   * - Login uses <Link> from react-router-dom
   * - <Link> only works inside a router
   */
  const renderLogin = () => {
    return render(
      <BrowserRouter>
        <Login onLogin={mockOnLogin} />
      </BrowserRouter>
    );
  };
  
  // ═══════════════════════════════════════════════════════
  // TEST 1: Component Renders
  // ═══════════════════════════════════════════════════════
  
  it('should render login form', () => {
    // ARRANGE - Render the component
    renderLogin();
    
    // ASSERT - Check elements exist
    expect(screen.getByText('Task Manager')).toBeInTheDocument();
    expect(screen.getByText('Welcome Back')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('you@example.com')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('••••••••')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /log in/i })).toBeInTheDocument();
    
    // ✅ If all elements are found, test passes
  });
  
  // ═══════════════════════════════════════════════════════
  // TEST 2: Form Validation
  // ═══════════════════════════════════════════════════════
  
  it('should show email and password inputs', () => {
    renderLogin();
    
    // Get input elements
    const emailInput = screen.getByPlaceholderText('you@example.com');
    const passwordInput = screen.getByPlaceholderText('••••••••');
    
    // Check inputs have correct type
    expect(emailInput).toHaveAttribute('type', 'email');
    expect(passwordInput).toHaveAttribute('type', 'password');
    
    // Check inputs are required
    expect(emailInput).toBeRequired();
    expect(passwordInput).toBeRequired();
  });
  
  // ═══════════════════════════════════════════════════════
  // TEST 3: User Can Type in Inputs
  // ═══════════════════════════════════════════════════════
  
  it('should allow user to type in inputs', async () => {
    renderLogin();
    
    // Get inputs
    const emailInput = screen.getByPlaceholderText('you@example.com');
    const passwordInput = screen.getByPlaceholderText('••••••••');
    
    // ACT - Simulate user typing
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'password123' } });
    
    // ASSERT - Check values were updated
    expect(emailInput.value).toBe('test@example.com');
    expect(passwordInput.value).toBe('password123');
  });
  
  // ═══════════════════════════════════════════════════════
  // TEST 4: Successful Login
  // ═══════════════════════════════════════════════════════
  
  it('should call onLogin when login succeeds', async () => {
    // ARRANGE - Mock successful API response
    const mockResponse = {
      data: {
        token: 'fake-jwt-token',
        email: 'test@example.com',
        name: 'Test User',
        userId: '123'
      }
    };
    
    axios.post.mockResolvedValueOnce(mockResponse);
    // This says: "When axios.post is called, return mockResponse"
    
    renderLogin();
    
    // Get form elements
    const emailInput = screen.getByPlaceholderText('you@example.com');
    const passwordInput = screen.getByPlaceholderText('••••••••');
    const submitButton = screen.getByRole('button', { name: /log in/i });
    
    // ACT - Fill form and submit
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'password123' } });
    fireEvent.click(submitButton);
    
    // ASSERT - Wait for async operations
    await waitFor(() => {
      // Check axios.post was called with correct data
      expect(axios.post).toHaveBeenCalledWith(
        'http://localhost:5290/api/auth/login',
        {
          email: 'test@example.com',
          password: 'password123'
        }
      );
      
      // Check onLogin was called with token and user data
      expect(mockOnLogin).toHaveBeenCalledWith(
        'fake-jwt-token',
        {
          name: 'Test User',
          email: 'test@example.com',
          userId: '123'
        }
      );
    });
  });
  
  // ═══════════════════════════════════════════════════════
  // TEST 5: Login Failure - Shows Error Message
  // ═══════════════════════════════════════════════════════
  
  it('should show error message when login fails', async () => {
    // ARRANGE - Mock API error
    const mockError = {
      response: {
        data: {
          message: 'Invalid email or password'
        }
      }
    };
    
    axios.post.mockRejectedValueOnce(mockError);
    // This says: "When axios.post is called, throw this error"
    
    renderLogin();
    
    // ACT - Fill form and submit
    const emailInput = screen.getByPlaceholderText('you@example.com');
    const passwordInput = screen.getByPlaceholderText('••••••••');
    const submitButton = screen.getByRole('button', { name: /log in/i });
    
    fireEvent.change(emailInput, { target: { value: 'wrong@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'wrongpass' } });
    fireEvent.click(submitButton);
    
    // ASSERT - Wait for error to appear
    await waitFor(() => {
      expect(screen.getByText('Invalid email or password')).toBeInTheDocument();
    });
    
    // onLogin should NOT be called on error
    expect(mockOnLogin).not.toHaveBeenCalled();
  });
  
  // ═══════════════════════════════════════════════════════
  // TEST 6: Loading State
  // ═══════════════════════════════════════════════════════
  
  it('should show loading state during login', async () => {
    // ARRANGE - Mock slow API (delayed response)
    axios.post.mockImplementation(() => 
      new Promise(resolve => setTimeout(resolve, 100))
    );
    
    renderLogin();
    
    // ACT - Submit form
    const emailInput = screen.getByPlaceholderText('you@example.com');
    const passwordInput = screen.getByPlaceholderText('••••••••');
    const submitButton = screen.getByRole('button', { name: /log in/i });
    
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } });
    fireEvent.change(passwordInput, { target: { value: 'password123' } });
    fireEvent.click(submitButton);
    
    // ASSERT - Button should show loading text
    expect(screen.getByText('Logging in...')).toBeInTheDocument();
    expect(submitButton).toBeDisabled();
  });
});

/*
 * ═══════════════════════════════════════════════════════════
 * KEY CONCEPTS EXPLAINED
 * ═══════════════════════════════════════════════════════════
 * 
 * 1. describe()
 *    - Groups related tests
 *    - Organizes test file
 *    - Can be nested
 * 
 * 2. it() or test()
 *    - Defines a single test
 *    - it('should do something', () => { ... })
 *    - Same as [Fact] in xUnit
 * 
 * 3. render()
 *    - Renders a React component for testing
 *    - Returns utilities for querying elements
 *    - From @testing-library/react
 * 
 * 4. screen
 *    - Object with query methods
 *    - screen.getByText('Hello') = find element with text "Hello"
 *    - screen.getByRole('button') = find button element
 * 
 * 5. fireEvent
 *    - Simulates user interactions
 *    - fireEvent.click(button) = click button
 *    - fireEvent.change(input, { target: { value: 'text' } }) = type in input
 * 
 * 6. waitFor()
 *    - Waits for async operations
 *    - Used for API calls, state updates
 *    - Retries until condition is true or timeout
 * 
 * 7. vi.mock()
 *    - Mocks entire module (like axios)
 *    - All axios methods become fake
 *    - We control what they return
 * 
 * 8. vi.fn()
 *    - Creates a spy function
 *    - Tracks when it's called and with what arguments
 *    - Like Moq's Mock<T> in .NET
 * 
 * 9. expect().toBeInTheDocument()
 *    - Checks element exists in rendered output
 *    - From @testing-library/jest-dom
 * 
 * 10. expect().toHaveBeenCalledWith()
 *     - Checks function was called with specific arguments
 *     - Like .Verify() in Moq
 * 
 * ═══════════════════════════════════════════════════════════
 * COMMON QUERIES
 * ═══════════════════════════════════════════════════════════
 * 
 * getByText('Hello')           - Element with text "Hello"
 * getByPlaceholderText('...')  - Input with placeholder
 * getByRole('button')          - Button element
 * getByLabelText('Email')      - Input with label "Email"
 * getByTestId('my-element')    - Element with data-testid="my-element"
 * 
 * queryBy...  - Returns null if not found (use for "should not exist")
 * findBy...   - Async version, waits for element
 * 
 * ═══════════════════════════════════════════════════════════
 * RUNNING TESTS
 * ═══════════════════════════════════════════════════════════
 * 
 * npm test                 - Run in watch mode
 * npm run test:run         - Run once
 * npm run test:ui          - Run with UI
 * 
 * ═══════════════════════════════════════════════════════════
 */