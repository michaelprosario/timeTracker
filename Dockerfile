# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY TimeTracker.slnx ./
COPY src/TimeTracker.Core/TimeTracker.Core.csproj ./src/TimeTracker.Core/
COPY src/TimeTracker.Infrastructure/TimeTracker.Infrastructure.csproj ./src/TimeTracker.Infrastructure/
COPY src/TimeTracker.Web/TimeTracker.Web.csproj ./src/TimeTracker.Web/

# Restore dependencies
RUN dotnet restore ./src/TimeTracker.Web/TimeTracker.Web.csproj

# Copy the rest of the source code
COPY src/ ./src/

# Build the application
WORKDIR /src/src/TimeTracker.Web
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TimeTracker.Web.dll"]
