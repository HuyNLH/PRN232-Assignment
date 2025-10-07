# Backend-only Dockerfile for Render deployment
# This builds and deploys only the ECommerceApp.API

# Build stage: Use .NET SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project file and restore dependencies
COPY ECommerceApp.API/*.csproj ./ECommerceApp.API/
RUN dotnet restore ECommerceApp.API/ECommerceApp.API.csproj

# Copy source code and build
COPY ECommerceApp.API/ ./ECommerceApp.API/
WORKDIR /app/ECommerceApp.API
RUN dotnet publish -c Release -o /app/publish

# Runtime stage: Use ASP.NET Core runtime for production
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Create startup script with embedded connection testing
RUN echo '#!/bin/bash\n\
echo "=== ECommerceApp.API Starting ==="\n\
echo "Environment Variables:"\n\
echo "DB_HOST: ${DB_HOST:-NOT_SET}"\n\
echo "DB_PORT: ${DB_PORT:-NOT_SET}"\n\
echo "DB_NAME: ${DB_NAME:-NOT_SET}"\n\
echo "DB_USER: ${DB_USER:-NOT_SET}"\n\
echo "DB_PASSWORD: ${DB_PASSWORD:+[SET]}"\n\
echo ""\n\
if [ -z "$DB_HOST" ] || [ -z "$DB_PORT" ] || [ -z "$DB_NAME" ] || [ -z "$DB_USER" ] || [ -z "$DB_PASSWORD" ]; then\n\
    echo "⚠️ Some environment variables are missing!"\n\
else\n\
    echo "✅ All required environment variables are set"\n\
fi\n\
echo ""\n\
echo "Starting .NET API..."\n\
exec dotnet ECommerceApp.API.dll' > /app/start.sh

RUN chmod +x /app/start.sh

# Expose only the API port
EXPOSE 5000

# Set production environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:5000

# Start the API
CMD ["./start.sh"]