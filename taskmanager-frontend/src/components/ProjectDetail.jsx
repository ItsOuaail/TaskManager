import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';
import { 
  ArrowLeft, 
  Plus, 
  CheckCircle2, 
  Circle, 
  Trash2, 
  Calendar 
} from 'lucide-react';

const API_URL = 'http://localhost:5290/api';

function ProjectDetail({ token }) {
  const { id } = useParams();
  const navigate = useNavigate();
  const [project, setProject] = useState(null);
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [filterCompleted, setFilterCompleted] = useState(null); // null = all, true = completed, false = active
  const [newTask, setNewTask] = useState({ title: '', description: '', dueDate: '' });
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  useEffect(() => {
    fetchProjectAndTasks();
  }, [id, filterCompleted, currentPage]);

  const fetchProjectAndTasks = async () => {
    try {
      setLoading(true);
      const [projectRes, tasksRes] = await Promise.all([
        axios.get(`${API_URL}/projects/${id}`, {
          headers: { Authorization: `Bearer ${token}` }
        }),
        axios.get(`${API_URL}/projects/${id}/tasks`, {
          headers: { Authorization: `Bearer ${token}` },
          params: { 
            page: currentPage,
            pageSize,
            completed: filterCompleted 
          }
        })
      ]);

      setProject(projectRes.data);
      setTasks(tasksRes.data.items || []);
      setTotalPages(tasksRes.data.totalPages || 1);
      setTotalCount(tasksRes.data.totalCount || 0);
    } catch (err) {
      console.error(err);
      if (err.response?.status === 404) {
        navigate('/dashboard');
      }
    } finally {
      setLoading(false);
    }
  };

  const createTask = async (e) => {
    e.preventDefault();
    if (!newTask.title.trim()) return;

    try {
      const taskData = {
        title: newTask.title.trim(),
        description: newTask.description.trim() || null,
        dueDate: newTask.dueDate ? new Date(newTask.dueDate).toISOString() : null
      };

      const response = await axios.post(
        `${API_URL}/projects/${id}/tasks`,
        taskData,
        { headers: { Authorization: `Bearer ${token}` } }
      );

      setShowCreateModal(false);
      setNewTask({ title: '', description: '', dueDate: '' });
      setCurrentPage(1);
      fetchProjectAndTasks(); // Refresh progress & list
    } catch (err) {
      console.error(err);
      alert('Failed to create task. Please try again.');
    }
  };

  const toggleTaskCompletion = async (taskId) => {
    try {
      const response = await axios.patch(
        `${API_URL}/projects/${id}/tasks/${taskId}/toggle`,
        {},
        { headers: { Authorization: `Bearer ${token}` } }
      );
      setTasks(tasks.map(t => t.id === taskId ? response.data : t));
      fetchProjectAndTasks(); // Update project progress
    } catch (err) {
      console.error(err);
    }
  };

  const deleteTask = async (taskId) => {
    if (!window.confirm('Delete this task permanently? This cannot be undone.')) return;

    try {
      await axios.delete(`${API_URL}/projects/${id}/tasks/${taskId}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      fetchProjectAndTasks();
    } catch (err) {
      console.error(err);
    }
  };

  const formatDate = (dateString) => {
    if (!dateString) return null;
    const date = new Date(dateString);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const taskDate = new Date(date);
    taskDate.setHours(0, 0, 0, 0);

    const isOverdue = taskDate < today && !project?.tasks?.find(t => t.id === taskDate)?.isCompleted;

    return {
      formatted: date.toLocaleDateString('en-US', { 
        month: 'short', 
        day: 'numeric', 
        year: taskDate.getFullYear() !== today.getFullYear() ? 'numeric' : undefined 
      }),
      isOverdue: !isOverdue ? false : true // Simplified for clarity
    };
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="animate-spin rounded-full h-16 w-16 border-t-4 border-b-4 border-indigo-600"></div>
      </div>
    );
  }

  if (!project) return null;

  return (
    <div className="min-h-screen bg-gray-100">
      {/* Header */}
      <header className="bg-white shadow-sm border-b border-gray-200 fixed top-0 left-0 right-0 z-40">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            <button
              onClick={() => navigate('/dashboard')}
              className="flex items-center gap-2 text-gray-600 hover:text-gray-900 transition"
            >
              <ArrowLeft className="w-5 h-5" />
              <span className="font-medium">Back to Projects</span>
            </button>
            <h1 className="text-xl font-bold text-indigo-700 hidden sm:block">ProTask</h1>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="pt-20 pb-12 px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto">
        {/* Project Title & Description */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-3">{project.title}</h1>
          {project.description && (
            <p className="text-lg text-gray-600 max-w-3xl">{project.description}</p>
          )}
        </div>

        {/* Progress Card */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-200 p-8 mb-10">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-xl font-semibold text-gray-900">Project Progress</h2>
            <div className="text-right">
              <p className="text-4xl font-bold text-indigo-600">{project.progressPercentage}%</p>
              <p className="text-sm text-gray-600 mt-1">
                {project.completedTasks} of {project.totalTasks} tasks completed
              </p>
            </div>
          </div>
          <div className="w-full bg-gray-200 rounded-full h-4 overflow-hidden">
            <div
              className={`h-full rounded-full transition-all duration-700 ${
                project.progressPercentage === 100 ? 'bg-green-500' : 'bg-indigo-600'
              }`}
              style={{ width: `${project.progressPercentage}%` }}
            />
          </div>
          {project.progressPercentage === 100 && project.totalTasks > 0 && (
            <p className="mt-4 text-green-600 font-medium flex items-center gap-2">
              <CheckCircle2 className="w-5 h-5" />
              Project Completed!
            </p>
          )}
        </div>

        {/* Filters & Add Task */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-6 mb-8">
          <div className="flex gap-3">
            {[
              { label: 'All Tasks', value: null },
              { label: 'Active', value: false },
              { label: 'Completed', value: true }
            ].map(({ label, value }) => (
              <button
                key={value ?? 'all'}
                onClick={() => {
                  setFilterCompleted(value);
                  setCurrentPage(1);
                }}
                className={`px-6 py-3 rounded-xl font-medium transition ${
                  filterCompleted === value
                    ? 'bg-indigo-600 text-white shadow-md'
                    : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'
                }`}
              >
                {label}
              </button>
            ))}
          </div>

          <button
            onClick={() => setShowCreateModal(true)}
            className="flex items-center gap-2 px-6 py-3 bg-indigo-600 text-white font-medium rounded-xl hover:bg-indigo-700 shadow-md transition"
          >
            <Plus className="w-5 h-5" />
            Add Task
          </button>
        </div>

        {/* Tasks List */}
        {tasks.length === 0 ? (
          <div className="bg-white rounded-2xl shadow-sm border border-gray-200 p-20 text-center">
            <Circle className="w-20 h-20 text-gray-300 mx-auto mb-6" />
            <h3 className="text-2xl font-semibold text-gray-800 mb-3">
              {filterCompleted === null ? 'No tasks yet' : `No ${filterCompleted ? 'completed' : 'active'} tasks`}
            </h3>
            <p className="text-gray-600 mb-8 max-w-md mx-auto">
              {filterCompleted === null 
                ? 'Start by adding your first task to this project.'
                : 'Try switching filters or adding a new task.'}
            </p>
            <button
              onClick={() => setShowCreateModal(true)}
              className="px-6 py-3 bg-indigo-600 text-white font-medium rounded-xl hover:bg-indigo-700 transition shadow-md"
            >
              Add Your First Task
            </button>
          </div>
        ) : (
          <div className="space-y-4">
            {tasks.map((task) => {
              const dueInfo = task.dueDate ? formatDate(task.dueDate) : null;
              const isOverdue = dueInfo?.isOverdue && !task.isCompleted;

              return (
                <div
                  key={task.id}
                  className="bg-white rounded-2xl shadow-sm hover:shadow-md transition-all duration-300 border border-gray-200 p-6 flex items-start gap-5 group"
                >
                  <button
                    onClick={() => toggleTaskCompletion(task.id)}
                    className="mt-1 flex-shrink-0 focus:outline-none focus:ring-2 focus:ring-indigo-500 rounded-full"
                    aria-label={task.isCompleted ? 'Mark as incomplete' : 'Mark as complete'}
                  >
                    {task.isCompleted ? (
                      <CheckCircle2 className="w-7 h-7 text-green-600" />
                    ) : (
                      <Circle className="w-7 h-7 text-gray-400 hover:text-indigo-600 transition" />
                    )}
                  </button>

                  <div className="flex-1 min-w-0">
                    <h3 className={`text-xl font-medium ${
                      task.isCompleted 
                        ? 'line-through text-gray-500' 
                        : 'text-gray-900'
                    }`}>
                      {task.title}
                    </h3>

                    {task.description && (
                      <p className="text-gray-600 mt-2 leading-relaxed">
                        {task.description}
                      </p>
                    )}

                    {dueInfo && (
                      <div className={`flex items-center gap-2 mt-3 text-sm font-medium ${
                        isOverdue ? 'text-red-600' : 'text-gray-600'
                      }`}>
                        <Calendar className="w-4 h-4" />
                        <span>
                          Due {dueInfo.formatted}
                          {isOverdue && ' • Overdue'}
                        </span>
                      </div>
                    )}
                  </div>

                  <button
                    onClick={() => deleteTask(task.id)}
                    className="opacity-0 group-hover:opacity-100 transition-opacity p-2 text-red-500 hover:bg-red-50 rounded-lg"
                    aria-label="Delete task"
                  >
                    <Trash2 className="w-5 h-5" />
                  </button>
                </div>
              );
            })}
          </div>
        )}

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="mt-12 flex justify-center">
            <nav className="flex items-center gap-2">
              <button
                onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                disabled={currentPage === 1}
                className="px-5 py-3 text-sm font-medium bg-white border border-gray-300 rounded-xl hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition"
              >
                Previous
              </button>

              <div className="flex gap-1">
                {Array.from({ length: totalPages }, (_, i) => i + 1).map(page => (
                  <button
                    key={page}
                    onClick={() => setCurrentPage(page)}
                    className={`px-4 py-3 text-sm font-medium rounded-xl transition ${
                      currentPage === page
                        ? 'bg-indigo-600 text-white'
                        : 'bg-white border border-gray-300 text-gray-700 hover:bg-gray-50'
                    }`}
                  >
                    {page}
                  </button>
                ))}
              </div>

              <button
                onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                disabled={currentPage === totalPages}
                className="px-5 py-3 text-sm font-medium bg-white border border-gray-300 rounded-xl hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition"
              >
                Next
              </button>
            </nav>
          </div>
        )}
      </main>

      {/* Create Task Modal */}
      {showCreateModal && (
        <div 
          className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50"
          onClick={() => setShowCreateModal(false)}
        >
          <div 
            className="bg-white rounded-2xl shadow-2xl p-8 w-full max-w-lg"
            onClick={(e) => e.stopPropagation()}
          >
            <h2 className="text-2xl font-bold text-gray-900 mb-6">Add New Task</h2>
            <form onSubmit={createTask} className="space-y-6">
              <div>
                <label htmlFor="task-title" className="block text-sm font-medium text-gray-700 mb-2">
                  Task Title <span className="text-red-500">*</span>
                </label>
                <input
                  id="task-title"
                  type="text"
                  required
                  value={newTask.title}
                  onChange={(e) => setNewTask({ ...newTask, title: e.target.value })}
                  className="w-full px-4 py-3 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 transition"
                  placeholder="e.g., Design homepage mockup"
                />
              </div>

              <div>
                <label htmlFor="task-desc" className="block text-sm font-medium text-gray-700 mb-2">
                  Description <span className="text-gray-500">(optional)</span>
                </label>
                <textarea
                  id="task-desc"
                  rows={4}
                  value={newTask.description}
                  onChange={(e) => setNewTask({ ...newTask, description: e.target.value })}
                  className="w-full px-4 py-3 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 resize-none transition"
                  placeholder="Add details about what needs to be done..."
                />
              </div>

              <div>
                <label htmlFor="due-date" className="block text-sm font-medium text-gray-700 mb-2">
                  Due Date <span className="text-gray-500">(optional)</span>
                </label>
                <input
                  id="due-date"
                  type="date"
                  value={newTask.dueDate}
                  onChange={(e) => setNewTask({ ...newTask, dueDate: e.target.value })}
                  className="w-full px-4 py-3 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 transition"
                />
              </div>

              <div className="flex gap-4 pt-4">
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="flex-1 px-6 py-3 border border-gray-300 text-gray-700 font-medium rounded-xl hover:bg-gray-50 transition"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="flex-1 px-6 py-3 bg-indigo-600 text-white font-medium rounded-xl hover:bg-indigo-700 shadow-md transition"
                >
                  Create Task
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default ProjectDetail;