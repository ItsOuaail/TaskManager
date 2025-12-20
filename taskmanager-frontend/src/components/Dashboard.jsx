import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { Plus, LogOut, Search, FolderOpen, Trash2, Edit2, CheckCircle2 } from 'lucide-react';

const API_URL = 'http://localhost:5290/api';

function Dashboard({ token, user, onLogout }) {
  const navigate = useNavigate();
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [newProject, setNewProject] = useState({ title: '', description: '' });
  const [error, setError] = useState('');

  useEffect(() => {
    fetchProjects();
  }, [searchTerm]);

  const fetchProjects = async () => {
    try {
      setLoading(true);
      const response = await axios.get(`${API_URL}/projects`, {
        headers: { Authorization: `Bearer ${token}` },
        params: { search: searchTerm, pageSize: 100 }
      });
      setProjects(response.data.items);
    } catch (err) {
      setError('Failed to fetch projects');
    } finally {
      setLoading(false);
    }
  };

  const createProject = async (e) => {
    e.preventDefault();
    try {
      const response = await axios.post(
        `${API_URL}/projects`,
        newProject,
        { headers: { Authorization: `Bearer ${token}` } }
      );
      setProjects([response.data, ...projects]);
      setShowCreateModal(false);
      setNewProject({ title: '', description: '' });
    } catch (err) {
      setError('Failed to create project');
    }
  };

  const deleteProject = async (id) => {
    if (!confirm('Are you sure you want to delete this project?')) return;
    try {
      await axios.delete(`${API_URL}/projects/${id}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      setProjects(projects.filter(p => p.id !== id));
    } catch (err) {
      setError('Failed to delete project');
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 py-4 flex justify-between items-center">
          <div>
            <h1 className="text-2xl font-bold text-gray-800">Task Manager</h1>
            <p className="text-sm text-gray-600">Welcome back, {user?.name}!</p>
          </div>
          <button
            onClick={onLogout}
            className="flex items-center gap-2 px-4 py-2 text-red-600 hover:bg-red-50 rounded-lg transition"
          >
            <LogOut className="w-4 h-4" />
            Logout
          </button>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 py-8">
        {/* Actions Bar */}
        <div className="flex justify-between items-center mb-6">
          <div className="flex-1 max-w-md">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
              <input
                type="text"
                placeholder="Search projects..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
              />
            </div>
          </div>
          <button
            onClick={() => setShowCreateModal(true)}
            className="ml-4 flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
          >
            <Plus className="w-5 h-5" />
            New Project
          </button>
        </div>

        {/* Error Message */}
        {error && (
          <div className="mb-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
            {error}
          </div>
        )}

        {/* Projects Grid */}
        {loading ? (
          <div className="text-center py-12">
            <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
          </div>
        ) : projects.length === 0 ? (
          <div className="text-center py-12 bg-white rounded-lg shadow">
            <FolderOpen className="w-16 h-16 text-gray-400 mx-auto mb-4" />
            <h3 className="text-xl font-semibold text-gray-700 mb-2">No projects yet</h3>
            <p className="text-gray-500 mb-4">Create your first project to get started</p>
            <button
              onClick={() => setShowCreateModal(true)}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
            >
              Create Project
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {projects.map((project) => (
              <div
                key={project.id}
                className="bg-white rounded-lg shadow hover:shadow-lg transition cursor-pointer p-6 border border-gray-200"
                onClick={() => navigate(`/projects/${project.id}`)}
              >
                <div className="flex justify-between items-start mb-4">
                  <h3 className="text-lg font-semibold text-gray-800 flex-1 pr-2">
                    {project.title}
                  </h3>
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      deleteProject(project.id);
                    }}
                    className="text-red-500 hover:text-red-700 p-1"
                  >
                    <Trash2 className="w-5 h-5" />
                  </button>
                </div>
                
                {project.description && (
                  <p className="text-gray-600 text-sm mb-4 line-clamp-2">
                    {project.description}
                  </p>
                )}

                <div className="space-y-3">
                  <div className="flex justify-between text-sm text-gray-600">
                    <span>Progress</span>
                    <span className="font-medium">{project.progressPercentage}%</span>
                  </div>
                  <div className="w-full bg-gray-200 rounded-full h-2">
                    <div
                      className="bg-blue-600 h-2 rounded-full transition-all"
                      style={{ width: `${project.progressPercentage}%` }}
                    ></div>
                  </div>
                  <div className="flex items-center justify-between text-sm text-gray-600">
                    <span>{project.completedTasks} / {project.totalTasks} tasks</span>
                    {project.completedTasks === project.totalTasks && project.totalTasks > 0 && (
                      <CheckCircle2 className="w-5 h-5 text-green-600" />
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </main>

      {/* Create Project Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg shadow-xl p-6 w-full max-w-md">
            <h2 className="text-2xl font-bold text-gray-800 mb-4">Create New Project</h2>
            <form onSubmit={createProject} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Project Title *
                </label>
                <input
                  type="text"
                  value={newProject.title}
                  onChange={(e) => setNewProject({ ...newProject, title: e.target.value })}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
                  placeholder="Enter project title"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Description
                </label>
                <textarea
                  value={newProject.description}
                  onChange={(e) => setNewProject({ ...newProject, description: e.target.value })}
                  rows="3"
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none resize-none"
                  placeholder="Enter project description (optional)"
                ></textarea>
              </div>
              <div className="flex gap-3">
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="flex-1 px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
                >
                  Create
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default Dashboard;