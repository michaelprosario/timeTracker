# Time Tracker Deployment Guide

This guide provides step-by-step instructions for deploying the Time Tracker application to Microsoft Azure using Bicep Infrastructure as Code templates.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Architecture Overview](#architecture-overview)
3. [Deployment Steps](#deployment-steps)
4. [Post-Deployment Configuration](#post-deployment-configuration)
5. [Verification](#verification)
6. [Troubleshooting](#troubleshooting)
7. [Rollback Procedures](#rollback-procedures)

---

## Prerequisites

### Required Tools

1. **Azure CLI** (version 2.50.0 or later)
   ```bash
   # Install Azure CLI
   curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
   
   # Verify installation
   az --version
   ```

2. **Docker** (version 20.10 or later)
   ```bash
   # Install Docker
   curl -fsSL https://get.docker.com -o get-docker.sh
   sudo sh get-docker.sh
   
   # Verify installation
   docker --version
   ```

3. **.NET SDK 10.0** (for local testing)
   ```bash
   # Install .NET SDK
   wget https://dot.net/v1/dotnet-install.sh
   chmod +x dotnet-install.sh
   ./dotnet-install.sh --channel 10.0
   
   # Verify installation
   dotnet --version
   ```

4. **jq** (for JSON parsing in scripts)
   ```bash
   sudo apt-get install jq -y
   ```

### Azure Subscription

- Active Azure subscription with appropriate permissions
- Permissions required:
  - Create resource groups
  - Deploy resources (App Service, PostgreSQL, Key Vault, etc.)
  - Assign roles (for managed identity)

### Azure Login

```bash
# Login to Azure
az login

# List available subscriptions
az account list --output table

# Set the subscription you want to use
az account set --subscription "YOUR_SUBSCRIPTION_ID"

# Verify current subscription
az account show
```

---

## Architecture Overview

The deployment creates the following resources following the **Deployment Stamps Pattern**:

### Infrastructure Components

```
┌─────────────────────────────────────────────────────────┐
│                    Resource Group                        │
│  ┌────────────────────────────────────────────────┐    │
│  │  Virtual Network (10.0.0.0/16)                 │    │
│  │  ├─ App Service Subnet (10.0.1.0/24)          │    │
│  │  ├─ PostgreSQL Subnet (10.0.2.0/24)           │    │
│  │  └─ Private Endpoints Subnet (10.0.3.0/24)    │    │
│  └────────────────────────────────────────────────┘    │
│                                                          │
│  ┌──────────────────┐  ┌────────────────────────┐     │
│  │  App Service     │  │  PostgreSQL Flexible   │     │
│  │  (Linux)         │──│  Server                │     │
│  │  + Managed ID    │  │  (Private VNet)        │     │
│  └──────────────────┘  └────────────────────────┘     │
│           │                                             │
│           ▼                                             │
│  ┌──────────────────┐  ┌────────────────────────┐     │
│  │  Azure Key Vault │  │  Container Registry    │     │
│  │  (Secrets)       │  │  (ACR)                 │     │
│  └──────────────────┘  └────────────────────────┘     │
│                                                          │
│  ┌──────────────────┐  ┌────────────────────────┐     │
│  │  Log Analytics   │  │  Application Insights  │     │
│  └──────────────────┘  └────────────────────────┘     │
└─────────────────────────────────────────────────────────┘
```

### Security Features

1. **Managed Identity**: App Service uses system-assigned managed identity
2. **Key Vault Integration**: All secrets stored in Azure Key Vault
3. **VNet Integration**: App Service and PostgreSQL communicate over private network
4. **No Public Database Access**: PostgreSQL only accessible from App Service subnet
5. **TLS/SSL**: All connections encrypted in transit
6. **RBAC**: Role-based access control for all resources

---

## Deployment Steps

### Step 1: Clone the Repository

```bash
git clone https://github.com/michaelprosario/timeTracker.git
cd timeTracker
```

### Step 2: Set Environment Variables

```bash
# Set PostgreSQL admin password (required)
export POSTGRESQL_ADMIN_PASSWORD="YourSecurePassword123!"

# Optional: Set Docker image tag (defaults to timestamp)
export DOCKER_IMAGE_TAG="v1.0.0"
```

**Security Note**: Never commit passwords to source control. Use environment variables or Azure Key Vault for all secrets.

### Step 3: Run the Deployment Script

#### Development Environment

```bash
cd deployment/scripts
./deploy.sh --environment dev --location eastus
```

This will:
1. Create resource group `rg-timetracker-dev`
2. Deploy all infrastructure using Bicep
3. Build Docker image
4. Push image to Azure Container Registry
5. Deploy application to App Service
6. Configure Key Vault secrets

#### Staging Environment

```bash
./deploy.sh --environment staging --location eastus --run-migrations
```

#### Production Environment

```bash
./deploy.sh --environment prod --location westus2 --run-migrations
```

### Step 4: Wait for Deployment

The deployment process takes approximately 15-20 minutes:

- Infrastructure provisioning: ~10 minutes
- Docker build and push: ~5 minutes
- Application startup: ~2 minutes

You'll see output like:

```
[INFO] ========================================
[INFO] Time Tracker Azure Deployment
[INFO] ========================================
[INFO] Environment:      dev
[INFO] Resource Group:   rg-timetracker-dev
[INFO] Location:         eastus
[INFO] Docker Tag:       20260124-150000
[INFO] ========================================
[SUCCESS] All prerequisites are met
[SUCCESS] Using Azure subscription: xxxxx-xxxx-xxxx-xxxx
[SUCCESS] Resource group ready
[INFO] Deploying infrastructure with Bicep...
[SUCCESS] Infrastructure deployed successfully
[INFO] Building Docker image...
[SUCCESS] Docker image built successfully
[SUCCESS] Image pushed to ACR
[SUCCESS] ========================================
[SUCCESS] Deployment Complete!
[INFO] ========================================
```

### Step 5: Note the Deployment Output

The script will save deployment information to `deployment-info.txt`:

```
Deployment Information
=====================
Timestamp: Fri Jan 24 15:00:00 UTC 2026
Environment: dev
Resource Group: rg-timetracker-dev
Location: eastus
Docker Image Tag: 20260124-150000

Resources:
- Web App: tt-dev-web-abc123
- Web App URL: https://tt-dev-web-abc123.azurewebsites.net
- ACR: acrdevabc123
- ACR Login Server: acrdevabc123.azurecr.io
```

---

## Post-Deployment Configuration

### Step 1: Run Database Migrations

The deployment script can run migrations automatically with the `--run-migrations` flag, or you can run them manually:

```bash
cd deployment/scripts
./run-migrations.sh --resource-group rg-timetracker-dev
```

### Step 2: Verify Database Connection

```bash
# Check application logs
az webapp log tail \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev
```

Look for log entries indicating successful database connection:

```
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (123ms) [Parameters=[], CommandType='Text']
      SELECT 1
```

### Step 3: Configure Custom Domain (Optional)

```bash
# Add custom domain
az webapp config hostname add \
  --webapp-name tt-prod-web-abc123 \
  --resource-group rg-timetracker-prod \
  --hostname timetracker.yourdomain.com

# Bind SSL certificate
az webapp config ssl bind \
  --certificate-thumbprint <thumbprint> \
  --ssl-type SNI \
  --name tt-prod-web-abc123 \
  --resource-group rg-timetracker-prod
```

### Step 4: Configure Scaling (Production)

```bash
# Configure auto-scaling
az monitor autoscale create \
  --resource-group rg-timetracker-prod \
  --resource tt-prod-asp \
  --resource-type Microsoft.Web/serverFarms \
  --name autoscale-prod \
  --min-count 2 \
  --max-count 10 \
  --count 2

# Add CPU-based scaling rule
az monitor autoscale rule create \
  --resource-group rg-timetracker-prod \
  --autoscale-name autoscale-prod \
  --condition "Percentage CPU > 70 avg 5m" \
  --scale out 1
```

---

## Verification

### 1. Check Web Application

```bash
# Get the URL
WEB_APP_URL=$(az webapp show \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev \
  --query defaultHostName -o tsv)

echo "Application URL: https://${WEB_APP_URL}"

# Test the application
curl -I "https://${WEB_APP_URL}"
```

Expected response:
```
HTTP/1.1 200 OK
Content-Type: text/html; charset=utf-8
```

### 2. Check Health Endpoint

```bash
curl "https://${WEB_APP_URL}/health"
```

Expected response:
```
Healthy
```

### 3. Verify Key Vault Integration

```bash
# List Key Vault secrets
az keyvault secret list \
  --vault-name kv-dev-abc123 \
  --query "[].name" -o table
```

Expected output:
```
Result
--------------------------------
ConnectionStrings--DefaultConnection
PostgreSQL--AdminPassword
ACR--Password
```

### 4. Check Database Connectivity

```bash
# SSH into the container
az webapp ssh \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# Inside container, check database
dotnet ef database update --project /app
```

### 5. Monitor Application Performance

```bash
# View Application Insights metrics
az monitor app-insights metrics show \
  --app tt-dev-ai \
  --resource-group rg-timetracker-dev \
  --metric requests/count \
  --aggregation count
```

---

## Troubleshooting

### Common Issues

#### 1. Application Won't Start

**Symptoms**: Application shows "Service Unavailable" or 503 error

**Solutions**:
```bash
# Check application logs
az webapp log tail \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# Restart the application
az webapp restart \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# Check container logs
az webapp log download \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev \
  --log-file logs.zip
```

#### 2. Database Connection Fails

**Symptoms**: "Npgsql.NpgsqlException: Connection refused"

**Solutions**:
```bash
# Verify connection string in Key Vault
az keyvault secret show \
  --vault-name kv-dev-abc123 \
  --name ConnectionStrings--DefaultConnection

# Check PostgreSQL firewall rules
az postgres flexible-server firewall-rule list \
  --name tt-dev-postgres-abc123 \
  --resource-group rg-timetracker-dev

# Verify VNet integration
az webapp vnet-integration list \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev
```

#### 3. Key Vault Access Denied

**Symptoms**: "Access denied" when reading secrets

**Solutions**:
```bash
# Verify managed identity is enabled
az webapp identity show \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# Check Key Vault access policies
az keyvault show \
  --name kv-dev-abc123 \
  --query properties.enableRbacAuthorization

# Assign Key Vault Secrets User role
PRINCIPAL_ID=$(az webapp identity show \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev \
  --query principalId -o tsv)

az role assignment create \
  --role "Key Vault Secrets User" \
  --assignee $PRINCIPAL_ID \
  --scope /subscriptions/SUBSCRIPTION_ID/resourceGroups/rg-timetracker-dev/providers/Microsoft.KeyVault/vaults/kv-dev-abc123
```

#### 4. Container Registry Pull Failed

**Symptoms**: "Failed to pull image from ACR"

**Solutions**:
```bash
# Check ACR credentials
az acr credential show \
  --name acrdevabc123

# Update app settings with correct ACR credentials
az webapp config appsettings set \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev \
  --settings DOCKER_REGISTRY_SERVER_USERNAME=acrdevabc123 \
              DOCKER_REGISTRY_SERVER_PASSWORD="$(az acr credential show --name acrdevabc123 --query passwords[0].value -o tsv)"

# Restart webapp
az webapp restart \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev
```

### Diagnostic Commands

```bash
# Get all resources in resource group
az resource list \
  --resource-group rg-timetracker-dev \
  --output table

# Check App Service configuration
az webapp config show \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# View App Service diagnostics
az webapp log show \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# Get PostgreSQL server status
az postgres flexible-server show \
  --name tt-dev-postgres-abc123 \
  --resource-group rg-timetracker-dev \
  --query state
```

---

## Rollback Procedures

### Rollback to Previous Docker Image

```bash
cd deployment/scripts
./rollback.sh \
  --resource-group rg-timetracker-prod \
  --tag 20260124-140000
```

### Rollback Using Deployment Slots (Recommended for Production)

```bash
# Create a staging slot
az webapp deployment slot create \
  --name tt-prod-web-abc123 \
  --resource-group rg-timetracker-prod \
  --slot staging

# Deploy to staging slot first
./deploy.sh \
  --environment prod \
  --skip-bicep

# Test staging
curl https://tt-prod-web-abc123-staging.azurewebsites.net/health

# Swap staging with production
az webapp deployment slot swap \
  --name tt-prod-web-abc123 \
  --resource-group rg-timetracker-prod \
  --slot staging

# If issues occur, swap back immediately
./rollback.sh \
  --resource-group rg-timetracker-prod \
  --use-slot
```

### Rollback Database Migrations

```bash
# SSH into container
az webapp ssh \
  --name tt-prod-web-abc123 \
  --resource-group rg-timetracker-prod

# Inside container, rollback to specific migration
dotnet ef database update PreviousMigrationName --project /app
```

### Complete Infrastructure Rollback

If you need to completely remove and redeploy:

```bash
# Delete resource group (WARNING: This deletes everything!)
az group delete \
  --name rg-timetracker-dev \
  --yes --no-wait

# Redeploy from scratch
./deploy.sh --environment dev --location eastus
```

---

## Additional Resources

- [Azure App Service Documentation](https://docs.microsoft.com/azure/app-service/)
- [Azure Database for PostgreSQL](https://docs.microsoft.com/azure/postgresql/)
- [Azure Key Vault](https://docs.microsoft.com/azure/key-vault/)
- [Bicep Documentation](https://docs.microsoft.com/azure/azure-resource-manager/bicep/)
- [Deployment Stamps Pattern](https://docs.microsoft.com/azure/architecture/patterns/deployment-stamp)

---

## Support

For issues or questions:
1. Check the [Troubleshooting](#troubleshooting) section
2. Review Azure Portal diagnostics
3. Contact the development team
4. Open an issue on GitHub
