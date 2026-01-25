# Docker Hub Quick Reference Guide

## Overview

This guide provides quick reference information for managing the Time Tracker application container images on Docker Hub.

## Docker Hub Setup

### 1. Create Docker Hub Account

1. Go to [https://hub.docker.com/signup](https://hub.docker.com/signup)
2. Create a free or Pro account
3. Verify your email address

### 2. Create Repository

```bash
# Option 1: Via Web UI
# 1. Login to Docker Hub
# 2. Click "Create Repository"
# 3. Name: timetracker
# 4. Visibility: Public or Private
# 5. Click "Create"

# Option 2: Automatically created on first push
docker push yourusername/timetracker:latest
```

### 3. Generate Access Token

**Recommended**: Use access tokens instead of passwords for better security.

1. Login to Docker Hub: [https://hub.docker.com](https://hub.docker.com)
2. Go to Account Settings → Security
3. Click "New Access Token"
4. Name: `timetracker-deployment`
5. Permissions: Read & Write
6. Copy the token (you won't see it again!)
7. Save it securely

## Environment Variables Setup

### Local Development

```bash
# Required for building and pushing
export DOCKERHUB_USERNAME="your-dockerhub-username"
export DOCKERHUB_PASSWORD="your-access-token"  # Use access token, not password
export DOCKERHUB_REPOSITORY="your-dockerhub-username/timetracker"

# Required for Azure deployment
export POSTGRESQL_ADMIN_PASSWORD="YourSecurePassword123!"

# Optional: Specific image tag
export DOCKER_IMAGE_TAG="v1.0.0"
```

### CI/CD Pipeline (GitHub Actions)

Set these as GitHub Secrets:

```yaml
# Repository Settings → Secrets → Actions
DOCKERHUB_USERNAME: your-dockerhub-username
DOCKERHUB_TOKEN: dckr_pat_xxxxxxxxxxxxxxxxxxxxx
POSTGRESQL_ADMIN_PASSWORD: YourSecurePassword123!
```

## Common Operations

### Build and Push Image

#### Using the Script

```bash
cd deployment/scripts

# Build and push with automatic tag
./build-and-push.sh \
  --dockerhub-username $DOCKERHUB_USERNAME \
  --tag $(date +%Y%m%d-%H%M%S)

# Build and push with semantic version
./build-and-push.sh \
  --dockerhub-username $DOCKERHUB_USERNAME \
  --tag v1.2.3
```

#### Manual Build and Push

```bash
# Build the image
docker build -t $DOCKERHUB_USERNAME/timetracker:latest .

# Tag with version
docker tag $DOCKERHUB_USERNAME/timetracker:latest \
  $DOCKERHUB_USERNAME/timetracker:v1.0.0

# Login to Docker Hub
echo $DOCKERHUB_PASSWORD | docker login -u $DOCKERHUB_USERNAME --password-stdin

# Push images
docker push $DOCKERHUB_USERNAME/timetracker:latest
docker push $DOCKERHUB_USERNAME/timetracker:v1.0.0
```

### Pull Image

```bash
# Public repository (no authentication needed)
docker pull michaelprosario/timetracker:latest

# Private repository (requires authentication)
docker login -u $DOCKERHUB_USERNAME
docker pull $DOCKERHUB_USERNAME/timetracker:latest
```

### Run Image Locally

```bash
# Pull and run
docker pull $DOCKERHUB_USERNAME/timetracker:latest

# Run with environment variables
docker run -d \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="Host=localhost;Database=timetracker;Username=postgres;Password=postgres" \
  --name timetracker \
  $DOCKERHUB_USERNAME/timetracker:latest

# View logs
docker logs -f timetracker

# Access the application
open http://localhost:8080
```

### List Images and Tags

```bash
# List local images
docker images | grep timetracker

# List all tags in Docker Hub (requires curl and jq)
curl -s "https://hub.docker.com/v2/repositories/$DOCKERHUB_USERNAME/timetracker/tags" | \
  jq -r '.results[].name'
```

### Delete Images

```bash
# Delete local image
docker rmi $DOCKERHUB_USERNAME/timetracker:latest

# Delete from Docker Hub (via Web UI)
# 1. Go to repository
# 2. Click on "Tags" tab
# 3. Select tag to delete
# 4. Click "Delete"
```

## Tagging Strategy

### Recommended Tags

```bash
# 1. Latest (always points to most recent build)
docker tag image $DOCKERHUB_USERNAME/timetracker:latest

# 2. Semantic Version (for releases)
docker tag image $DOCKERHUB_USERNAME/timetracker:v1.2.3

# 3. Git SHA (for traceability)
docker tag image $DOCKERHUB_USERNAME/timetracker:$(git rev-parse --short HEAD)

# 4. Environment + Version (for environment-specific builds)
docker tag image $DOCKERHUB_USERNAME/timetracker:prod-v1.2.3

# 5. Timestamp (for automated builds)
docker tag image $DOCKERHUB_USERNAME/timetracker:$(date +%Y%m%d-%H%M%S)
```

### Example Multi-Tag Build

```bash
IMAGE_VERSION="v1.2.3"
GIT_SHA=$(git rev-parse --short HEAD)
TIMESTAMP=$(date +%Y%m%d-%H%M%S)

docker build -t $DOCKERHUB_USERNAME/timetracker:latest .
docker tag $DOCKERHUB_USERNAME/timetracker:latest $DOCKERHUB_USERNAME/timetracker:$IMAGE_VERSION
docker tag $DOCKERHUB_USERNAME/timetracker:latest $DOCKERHUB_USERNAME/timetracker:$GIT_SHA
docker tag $DOCKERHUB_USERNAME/timetracker:latest $DOCKERHUB_USERNAME/timetracker:$TIMESTAMP

docker push $DOCKERHUB_USERNAME/timetracker:latest
docker push $DOCKERHUB_USERNAME/timetracker:$IMAGE_VERSION
docker push $DOCKERHUB_USERNAME/timetracker:$GIT_SHA
docker push $DOCKERHUB_USERNAME/timetracker:$TIMESTAMP
```

## Public vs Private Repositories

### Public Repository

**Pros:**
- Free unlimited pulls
- No authentication needed for pulls
- Easy sharing with community
- Promotes open source

**Cons:**
- Anyone can see and pull your images
- May not be suitable for proprietary software

**Use Cases:**
- Open source projects
- Demo applications
- Community tools

### Private Repository

**Pros:**
- Only you and authorized users can pull
- Secure for proprietary code
- Better for production deployments

**Cons:**
- Limited pulls on free tier
- Requires authentication
- May need Pro account ($5/month)

**Use Cases:**
- Production applications
- Proprietary software
- Internal tools

### Converting Between Public and Private

```bash
# Via Web UI:
# 1. Go to repository settings
# 2. Click "Make Private" or "Make Public"
# 3. Confirm the change
```

## Azure App Service Integration

### Configure App Service for Public Docker Hub Image

No authentication needed:

```bash
az webapp config container set \
  --name timetracker-web \
  --resource-group rg-timetracker-dev \
  --docker-custom-image-name "michaelprosario/timetracker:latest"
```

### Configure App Service for Private Docker Hub Image

Requires authentication:

```bash
az webapp config container set \
  --name timetracker-web \
  --resource-group rg-timetracker-dev \
  --docker-custom-image-name "$DOCKERHUB_USERNAME/timetracker:latest" \
  --docker-registry-server-url "https://index.docker.io/v1" \
  --docker-registry-server-user "$DOCKERHUB_USERNAME" \
  --docker-registry-server-password "$DOCKERHUB_PASSWORD"
```

### Update App Service to New Image Tag

```bash
# Update to specific version
az webapp config container set \
  --name timetracker-web \
  --resource-group rg-timetracker-dev \
  --docker-custom-image-name "$DOCKERHUB_USERNAME/timetracker:v1.2.3"

# Restart to pull new image
az webapp restart \
  --name timetracker-web \
  --resource-group rg-timetracker-dev
```

## Troubleshooting

### Authentication Failed

```bash
# Problem: docker login fails
# Solution: Use access token instead of password

# Generate new access token from Docker Hub
# Then login:
echo $DOCKERHUB_PASSWORD | docker login -u $DOCKERHUB_USERNAME --password-stdin
```

### Rate Limiting

```bash
# Problem: "You have reached your pull rate limit"
# Solutions:
# 1. Authenticate (increases limit)
docker login -u $DOCKERHUB_USERNAME

# 2. Upgrade to Pro account ($5/month)
# 3. Use image caching in Azure App Service
# 4. Deploy during off-peak hours
```

### Image Not Found

```bash
# Problem: "Error: manifest for username/timetracker:tag not found"
# Solutions:

# 1. Check if image exists
docker pull $DOCKERHUB_USERNAME/timetracker:latest

# 2. List available tags
curl -s "https://hub.docker.com/v2/repositories/$DOCKERHUB_USERNAME/timetracker/tags" | jq

# 3. Push the image if missing
./deployment/scripts/build-and-push.sh --dockerhub-username $DOCKERHUB_USERNAME
```

### Azure App Service Won't Pull Image

```bash
# Problem: App Service shows "Container didn't respond to HTTP pings"
# Solutions:

# 1. Check container logs
az webapp log tail --name timetracker-web --resource-group rg-timetracker-dev

# 2. Verify image works locally
docker run -p 8080:8080 $DOCKERHUB_USERNAME/timetracker:latest

# 3. Check App Service configuration
az webapp config show --name timetracker-web --resource-group rg-timetracker-dev

# 4. Verify credentials (for private repos)
az webapp config appsettings list --name timetracker-web --resource-group rg-timetracker-dev | grep DOCKER_REGISTRY
```

### Image Size Too Large

```bash
# Problem: Image push is slow or fails
# Solutions:

# 1. Check image size
docker images | grep timetracker

# 2. Use .dockerignore to exclude unnecessary files
cat .dockerignore

# 3. Use multi-stage builds (already in Dockerfile)
# 4. Remove development dependencies

# 5. Analyze image layers
docker history $DOCKERHUB_USERNAME/timetracker:latest
```

## Security Best Practices

### 1. Use Access Tokens

Never use your Docker Hub password in scripts or CI/CD:

```bash
# ❌ Bad: Using password
DOCKERHUB_PASSWORD="my-password"

# ✅ Good: Using access token
DOCKERHUB_PASSWORD="dckr_pat_xxxxxxxxxxxxxxxxxxxxx"
```

### 2. Store Credentials Securely

```bash
# Local development: Use environment variables
export DOCKERHUB_PASSWORD="dckr_pat_xxxxxxxxxxxxxxxxxxxxx"

# Azure: Store in Key Vault
az keyvault secret set \
  --vault-name kv-timetracker \
  --name dockerhub-password \
  --value "dckr_pat_xxxxxxxxxxxxxxxxxxxxx"

# CI/CD: Use secrets manager
# GitHub: Repository Settings → Secrets
# Azure DevOps: Pipeline → Variables → Secret
```

### 3. Use Private Repositories for Production

```bash
# Production deployments should use private repositories
# This prevents unauthorized access to your application images
```

### 4. Scan Images for Vulnerabilities

```bash
# Docker Hub Pro includes image scanning
# Or use Trivy locally:
trivy image $DOCKERHUB_USERNAME/timetracker:latest
```

### 5. Rotate Access Tokens Regularly

```bash
# Rotate access tokens every 90 days:
# 1. Generate new token in Docker Hub
# 2. Update environment variables
# 3. Update Azure Key Vault secrets
# 4. Delete old token
```

## Monitoring and Metrics

### Docker Hub Metrics

View in Docker Hub Web UI:
- Pull count
- Last pushed date
- Image size
- Vulnerabilities (Pro account)

### Query Docker Hub API

```bash
# Get repository information
curl -s "https://hub.docker.com/v2/repositories/$DOCKERHUB_USERNAME/timetracker/" | jq

# Get tag information
curl -s "https://hub.docker.com/v2/repositories/$DOCKERHUB_USERNAME/timetracker/tags/" | jq

# Get pull statistics (requires authentication)
curl -s -H "Authorization: JWT $TOKEN" \
  "https://hub.docker.com/v2/repositories/$DOCKERHUB_USERNAME/timetracker/" | jq
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Build and Push to Docker Hub

on:
  push:
    branches: [main]
  release:
    types: [created]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v2
      
      - name: Login to Docker Hub
        uses: docker/login-action@v2
        with:
          username: ${{ secrets.DOCKERHUB_USERNAME }}
          password: ${{ secrets.DOCKERHUB_TOKEN }}
      
      - name: Build and push
        uses: docker/build-push-action@v4
        with:
          context: .
          file: ./deployment/docker/Dockerfile.production
          push: true
          tags: |
            ${{ secrets.DOCKERHUB_USERNAME }}/timetracker:latest
            ${{ secrets.DOCKERHUB_USERNAME }}/timetracker:${{ github.sha }}
            ${{ secrets.DOCKERHUB_USERNAME }}/timetracker:${{ github.ref_name }}
```

## Resources

- **Docker Hub**: [https://hub.docker.com](https://hub.docker.com)
- **Docker Hub Docs**: [https://docs.docker.com/docker-hub/](https://docs.docker.com/docker-hub/)
- **Docker CLI Reference**: [https://docs.docker.com/engine/reference/commandline/cli/](https://docs.docker.com/engine/reference/commandline/cli/)
- **Rate Limits**: [https://docs.docker.com/docker-hub/download-rate-limit/](https://docs.docker.com/docker-hub/download-rate-limit/)
- **Access Tokens**: [https://docs.docker.com/docker-hub/access-tokens/](https://docs.docker.com/docker-hub/access-tokens/)

## Support

For issues related to:
- **Docker Hub**: [https://hub.docker.com/support/contact/](https://hub.docker.com/support/contact/)
- **Time Tracker Deployment**: See [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
- **Azure App Service**: [https://docs.microsoft.com/azure/app-service/](https://docs.microsoft.com/azure/app-service/)
