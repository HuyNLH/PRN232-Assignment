# Use the official .NET SDK image as the build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-backend
WORKDIR /app

# Copy the backend project files
COPY ECommerceApp.API/*.csproj ./ECommerceApp.API/
RUN dotnet restore ECommerceApp.API/ECommerceApp.API.csproj

# Copy the rest of the backend source code
COPY ECommerceApp.API/ ./ECommerceApp.API/

# Build and publish the backend
WORKDIR /app/ECommerceApp.API
RUN dotnet publish -c Release -o /app/publish

# Use Node.js for the frontend build
FROM node:18-alpine AS build-frontend
WORKDIR /app

# Copy package files
COPY client/package*.json ./client/
WORKDIR /app/client
RUN npm ci --only=production

# Copy frontend source code and build
COPY client/ .
RUN npm run build

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install Node.js to serve the React app (alternative: use a reverse proxy)
RUN apt-get update && apt-get install -y \
    curl \
    && curl -fsSL https://deb.nodesource.com/setup_18.x | bash - \
    && apt-get install -y nodejs \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Copy the published backend
COPY --from=build-backend /app/publish .

# Copy the built frontend
COPY --from=build-frontend /app/client/build ./wwwroot

# Create a simple script to start both backend and serve frontend
RUN echo '#!/bin/bash\n\
# Start the .NET API in the background\n\
dotnet ECommerceApp.API.dll &\n\
\n\
# Serve the React build files\n\
cd wwwroot\n\
npx serve -s . -l 3000 &\n\
\n\
# Wait for both processes\n\
wait' > /app/start.sh

RUN chmod +x /app/start.sh

# Expose ports
EXPOSE 5000 3000

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

# Start the application
CMD ["./start.sh"]
