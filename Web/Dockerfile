# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution file
COPY FoodAI.slnx .

# Copy project directories
COPY Core/ ./Core/
COPY Data/ ./Data/
COPY Services/ ./Services/
COPY Web/ ./Web/

# Restore dependencies for all projects
RUN dotnet restore FoodAI.slnx

# Publish the FoodAI.Web project
WORKDIR /app/Web
RUN dotnet publish -c Release -o out

# Use the official .NET ASP.NET Core runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/Web/out .

# Copy the .env file from the source directory directly to the app directory in the final image
COPY Web/.env ./.env

# Add a health check for MongoDB (requires mongo-tools in the image)
# This is a placeholder, a more robust solution might involve a separate entrypoint script
# HEALTHCHECK --interval=5s --timeout=3s --retries=5 CMD mongosh --eval 'db.runCommand("ping").ok' || exit 1

# Optional: Copy your Google Cloud service account key file if you are using GOOGLE_APPLICATION_CREDENTIALS
# Make sure to update the path if your key file is located elsewhere
# COPY service-account-key.json /app/service-account-key.json

# Expose the port the app runs on
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Add a command to list wwwroot contents for debugging static files
RUN ls -la /app/wwwroot

ENTRYPOINT ["dotnet", "Web.dll"]
