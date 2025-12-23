# Task Manager - Full Stack Application

> A comprehensive project and task management system built with .NET 10, React, and PostgreSQL, demonstrating Clean Architecture and modern development practices.

**Demo Video:** [Watch Demo on YouTube](https://youtu.be/TwJMXxNWE3Q)

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18.2-blue.svg)](https://reactjs.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-blue.svg)](https://www.postgresql.org/)

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technologies](#technologies)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Testing](#testing)
- [Docker Deployment](#docker-deployment)
- [Troubleshooting](#troubleshooting)

---

## 🎯 Overview

Task Manager is a production-ready full-stack application demonstrating enterprise-level software development practices with Clean Architecture, comprehensive testing, and containerized deployment.

**Built for:** Hahn Software Morocco - End of Studies Internship 2026

### Key Highlights

- ✨ Clean Architecture with clear separation of concerns
- 🔐 JWT authentication with BCrypt password hashing
- 📊 Real-time progress tracking with automatic calculation
- 🎨 Modern UI/UX with Tailwind CSS
- ✅ Unit tests with 90%+ coverage
- 🐳 Full Docker support with docker-compose
- 📄 Complete API documentation with Swagger

---

## ✨ Features

### Authentication & Security
- User registration with email validation
- Secure login with JWT token authentication
- BCrypt password hashing
- Token-based authorization on protected routes

### Project Management
- Create, read, update, and delete projects
- Search projects by title or description
- Pagination (6 projects per page)
- Real-time progress tracking with visual progress bars

### Task Management
- Add tasks with title, description, and due date
- Toggle task completion status
- Filter tasks by status (All, Active, Completed)
- Pagination (10 tasks per page)
- Search tasks within projects

### User Experience
- Responsive design (mobile, tablet, desktop)
- Loading states and smooth transitions
- Error handling with clear feedback
- Intuitive navigation

---

## 🛠 Technologies

### Backend
- .NET 10 & ASP.NET Core Web API
- Entity Framework Core 8.0
- PostgreSQL 15
- JWT Bearer Authentication
- BCrypt.Net for password hashing
- Swagger/OpenAPI
- xUnit & Moq for testing

### Frontend
- React 18 with Hooks
- React Router v6
- Axios for HTTP requests
- Tailwind CSS
- Lucide React icons
- Vite build tool
- Vitest for testing

### DevOps
- Docker & Docker Compose
- Git version control

---

## 🏗 Architecture

This project follows **Clean Architecture** principles:

```
┌─────────────────────────────────────────────────────────────┐
│                        Presentation Layer                    │
│                     (React Frontend + API)                   │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                     Application Layer                        │
│              (Business Logic, DTOs, Interfaces)              │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                      Domain Layer                            │
│            (Entities, Business Rules, Core Logic)            │
└──────────────────────────────────────────────────────────────┘
                         ▲
┌────────────────────────┴────────────────────────────────────┐
│                   Infrastructure Layer                       │
│        (Database, External Services, Repositories)           │
└──────────────────────────────────────────────────────────────┘
```

### Layer Responsibilities

- **Domain:** Core entities (User, Project, Task) and business logic
- **Application:** Use cases, DTOs, service interfaces, validation
- **Infrastructure:** EF Core, repositories, external services (JWT, BCrypt)
- **API:** HTTP endpoints, middleware, Swagger configuration

---

## 🚀 Getting Started

### Prerequisites

- Visual Studio 2022 (17.8+) with ASP.NET workload
- .NET 10 SDK
- Node.js 18+ and npm
- PostgreSQL 15+
- Docker Desktop (optional)

**Verify installations:**

```bash
dotnet --version        # Should show 10.x.x
node --version          # Should show 18.x.x or higher
psql --version          # Should show 15.x
```

### Installation

#### 1. Clone Repository

```bash
git clone https://github.com/ItsOuaail/TaskManager.git
cd TaskManager
```

#### 2. Backend Setup

**Configure Database:**

Update `TaskManager.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=taskmanager;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyThatMustBeAtLeast32CharactersLongForHS256Algorithm!",
    "Issuer": "TaskManagerAPI",
    "Audience": "TaskManagerClient"
  }
}
```

**Create Database:**

```bash
psql -U postgres
CREATE DATABASE taskmanager;
\q
```

**Apply Migrations:**

In Visual Studio Package Manager Console:

```powershell
Add-Migration InitialCreate -Project TaskManager.Infrastructure
Update-Database -Project TaskManager.Infrastructure
```

Or using .NET CLI:

```bash
cd TaskManager.API
dotnet ef migrations add InitialCreate --project ../TaskManager.Infrastructure
dotnet ef database update
```

#### 3. Frontend Setup

```bash
cd taskmanager-frontend
npm install
```

**Optional - Configure API URL:**

Create `.env` file:

```env
VITE_API_URL=http://localhost:5290/api
```

---

## ▶️ Running the Application

### Option 1: Local Development

**Start Backend:**

In Visual Studio 2022:
1. Set `TaskManager.API` as startup project
2. Press **F5**

Or via CLI:
```bash
cd TaskManager.API
dotnet run
```

Backend runs on: `http://localhost:5290`

**Start Frontend:**

```bash
cd taskmanager-frontend
npm run dev
```

Frontend runs on: `http://localhost:5173`

**Access Points:**
- Frontend: http://localhost:5173
- API: http://localhost:5290
- Swagger: http://localhost:5290/swagger

### Option 2: Docker

```bash
# Start all services
docker-compose up -d --build

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

**Access Points:**
- Frontend: http://localhost:5173
- API: http://localhost:5290
- Swagger: http://localhost:5290/swagger
- Database: localhost:5432

---

## 🔑 Test Credentials

**User:**
- Email: `user@test.com`
- Password: `password123`
 Or create your owb account 
---

## 📖 API Documentation

### Authentication

#### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "password123"
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "password123"
}
```

### Projects

All endpoints require `Authorization: Bearer {token}` header.

#### Get All Projects
```http
GET /api/projects?page=1&pageSize=6&search=term
```

#### Get Project Details
```http
GET /api/projects/{id}
```

#### Create Project
```http
POST /api/projects
Content-Type: application/json

{
  "title": "New Project",
  "description": "Optional description"
}
```

#### Update Project
```http
PUT /api/projects/{id}
Content-Type: application/json

{
  "title": "Updated Title",
  "description": "Updated description"
}
```

#### Delete Project
```http
DELETE /api/projects/{id}
```

### Tasks

#### Get Tasks
```http
GET /api/projects/{projectId}/tasks?page=1&pageSize=10&completed=false
```

#### Create Task
```http
POST /api/projects/{projectId}/tasks
Content-Type: application/json

{
  "title": "New Task",
  "description": "Task details",
  "dueDate": "2025-01-20T00:00:00Z"
}
```

#### Toggle Task Completion
```http
PATCH /api/projects/{projectId}/tasks/{id}/toggle
```

#### Delete Task
```http
DELETE /api/projects/{projectId}/tasks/{id}
```

**Interactive Documentation:** Visit http://localhost:5290/swagger when backend is running.

---

## 🧪 Testing

### Backend Tests (xUnit + Moq)

```bash
# Visual Studio
Test → Run All Tests (Ctrl+R, A)

# CLI
cd TaskManager.Tests
dotnet test
```

**Coverage:**
- ProjectService: 15 tests
- TaskService: 5 tests
- Total: 20+ tests

### Frontend Tests (Vitest)

```bash
cd taskmanager-frontend

# Watch mode
npm test

# Run once
npm run test:run
```

**Coverage:**
- Login Component: 6 tests
- Dashboard Component: 8 tests
- Total: 14+ tests

---

## 🐳 Docker Deployment

### Commands

```bash
# Start all services
docker-compose up -d --build

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Stop and remove data
docker-compose down -v
```

### Services

- **postgres:** PostgreSQL database
- **api:** .NET backend
- **frontend:** React application

---

## 📁 Project Structure

```
TaskManager/
├── TaskManager.Domain/              # Core entities
├── TaskManager.Application/         # Business logic, DTOs, interfaces
├── TaskManager.Infrastructure/      # Data access, repositories, services
├── TaskManager.API/                 # Web API, controllers
├── TaskManager.Tests/               # Unit tests
└ taskmanager-frontend/            # React application
    ├── src/
    │   ├── components/
    │   ├── App.jsx
    │   └── main.jsx
    └── Dockerfile
```

---

## 🔧 Troubleshooting

### Database Connection Failed

**Check:**
1. PostgreSQL is running
2. Connection string in `appsettings.json`
3. Database exists
4. Firewall allows port 5432

### CORS Error

**Check:**
1. Backend running on port 5290
2. `Program.cs` has `app.UseCors("AllowReactApp")`
3. Frontend URL matches CORS policy

### Port Already in Use

**Windows:**
```bash
netstat -ano | findstr :5290
taskkill /PID <PID> /F
```

**Linux/Mac:**
```bash
lsof -i :5290
kill -9 <PID>
```

### Docker Container Won't Start

```bash
# Check logs
docker-compose logs api

# Rebuild
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d
```

---

## 🎥 Demo Video

**Watch:** [Task Manager Demo - YouTube](https://youtu.be/TwJMXxNWE3Q)

**Duration:** 1:44 | **Language:** English


---

## 👤 Author

**Ouaail EL AOUAD**

- GitHub: [@ItsOuaail](https://github.com/ItsOuaail)
- LinkedIn: [Ouaail EL AOUAD](https://www.linkedin.com/in/el-aouad-ouaail/)
- Email: ouaailelaouad@gmail.com


---

## 🙏 Acknowledgments

- **Hahn Software Morocco** - Internship opportunity
- **.NET Community** - Documentation and tools
- **React Community** - Ecosystem
- **Open Source Contributors** - Libraries used in this project

---

<div align="center">

**Built with ❤️ for Hahn Software Morocco**

⭐ Star this project if you found it interesting!



</div>
