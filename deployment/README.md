# Time Tracker - Azure Deployment

Complete Infrastructure as Code (IaC) deployment for the Time Tracker application using Azure Bicep templates and the Deployment Stamps pattern.

## 🚀 Quick Start

```bash
# Set required environment variable
export POSTGRESQL_ADMIN_PASSWORD="YourSecurePassword123!"

# Deploy to development
cd deployment/scripts
./deploy.sh --environment dev --location eastus --run-migrations

# Your application will be available at the URL shown in the deployment output
```

## 📋 Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Deployment Options](#deployment-options)
- [Documentation](#documentation)
- [Cost Estimates](#cost-estimates)
- [Support](#support)

## Overview

This deployment creates a complete, production-ready infrastructure on Azure including:

- **App Service**: Containerized .NET 10.0 application
- **PostgreSQL**: Managed database with VNet integration
- **Key Vault**: Secure secret management
- **Container Registry**: Private Docker registry
- **Monitoring**: Application Insights & Log Analytics
- **Networking**: Virtual Network with subnet isolation

### Architecture

```
┌─────────────────────────────────────────────────┐
│              Azure Resource Group                │
│                                                  │
│  ┌────────────┐    ┌──────────────┐            │
│  │ App Service│────│  PostgreSQL  │            │
│  │  (Docker)  │    │  (Private)   │            │
│  └──────┬─────┘    └──────────────┘            │
│         │                                        │
│         ▼                                        │
│  ┌────────────┐    ┌──────────────┐            │
│  │ Key Vault  │    │     ACR      │            │
│  │ (Secrets)  │    │  (Images)    │            │
│  └────────────┘    └──────────────┘            │
│                                                  │
│  ┌────────────┐    ┌──────────────┐            │
│  │    Log     │    │  App Insights│            │
│  │ Analytics  │    │ (Monitoring) │            │
│  └────────────┘    └──────────────┘            │
└─────────────────────────────────────────────────┘
```

## Prerequisites

### Required Tools

| Tool | Version | Installation |
|------|---------|--------------|
| Azure CLI | 2.50.0+ | `curl -sL https://aka.ms/InstallAzureCLIDeb \| sudo bash` |
| Docker | 20.10+ | `curl -fsSL https://get.docker.com \| sh` |
| .NET SDK | 10.0 | [Download](https://dotnet.microsoft.com/download) |
| jq | latest | `sudo apt-get install -y jq` |

### Azure Requirements

- Active Azure subscription
- Permissions: Owner or Contributor role
- Azure CLI logged in: `az login`

## Project Structure

```
deployment/
├── bicep/                          # Infrastructure as Code
│   ├── main.bicep                  # Main orchestration template
│   ├── modules/                    # Modular Bicep files
│   │   ├── vnet.bicep             # Virtual Network
│   │   ├── keyvault.bicep         # Key Vault
│   │   ├── postgresql.bicep       # PostgreSQL Server
│   │   ├── acr.bicep              # Container Registry
│   │   ├── appservice.bicep       # App Service & Plan
│   │   └── monitoring.bicep       # Log Analytics & App Insights
│   └── parameters/                 # Environment parameters
│       ├── dev.bicepparam         # Development
│       ├── staging.bicepparam     # Staging
│       └── prod.bicepparam        # Production
│
├── scripts/                        # Deployment automation
│   ├── deploy.sh                  # Main deployment script
│   ├── build-and-push.sh          # Docker build & push
│   ├── run-migrations.sh          # Database migrations
│   └── rollback.sh                # Rollback procedures
│
├── docker/                         # Docker configuration
│   ├── Dockerfile.production      # Optimized Dockerfile
│   └── .dockerignore              # Build exclusions
│
└── docs/                           # Documentation
    ├── DEPLOYMENT.md              # Step-by-step guide
    ├── ARCHITECTURE.md            # Architecture details
    └── TROUBLESHOOTING.md         # Common issues & solutions
```

## Deployment Options

### Development Environment

Minimal resources for development and testing.

```bash
export POSTGRESQL_ADMIN_PASSWORD="DevPassword123!"
./scripts/deploy.sh --environment dev --location eastus
```

**Resources Created**:
- App Service Plan: B1 (Basic)
- PostgreSQL: Burstable B1ms
- Cost: ~$28/month

### Staging Environment

Pre-production environment for validation.

```bash
export POSTGRESQL_ADMIN_PASSWORD="StagingPassword123!"
./scripts/deploy.sh --environment staging --location eastus --run-migrations
```

**Resources Created**:
- App Service Plan: S1 (Standard)
- PostgreSQL: Burstable B1ms
- Cost: ~$90/month

### Production Environment

Production-ready with high availability and auto-scaling.

```bash
export POSTGRESQL_ADMIN_PASSWORD="$(openssl rand -base64 32)"
export DOCKER_IMAGE_TAG="v1.0.0"
./scripts/deploy.sh --environment prod --location westus2 --run-migrations
```

**Resources Created**:
- App Service Plan: P1V3 (Premium)
- PostgreSQL: General Purpose D2s_v3
- Geo-redundant backups
- Cost: ~$278/month

### Advanced Options

```bash
# Skip infrastructure deployment (update app only)
./scripts/deploy.sh --environment dev --skip-bicep --run-migrations

# Skip Docker build (use existing image)
./scripts/deploy.sh --environment dev --skip-build

# Custom resource group name
./scripts/deploy.sh --environment prod \
  --resource-group my-custom-rg \
  --location eastus2
```

## Key Features

### 🔒 Security

- **Managed Identity**: No credentials in code
- **Key Vault Integration**: All secrets centrally managed
- **Private Networking**: Database not exposed to internet
- **TLS/SSL**: All connections encrypted
- **RBAC**: Role-based access control

### 📈 Scalability

- **Auto-scaling**: Horizontal scaling based on CPU/memory
- **Deployment Stamps**: Multiple isolated environments
- **Database Auto-grow**: Storage scales automatically
- **Load Balancing**: Built into App Service

### 🔍 Monitoring

- **Application Insights**: Performance monitoring
- **Log Analytics**: Centralized logging
- **Health Checks**: /health endpoint
- **Alerts**: Automated notifications

### 🛡️ Reliability

- **Automated Backups**: 7-35 day retention
- **Geo-redundancy**: Production database replication
- **Deployment Slots**: Zero-downtime deployments
- **Rollback**: Quick recovery procedures

## Documentation

Comprehensive documentation is available in the [docs](./docs/) folder:

| Document | Description |
|----------|-------------|
| [DEPLOYMENT.md](./docs/DEPLOYMENT.md) | Complete deployment guide with step-by-step instructions |
| [ARCHITECTURE.md](./docs/ARCHITECTURE.md) | Detailed architecture documentation and design decisions |
| [TROUBLESHOOTING.md](./docs/TROUBLESHOOTING.md) | Common issues and solutions |
| [plan2.md](./plan2.md) | Refined deployment plan and strategy |

## Cost Estimates

### Monthly Costs by Environment

| Environment | App Service | Database | Other | Total |
|-------------|-------------|----------|-------|-------|
| Development | $13 (B1) | $12 (B1ms) | $3 | **~$28** |
| Staging | $70 (S1) | $12 (B1ms) | $8 | **~$90** |
| Production | $115 (P1V3) | $150 (D2s) | $13 | **~$278** |

### Cost Optimization Tips

- ✅ Use **Azure Dev/Test** pricing for non-production
- ✅ Consider **Reserved Instances** (save 30-70%)
- ✅ Enable **auto-shutdown** for dev environments
- ✅ Right-size resources based on actual usage
- ✅ Use **Burstable SKUs** for variable workloads

## Common Commands

### View Deployment Status

```bash
# Get resource group information
az group show --name rg-timetracker-dev

# List all resources
az resource list --resource-group rg-timetracker-dev --output table

# Check web app status
az webapp show --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev \
  --query "state"
```

### View Logs

```bash
# Stream live logs
az webapp log tail \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# Download logs
az webapp log download \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev \
  --log-file logs.zip
```

### Manage Application

```bash
# Restart application
az webapp restart \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# SSH into container
az webapp ssh \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# Scale out (add instances)
az appservice plan update \
  --name tt-dev-asp \
  --resource-group rg-timetracker-dev \
  --number-of-workers 2
```

### Build and Deploy

```bash
# Build and push new image
./scripts/build-and-push.sh \
  --resource-group rg-timetracker-dev \
  --tag v1.1.0

# Run migrations
./scripts/run-migrations.sh \
  --resource-group rg-timetracker-dev

# Rollback to previous version
./scripts/rollback.sh \
  --resource-group rg-timetracker-dev \
  --tag v1.0.0
```

## Deployment Stamps Pattern

This deployment follows the [Deployment Stamps Pattern](https://learn.microsoft.com/azure/architecture/patterns/deployment-stamp), where each "stamp" is a complete, isolated unit of deployment.

### Benefits

✅ **Isolation**: Issues in one stamp don't affect others
✅ **Scalability**: Add stamps to handle more load
✅ **Geographic Distribution**: Deploy stamps in different regions
✅ **Testing**: Separate stamps for dev/staging/prod
✅ **Blue-Green Deployments**: Deploy to new stamp, switch traffic

### Example Multi-Region Deployment

```bash
# Deploy to East US
./scripts/deploy.sh --environment prod \
  --resource-group rg-timetracker-prod-eastus \
  --location eastus

# Deploy to West US
./scripts/deploy.sh --environment prod \
  --resource-group rg-timetracker-prod-westus \
  --location westus2

# Use Azure Front Door for global load balancing
```

## Environment Variables

| Variable | Required | Description | Example |
|----------|----------|-------------|---------|
| `POSTGRESQL_ADMIN_PASSWORD` | Yes | PostgreSQL admin password | `SecurePass123!` |
| `DOCKER_IMAGE_TAG` | No | Docker image tag | `v1.0.0` (default: timestamp) |

## Verification Checklist

After deployment, verify:

- [ ] Web app is accessible and returns HTTP 200
- [ ] Health endpoint `/health` returns "Healthy"
- [ ] Database connection is working
- [ ] Key Vault secrets are accessible
- [ ] Application Insights is receiving telemetry
- [ ] Logs are streaming to Log Analytics

```bash
# Quick verification script
WEB_APP_URL=$(az webapp show --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev \
  --query defaultHostName -o tsv)

echo "Testing application..."
curl -f "https://${WEB_APP_URL}" && echo "✅ App is running"
curl -f "https://${WEB_APP_URL}/health" && echo "✅ Health check passed"
```

## Cleanup

To remove all resources:

```bash
# WARNING: This deletes EVERYTHING in the resource group!
az group delete \
  --name rg-timetracker-dev \
  --yes --no-wait

# Check deletion status
az group show --name rg-timetracker-dev
```

## CI/CD Integration

### GitHub Actions

```yaml
name: Deploy to Azure
on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      
      - name: Azure Login
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      
      - name: Deploy
        env:
          POSTGRESQL_ADMIN_PASSWORD: ${{ secrets.DB_PASSWORD }}
        run: |
          cd deployment/scripts
          ./deploy.sh --environment prod --skip-bicep
```

### Azure DevOps

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: AzureCLI@2
    inputs:
      azureSubscription: 'Azure-Connection'
      scriptType: 'bash'
      scriptLocation: 'scriptPath'
      scriptPath: 'deployment/scripts/deploy.sh'
      arguments: '--environment prod'
    env:
      POSTGRESQL_ADMIN_PASSWORD: $(DB_PASSWORD)
```

## Security Best Practices

1. ✅ **Never commit secrets** to source control
2. ✅ **Use Key Vault** for all credentials
3. ✅ **Enable managed identities** instead of service principals
4. ✅ **Use private networking** for database access
5. ✅ **Enable diagnostic logging** for audit trails
6. ✅ **Regularly rotate passwords** and secrets
7. ✅ **Review access policies** periodically
8. ✅ **Enable Azure Security Center** recommendations

## Troubleshooting

For common issues, see [TROUBLESHOOTING.md](./docs/TROUBLESHOOTING.md).

Quick diagnostics:

```bash
# Check deployment errors
az deployment group list \
  --resource-group rg-timetracker-dev \
  --query "[?properties.provisioningState=='Failed']"

# View application logs
az webapp log tail \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# Test database connectivity
az webapp ssh \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev
```

## Support

- 📚 [Azure Documentation](https://docs.microsoft.com/azure/)
- 📝 [Bicep Documentation](https://docs.microsoft.com/azure/azure-resource-manager/bicep/)
- 🏗️ [Architecture Center](https://docs.microsoft.com/azure/architecture/)
- 💬 [GitHub Issues](https://github.com/michaelprosario/timeTracker/issues)

## License

This project is licensed under the MIT License - see the LICENSE file for details.

---

**Made with ❤️ for Azure deployments**

For questions or support, please open an issue on GitHub.
