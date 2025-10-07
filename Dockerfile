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

# Copy connection test script
COPY test-connection.sh /app/test-connection.sh
RUN chmod +x /app/test-connection.sh

# Create startup script for the API with connection testing
RUN echo '#!/bin/bash\n\
./test-connection.sh\n\
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