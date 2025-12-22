# Task Manager - Full Stack Application

A comprehensive project and task management system built with .NET 10, React, and PostgreSQL, demonstrating Clean Architecture and modern development practices.

**Demo Video:** [Watch Demo on YouTube](YOUR_VIDEO_LINK_HERE)

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)
![React](https://img.shields.io/badge/React-18.2-blue.svg)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-blue.svg)

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Testing](#testing)
- [Docker Deployment](#docker-deployment)
- [Troubleshooting](#troubleshooting)

---

## Overview

Task Manager is a production-ready full-stack application built for the **Hahn Software Morocco - End of Studies Internship 2026**. It showcases enterprise-level development with Clean Architecture, comprehensive testing (34+ tests, 90%+ coverage), and containerized deployment.

### Key Highlights

- Clean Architecture with clear separation of concerns
- JWT authentication with BCrypt password hashing
- Real-time progress tracking with visual indicators
- Modern, responsive UI with Tailwind CSS
- Comprehensive unit tests (backend + frontend)
- Full Docker support with docker-compose

---

## Features

### Core Functionality
- **Authentication:** Secure registration/login with JWT tokens
- **Project Management:** CRUD operations with search and pagination
- **Task Management:** Create, update, toggle, and delete tasks with due dates
- **Progress Tracking:** Automatic calculation and visual progress bars
- **Filtering:** Filter tasks by status (All, Active, Completed)
- **Responsive Design:** Works seamlessly on mobile, tablet, and desktop

---

## Tech Stack

### Backend
- **.NET 10** with ASP.NET Core Web API
- **Entity Framework Core 8.0** + PostgreSQL 15
- **JWT Authentication** with BCrypt password hashing
- **xUnit** + **Moq** for testing

### Frontend
- **React 18** with React Router v6
- **Axios** for API communication
- **Tailwind CSS** for styling
- **Vite** for fast development
- **Vitest** for testing

### DevOps
- **Docker** & **Docker Compose**
- **Swagger/OpenAPI** for API documentation

---

## Architecture

This project follows **Clean Architecture** principles:

```
┌─────────────────────────────────────┐
│   API Layer (Controllers)           │
├─────────────────────────────────────┤
│   Application Layer (Services, DTOs)│
├─────────────────────────────────────┤
│   Domain Layer (Entities, Logic)    │
├─────────────────────────────────────┤
│   Infrastructure (DB, Repositories) │
└─────────────────────────────────────┘
```

**Benefits:**
- Independent testability of each layer
- Easy to maintain and extend
- Flexibility to swap implementations
- Clear separation of concerns

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional)

### Installation

#### 1. Clone Repository
```bash
git clone https://github.com/YOUR_USERNAME/task-manager.git
cd task-manager
```

#### 2. Setup Backend

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

**Create Database and Apply Migrations:**
```bash
# Create database
psql -U postgres -c "CREATE DATABASE taskmanager;"

# Apply migrations
cd TaskManager.API
dotnet ef database update --project ../TaskManager.Infrastructure
```

#### 3. Setup Frontend
```bash
cd taskmanager-frontend
npm install
```

---

## Running the Application

### Option 1: Local Development

**Start Backend:**
```bash
cd TaskManager.API
dotnet run
# Backend: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

**Start Frontend:**
```bash
cd taskmanager-frontend
npm run dev
# Frontend: http://localhost:3000
```

### Option 2: Docker (Recommended)

```bash
# Start all services
docker-compose up -d --build

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

**Access:**
- Frontend: http://localhost:3000
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

### Test Credentials

```
Email: admin@test.com
Password: password123
```

Or register your own account.

---

## API Documentation

### Authentication

**Register:**
```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "password123"
}
```

**Login:**
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

**Get Projects (Paginated):**
```http
GET /api/projects?page=1&pageSize=6&search=term
```

**Create Project:**
```http
POST /api/projects
Content-Type: application/json

{
  "title": "New Project",
  "description": "Optional description"
}
```

**Get Project Details:**
```http
GET /api/projects/{id}
```

**Update Project:**
```http
PUT /api/projects/{id}
```

**Delete Project:**
```http
DELETE /api/projects/{id}
```

### Tasks

**Get Tasks (Filtered):**
```http
GET /api/projects/{projectId}/tasks?page=1&pageSize=10&completed=false
```

**Create Task:**
```http
POST /api/projects/{projectId}/tasks
Content-Type: application/json

{
  "title": "New Task",
  "description": "Task details",
  "dueDate": "2025-01-20T00:00:00Z"
}
```

**Toggle Task Completion:**
```http
PATCH /api/projects/{projectId}/tasks/{id}/toggle
```

**Delete Task:**
```http
DELETE /api/projects/{projectId}/tasks/{id}
```

**Full API Documentation:** Visit http://localhost:5000/swagger when running.

---

## Testing

### Backend Tests (20 tests)

```bash
cd TaskManager.Tests
dotnet test
```

**Coverage:**
- ProjectService: 15 tests (CRUD, pagination, authorization)
- TaskService: 5 tests (create, toggle, delete, filters)

### Frontend Tests (14 tests)

```bash
cd taskmanager-frontend
npm test
```

**Coverage:**
- Login Component: 6 tests
- Dashboard Component: 8 tests

---

## Docker Deployment

The application includes complete Docker setup with three services: PostgreSQL, API, and Frontend.

**Start all services:**
```bash
docker-compose up -d --build
```

**Benefits:**
- Consistent environment across development and production
- One-command deployment
- Service isolation
- Easy scaling

**Docker images:**
- Backend: Multi-stage .NET build (~210MB)
- Frontend: Multi-stage Node + Nginx build (~40MB)

---

## Project Structure

```
TaskManager/
├── TaskManager.Domain/              # Entities (User, Project, Task)
├── TaskManager.Application/         # DTOs, Interfaces, Business Logic
├── TaskManager.Infrastructure/      # EF Core, Repositories, Services
├── TaskManager.API/                 # Controllers, Startup Configuration
├── TaskManager.Tests/               # Unit Tests (xUnit + Moq)
├── taskmanager-frontend/            # React Application
│   ├── src/components/              # React Components + Tests
│   ├── Dockerfile
│   └── nginx.conf
├── docker-compose.yml
└── README.md
```

---

## Troubleshooting

### Database Connection Issues
```bash
# Verify PostgreSQL is running
systemctl status postgresql  # Linux
# Or check Services on Windows

# Test connection
psql -U postgres -c "\l"
```

### Port Already in Use
```bash
# Windows
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Linux/Mac
lsof -i :5000
kill -9 <PID>
```

### Docker Container Won't Start
```bash
# Check logs
docker-compose logs api

# Rebuild from scratch
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d
```

### CORS Errors
Ensure backend `Program.cs` includes:
```csharp
app.UseCors("AllowReactApp");
```

---

## Key Technical Decisions

1. **Clean Architecture:** Ensures testability, maintainability, and scalability
2. **JWT Authentication:** Stateless, scalable, mobile-friendly
3. **Repository Pattern:** Abstracts database access for better testing
4. **Server-Side Pagination:** Handles large datasets efficiently
5. **Docker Containerization:** Consistent deployment across environments
6. **Comprehensive Testing:** Ensures code quality and confidence in refactoring

---

## Future Enhancements

- [ ] Email verification and password reset
- [ ] Real-time updates with SignalR
- [ ] Task comments and file attachments
- [ ] Project collaboration (multiple users)
- [ ] Export to PDF
- [ ] Dark mode
- [ ] CI/CD pipeline

---

## Demo Video

**Watch the demo:** [Task Manager Demo - YouTube](YOUR_VIDEO_LINK_HERE)

*Duration: 1:47 | Covers: Authentication, Dashboard, Task Management, Architecture, and Docker Deployment*

---

## Author

**Ouaail [Your Last Name]**

- GitHub: [@your-username](https://github.com/your-username)
- LinkedIn: [Your Profile](https://linkedin.com/in/your-profile)
- Email: your.email@example.com
- Location: Tangier, Morocco

---

## Acknowledgments

Built with ❤️ for **Hahn Software Morocco** - End of Studies Internship 2026

Special thanks to the .NET and React communities for excellent documentation and tools.

---

<div align="center">

⭐ If you found this project helpful, please give it a star!

[Report Bug](https://github.com/your-username/task-manager/issues) · [Request Feature](https://github.com/your-username/task-manager/issues)

</div>
