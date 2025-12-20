# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["TaskManager.API/TaskManager.API.csproj", "TaskManager.API/"]
COPY ["TaskManager.Application/TaskManager.Application.csproj", "TaskManager.Application/"]
COPY ["TaskManager.Domain/TaskManager.Domain.csproj", "TaskManager.Domain/"]
COPY ["TaskManager.Infrastructure/TaskManager.Infrastructure.csproj", "TaskManager.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "TaskManager.API/TaskManager.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/TaskManager.API"
RUN dotnet build "TaskManager.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "TaskManager.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Set entry point
ENTRYPOINT ["dotnet", "TaskManager.API.dll"]