# -------------------------
# Stage 1: Build the application
# -------------------------

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy solution and project files first (better Docker layer caching)
COPY ToDoApi.slnx .
COPY src/ToDoApi/ToDoApi.csproj src/ToDoApi/

# Restore only the API project (this also restores referenced projects)
RUN dotnet restore src/ToDoApi/ToDoApi.csproj

# Copy the source code
COPY src/ToDoApi src/ToDoApi/

# Publish the API
RUN dotnet publish src/ToDoApi/ToDoApi.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    -p:UseAppHost=false

# -------------------------
# Stage 2: Runtime image
# -------------------------

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ToDoApi.dll"]