# -------------------------
# Stage 1: Build the application
# -------------------------

# Start with the .NET SDK image (contains compiler, restore tools, etc.)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Set the working directory inside the container
WORKDIR /src

# Copy only the project file first.
# This allows Docker to cache the 'restore' layer if dependencies don't change.
COPY ToDoApi.csproj .

# Download all NuGet packages
RUN dotnet restore

# Copy the rest of the application source code
COPY . .

# Build and publish the application into a deployment folder
RUN dotnet publish -c Release -o /app/publish


# -------------------------
# Stage 2: Runtime image
# -------------------------

# Use the much smaller ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0

# Set the working directory
WORKDIR /app

# Copy only the published output from the build stage
COPY --from=build /app/publish .

# Inform Docker that the application listens on port 8080
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "ToDoApi.dll"]