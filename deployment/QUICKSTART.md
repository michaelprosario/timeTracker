# 🚀 Quick Start: Docker Hub Deployment

This is a quick reference for deploying the Time Tracker application using Docker Hub.

## Prerequisites Checklist

- [ ] Azure subscription with CLI installed and logged in
- [ ] Docker Hub account created
- [ ] Docker Hub access token generated
- [ ] Docker installed locally
- [ ] Git repository cloned

## Step-by-Step Deployment

### 1. Set Environment Variables

```bash
# Required for Docker Hub
export DOCKERHUB_USERNAME="your-dockerhub-username"
export DOCKERHUB_PASSWORD="your-access-token"  # From https://hub.docker.com/settings/security
export DOCKERHUB_REPOSITORY="${DOCKERHUB_USERNAME}/timetracker"

# Required for Azure
export POSTGRESQL_ADMIN_PASSWORD="YourSecurePassword123!"

# Optional: Specific version tag
export DOCKER_IMAGE_TAG="v1.0.0"
```

### 2. Build and Push Docker Image

```bash
cd /workspaces/timeTracker/deployment/scripts

# Build and push to Docker Hub
./build-and-push.sh \
  --dockerhub-username $DOCKERHUB_USERNAME \
  --tag ${DOCKER_IMAGE_TAG:-latest}
```

Expected output:
```
[INFO] Building and pushing Docker image to Docker Hub...
[INFO] Logging in to Docker Hub...
[SUCCESS] Logged in to Docker Hub
[INFO] Building Docker image...
[SUCCESS] Docker image built successfully
[INFO] Pushing image to Docker Hub...
[SUCCESS] Image pushed to Docker Hub: your-username/timetracker:latest
```

### 3. Deploy to Azure

```bash
# Deploy to development
./deploy.sh --environment dev --location eastus

# Or deploy to production with migrations
./deploy.sh --environment prod --location westus2 --run-migrations
```

Expected output:
```
[INFO] ========================================
[INFO] Time Tracker Azure Deployment
[INFO] ========================================
[INFO] Environment:      dev
[INFO] Resource Group:   rg-timetracker-dev
[INFO] Docker Hub:       your-username/timetracker
[SUCCESS] Deployment Complete!
[INFO] Web Application URL: https://tt-dev-web-xxxx.azurewebsites.net
```

### 4. Verify Deployment

```bash
# Get the web app URL from deployment output
WEB_APP_URL="<url-from-output>"

# Test the application
curl -I $WEB_APP_URL

# View logs
az webapp log tail \
  --name tt-dev-web-xxxx \
  --resource-group rg-timetracker-dev
```

## Common Commands

### Update Application

```bash
# Build new version
./build-and-push.sh -u $DOCKERHUB_USERNAME -t v1.1.0

# Deploy update
./deploy.sh -e dev --skip-bicep
```

### View Logs

```bash
az webapp log tail \
  --name <webapp-name> \
  --resource-group <resource-group>
```

### Restart Application

```bash
az webapp restart \
  --name <webapp-name> \
  --resource-group <resource-group>
```

### Delete Resources

```bash
az group delete \
  --name rg-timetracker-dev \
  --yes --no-wait
```

## Troubleshooting

### Issue: Docker login fails
```bash
# Solution: Use access token, not password
# Generate new token at: https://hub.docker.com/settings/security
```

### Issue: Image not found
```bash
# Check if image exists on Docker Hub
docker pull $DOCKERHUB_USERNAME/timetracker:latest

# Or rebuild and push
./build-and-push.sh -u $DOCKERHUB_USERNAME -t latest
```

### Issue: App Service won't start
```bash
# Check container logs
az webapp log tail --name <webapp> --resource-group <rg>

# Verify image works locally
docker run -p 8080:8080 $DOCKERHUB_USERNAME/timetracker:latest
```

## Documentation

For detailed information, see:

- **[Complete Deployment Guide](docs/DEPLOYMENT.md)** - Full deployment instructions
- **[Architecture Overview](docs/ARCHITECTURE.md)** - System architecture
- **[Docker Hub Quick Reference](docs/DOCKERHUB.md)** - Docker Hub operations
- **[Implementation Plan](plan4.md)** - Migration strategy and details
- **[Migration Summary](DOCKERHUB_MIGRATION.md)** - Change summary

## Quick Links

- **Docker Hub**: https://hub.docker.com
- **Azure Portal**: https://portal.azure.com
- **Application Insights**: Azure Portal → Monitor → Application Insights
- **Logs**: Azure Portal → App Service → Log stream

## Cost Estimate

| Environment | Monthly Cost | Notes |
|-------------|--------------|-------|
| Development | ~$35-40 | B1 App Service + B1ms PostgreSQL |
| Staging | ~$100-120 | S1 App Service + B1ms PostgreSQL |
| Production | ~$180-220 | P1V3 App Service + D2s PostgreSQL |

**Docker Hub**: $0 (free public repo) or $5/month (Pro with private repos)

**Savings vs ACR**: ~$45/month (~$540/year)

## Support

- **Issues**: See [TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md)
- **Questions**: Check [plan4.md](plan4.md) for implementation details
- **Updates**: Follow [DOCKERHUB_MIGRATION.md](DOCKERHUB_MIGRATION.md)

---

**Last Updated**: January 25, 2026  
**Version**: 1.0.0 (Docker Hub Migration)  
**Status**: ✅ Ready for Deployment
