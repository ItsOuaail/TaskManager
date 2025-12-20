/**
 * DASHBOARD COMPONENT TESTS
 * 
 * Tests more complex scenarios:
 * - API mocking
 * - List rendering
 * - User interactions
 * - Modal dialogs
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import Dashboard from './Dashboard';
import axios from 'axios';

vi.mock('axios');

describe('Dashboard Component', () => {
  const mockToken = 'fake-token';
  const mockUser = { name: 'Test User', email: 'test@example.com' };
  const mockOnLogout = vi.fn();
  
  // Mock project data
  const mockProjects = {
    items: [
      {
        id: '1',
        title: 'Project 1',
        description: 'Description 1',
        totalTasks: 5,
        completedTasks: 3,
        progressPercentage: 60
      },
      {
        id: '2',
        title: 'Project 2',
        description: 'Description 2',
        totalTasks: 10,
        completedTasks: 10,
        progressPercentage: 100
      }
    ],
    totalCount: 2,
    pageNumber: 1,
    pageSize: 6,
    totalPages: 1
  };
  
  beforeEach(() => {
    vi.clearAllMocks();
  });
  
  const renderDashboard = () => {
    return render(
      <BrowserRouter>
        <Dashboard token={mockToken} user={mockUser} onLogout={mockOnLogout} />
      </BrowserRouter>
    );
  };
  
  // ═══════════════════════════════════════════════════════
  // TEST 1: Loads and Displays Projects
  // ═══════════════════════════════════════════════════════
  
  it('should load and display projects', async () => {
    // ARRANGE - Mock API to return projects
    axios.get.mockResolvedValueOnce({ data: mockProjects });
    
    // ACT - Render component
    renderDashboard();
    
    // Component should show loading state first
    expect(screen.getByRole('status')).toBeInTheDocument();
    
    // ASSERT - Wait for projects to appear
    await waitFor(() => {
      expect(screen.getByText('Project 1')).toBeInTheDocument();
      expect(screen.getByText('Project 2')).toBeInTheDocument();
    });
    
    // Check API was called correctly
    expect(axios.get).toHaveBeenCalledWith(
      'http://localhost:5290/api/projects',
      {
        headers: { Authorization: `Bearer ${mockToken}` },
        params: { search: '', page: 1, pageSize: 6 }
      }
    );
  });
  
  // ═══════════════════════════════════════════════════════
  // TEST 2: Shows Empty State When No Projects
  // ═══════════════════════════════════════════════════════
  
  it('should show empty state when no projects exist', async () => {
    // ARRANGE - Mock API to return empty array
    axios.get.mockResolvedValueOnce({
      data: {
        items: [],
        totalCount: 0,
        pageNumber: 1,
        pageSize: 6,
        totalPages: 0
      }
    });
    
    renderDashboard();
    
    // ASSERT - Wait for empty message
    await waitFor(() => {
      expect(screen.getByText('No projects yet')).toBeInTheDocument();
      expect(screen.getByText('Create your first project to get started')).toBeInTheDocument();
    });
  });
  
  // ═══════════════════════════════════════════════════════
  // TEST 3: Search Functionality
  // ═══════════════════════════════════════════════════════
  
  it('should search projects when user types', async () => {
    // ARRANGE - Mock initial load
    axios.get.mockResolvedValueOnce({ data: mockProjects });
    
    renderDashboard();
    
    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByText('Project 1')).toBeInTheDocument();
    });
    
    // ACT - Type in search box
    const searchInput = screen.getByPlaceholderText('Search projects...');
    
    // Mock search results
    axios.get.mockResolvedValueOnce({
      data: {
        items: [mockProjects.items[0]],  // Only first project
        totalCount: 1,
        pageNumber: 1,
        pageSize: 6,
        totalPages: 1
      }
    });
    
    fireEvent.change(searchInput, { target: { value: 'Project 1' } });
    
    // ASSERT - Check API was called with search term
    await waitFor(() => {
      expect(axios.get).toHaveBeenCalledWith(
        'http://localhost:5290/api/projects',
        expect.objectContaining({
          params: expect.objectContaining({
            search: 'Project 1'
          })
        })
      );
    });
  });
  
  // ═══════════════════════════════════════════════════════
  // TEST 4: Create Project Modal
  // ═══════════════════════════════════════════════════════
  
  it('should open create project modal when button clicked', async () => {
    axios.get.mockResolvedValueOnce({ data: mockProjects });
    
    renderDashboard();
    
    await waitFor(() => {
      expect(screen.getByText('Project 1')).toBeInTheDocument();
    });
    
    // ACT - Click "New Project" button
    const newProjectButton = screen.getByRole('button', { name: /new project/i });
    fireEvent.click(newProjectButton);
    
    // ASSERT - Modal should appear
    expect(screen.getByText('Create New Project')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('Enter project title')).toBeInTheDocument();
  });
  
  // ═══════════════════════════════════════════════════════
  // TEST 5: Create Project Successfully
  // ═══════════════════════════════════════════════════════
  
  it('should create project when form submitted', async () => {
    // ARRANGE - Mock project list load
    axios.get.mockResolvedValueOnce({ data: mockProjects });
    
    renderDashboard();
    
    await waitFor(() => {
      expect(screen.getByText('Project 1')).toBeInTheDocument();
    });
    
    // Open modal
    const newProjectButton = screen.getByRole('button', { name: /new project/i });
    fireEvent.click(newProjectButton);
    
    // ACT - Fill form
    const titleInput = screen.getByPlaceholderText('Enter project title');
    const descInput = screen.getByPlaceholderText(/enter project description/i);
    
    fireEvent.change(titleInput, { target: { value: 'New Project' } });
    fireEvent.change(descInput, { target: { value: 'New Description' } });
    
    // Mock create API response
    const newProject = {
      id: '3',
      title: 'New Project',
      description: 'New Description',
      totalTasks: 0,
      completedTasks: 0,
      progressPercentage: 0
    };
    
    axios.post.mockResolvedValueOnce({ data: newProject });
    
    // Mock refresh after create
    axios.get.mockResolvedValueOnce({
      data: {
        ...mockProjects,
        items: [newProject, ...mockProjects.items]
      }
    });
    
    // Submit form
    const createButton = screen.getByRole('button', { name: /^create$/i });
    fireEvent.click(createButton);
    
    // ASSERT - Check API was called
    await waitFor(() => {
      expect(axios.post).toHaveBeenCalledWith(
        'http://localhost:5290/api/projects',
        {
          title: 'New Project',
          description: 'New Description'
        },
        {
          headers: { Authorization: `Bearer ${mockToken}` }
        }
      );
    });
    
    // Modal should close
    await waitFor(() => {
      expect(screen.queryByText('Create New Project')).not.toBeInTheDocument();
    });
  });
  
  // ═══════════════════════════════════════════════════════
  // TEST 6: Delete Project
  // ═══════════════════════════════════════════════════════
  
  it('should delete project when delete button clicked', async () => {
    // ARRANGE
    axios.get.mockResolvedValueOnce({ data: mockProjects });
    
    // Mock window.confirm
    global.confirm = vi.fn(() => true);  // User clicks "OK"
    
    renderDashboard();
    
    await waitFor(() => {
      expect(screen.getByText('Project 1')).toBeInTheDocument();
    });
    
    // ACT - Click delete button
    const deleteButtons = screen.getAllByRole('button', { name: /delete/i });
    
    // Mock delete API
    axios.delete.mockResolvedValueOnce({ data: {} });
    
    // Mock refresh after delete
    axios.get.mockResolvedValueOnce({
      data: {
        ...mockProjects,
        items: [mockProjects.items[1]]  // Only second project remains
      }
    });
    
    fireEvent.click(deleteButtons[0]);
    
    // ASSERT - Check API was called
    await waitFor(() => {
      expect(axios.delete).toHaveBeenCalledWith(
        'http://localhost:5290/api/projects/1',
        {
          headers: { Authorization: `Bearer ${mockToken}` }
        }
      );
    });
  });
  
  // ═══════════════════════════════════════════════════════
  // TEST 7: Progress Bar Display
  // ═══════════════════════════════════════════════════════
  
  it('should display progress bars correctly', async () => {
    axios.get.mockResolvedValueOnce({ data: mockProjects });
    
    renderDashboard();
    
    await waitFor(() => {
      expect(screen.getByText('Project 1')).toBeInTheDocument();
    });
    
    // Check progress text
    expect(screen.getByText('3 / 5 tasks')).toBeInTheDocument();
    expect(screen.getByText('10 / 10 tasks')).toBeInTheDocument();
    
    // Check progress percentages
    expect(screen.getByText('60%')).toBeInTheDocument();
    expect(screen.getByText('100%')).toBeInTheDocument();
  });
  
  // ═══════════════════════════════════════════════════════
  // TEST 8: Logout Functionality
  // ═══════════════════════════════════════════════════════
  
  it('should call onLogout when logout button clicked', async () => {
    axios.get.mockResolvedValueOnce({ data: mockProjects });
    
    renderDashboard();
    
    await waitFor(() => {
      expect(screen.getByText('Project 1')).toBeInTheDocument();
    });
    
    // ACT - Click logout
    const logoutButton = screen.getByRole('button', { name: /logout/i });
    fireEvent.click(logoutButton);
    
    // ASSERT
    expect(mockOnLogout).toHaveBeenCalled();
  });
});

/*
 * ═══════════════════════════════════════════════════════════
 * ADVANCED TESTING CONCEPTS
 * ═══════════════════════════════════════════════════════════
 * 
 * 1. MOCKING API CALLS
 *    - axios.get.mockResolvedValueOnce() = one-time mock
 *    - axios.get.mockResolvedValue() = persistent mock
 *    - axios.get.mockRejectedValue() = mock error
 * 
 * 2. TESTING ASYNC OPERATIONS
 *    - Always use waitFor() for async state changes
 *    - Don't test implementation details
 *    - Test what user sees, not internal state
 * 
 * 3. TESTING USER INTERACTIONS
 *    - fireEvent.click() = click
 *    - fireEvent.change() = type
 *    - fireEvent.submit() = submit form
 * 
 * 4. QUERYING ELEMENTS
 *    - getBy... = element must exist (throws if not found)
 *    - queryBy... = returns null if not found
 *    - findBy... = waits for element (async)
 * 
 * 5. TESTING LISTS
 *    - getAllByRole() = get multiple elements
 *    - within() = query within specific element
 * 
 * 6. MOCKING BROWSER APIS
 *    - window.confirm = vi.fn(() => true)
 *    - window.alert = vi.fn()
 *    - localStorage.setItem = vi.fn()
 * 
 * ═══════════════════════════════════════════════════════════
 * BEST PRACTICES
 * ═══════════════════════════════════════════════════════════
 * 
 * ✅ DO:
 * - Test user behavior, not implementation
 * - Use semantic queries (getByRole, getByLabelText)
 * - Clean up mocks between tests
 * - Test error states
 * - Test loading states
 * 
 * ❌ DON'T:
 * - Test internal state (use getByText instead)
 * - Test CSS/styling (unless critical)
 * - Make tests depend on each other
 * - Mock too much (over-mocking)
 * 
 * ═══════════════════════════════════════════════════════════
 */