# Build stage
FROM node:18-alpine AS build

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build the application
RUN npm run build

# Production stage with nginx
FROM nginx:alpine

# Copy built files to nginx
COPY --from=build /app/dist /usr/share/nginx/html

# Copy nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]

# DOCKERFILE EXPLAINED:
# 
# Two-stage build for React app:
# 
# 1. BUILD STAGE:
#    - Use Node.js to build the React app
#    - npm ci: Clean install (faster than npm install)
#    - npm run build: Creates optimized production build
#    - Output goes to /app/dist
# 
# 2. PRODUCTION STAGE:
#    - Use nginx (lightweight web server)
#    - Copy built files from previous stage
#    - nginx serves static files (HTML, CSS, JS)
#    - Only ~40MB final image size
# 
# Build manually:
# docker build -t taskmanager-frontend .
# docker run -p 3000:80 taskmanager-frontend