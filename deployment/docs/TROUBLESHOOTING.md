# Troubleshooting Guide - Time Tracker Azure Deployment

This guide helps you diagnose and resolve common issues when deploying and running the Time Tracker application on Azure.

## Table of Contents

1. [Deployment Issues](#deployment-issues)
2. [Application Runtime Issues](#application-runtime-issues)
3. [Database Connectivity Issues](#database-connectivity-issues)
4. [Container Registry Issues](#container-registry-issues)
5. [Key Vault Issues](#key-vault-issues)
6. [Networking Issues](#networking-issues)
7. [Performance Issues](#performance-issues)
8. [Monitoring and Logging](#monitoring-and-logging)

---

## Deployment Issues

### Issue: Bicep Deployment Fails

**Symptoms**:
```
ERROR: Deployment failed. Correlation ID: xxxxx-xxxx-xxxx-xxxx
```

**Diagnostic Steps**:
```bash
# Get deployment details
az deployment group show \
  --name timetracker-dev-20260124-150000 \
  --resource-group rg-timetracker-dev

# Check deployment operations
az deployment operation group list \
  --name timetracker-dev-20260124-150000 \
  --resource-group rg-timetracker-dev \
  --query "[?properties.provisioningState=='Failed']"
```

**Common Causes & Solutions**:

1. **Resource name already exists**
   ```
   Error: The storage account 'kvdevabc123' is already taken
   ```
   Solution: Delete the existing resource or use a different name
   ```bash
   az keyvault delete --name kv-dev-abc123 --resource-group rg-timetracker-dev
   az keyvault purge --name kv-dev-abc123  # If soft-delete is enabled
   ```

2. **Insufficient permissions**
   ```
   Error: The client does not have authorization to perform action
   ```
   Solution: Ensure you have Owner or Contributor role
   ```bash
   # Check your role assignments
   az role assignment list --assignee $(az account show --query user.name -o tsv)
   
   # Request access from subscription admin
   ```

3. **Quota exceeded**
   ```
   Error: Quota 'standardBSFamily' exceeded for region 'eastus'
   ```
   Solution: Choose a different SKU or request quota increase
   ```bash
   # Check current quota
   az vm list-usage --location eastus -o table
   
   # Request quota increase through Azure Portal
   ```

### Issue: Deployment Script Permission Denied

**Symptoms**:
```bash
./deploy.sh: Permission denied
```

**Solution**:
```bash
# Make scripts executable
chmod +x deployment/scripts/*.sh

# Or run with bash explicitly
bash deployment/scripts/deploy.sh --environment dev
```

### Issue: jq Command Not Found

**Symptoms**:
```
./deploy.sh: line 123: jq: command not found
```

**Solution**:
```bash
# Install jq
sudo apt-get update && sudo apt-get install -y jq
```

---

## Application Runtime Issues

### Issue: Application Shows 503 Service Unavailable

**Symptoms**:
- Web app returns 503 error
- Azure Portal shows "Container not ready"

**Diagnostic Steps**:
```bash
# Check container logs
az webapp log tail \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# Check container status
az webapp show \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev \
  --query "state"

# Download full logs
az webapp log download \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev \
  --log-file app-logs.zip
```

**Common Causes & Solutions**:

1. **Container failed to start**
   ```
   Error: Failed to pull image from registry
   ```
   Solution: Check ACR credentials
   ```bash
   # Verify ACR credentials in app settings
   az webapp config appsettings list \
     --name tt-dev-web-abc123 \
     --resource-group rg-timetracker-dev \
     --query "[?name=='DOCKER_REGISTRY_SERVER_PASSWORD']"
   
   # Update credentials
   ACR_PASSWORD=$(az acr credential show \
     --name acrdevabc123 \
     --query passwords[0].value -o tsv)
   
   az webapp config appsettings set \
     --name tt-dev-web-abc123 \
     --resource-group rg-timetracker-dev \
     --settings DOCKER_REGISTRY_SERVER_PASSWORD="$ACR_PASSWORD"
   ```

2. **Application port mismatch**
   ```
   Error: Container didn't respond to HTTP pings on port: 80
   ```
   Solution: Ensure WEBSITES_PORT is set correctly
   ```bash
   az webapp config appsettings set \
     --name tt-dev-web-abc123 \
     --resource-group rg-timetracker-dev \
     --settings WEBSITES_PORT=8080
   ```

3. **Application startup timeout**
   ```
   Error: Container didn't start within expected time
   ```
   Solution: Increase startup timeout or check application startup code
   ```bash
   # Increase timeout to 600 seconds
   az webapp config set \
     --name tt-dev-web-abc123 \
     --resource-group rg-timetracker-dev \
     --startup-time 600
   ```

### Issue: Application Crashes After Starting

**Symptoms**:
- Container starts but then exits
- Logs show application error

**Diagnostic Steps**:
```bash
# SSH into container
az webapp ssh \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# Inside container, check process
ps aux | grep dotnet

# Check application logs
cat /home/LogFiles/Application/*.log
```

**Common Causes**:

1. **Database connection failure** - See [Database Connectivity Issues](#database-connectivity-issues)
2. **Missing environment variables** - Check app settings
3. **Unhandled exception** - Review application logs

---

## Database Connectivity Issues

### Issue: Unable to Connect to PostgreSQL

**Symptoms**:
```
Npgsql.NpgsqlException: Connection refused
or
System.TimeoutException: Timeout during connection attempt
```

**Diagnostic Steps**:
```bash
# 1. Check PostgreSQL server status
az postgres flexible-server show \
  --name tt-dev-postgres-abc123 \
  --resource-group rg-timetracker-dev \
  --query state

# 2. Check connection string
az webapp config connection-string list \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# 3. Check firewall rules
az postgres flexible-server firewall-rule list \
  --name tt-dev-postgres-abc123 \
  --resource-group rg-timetracker-dev

# 4. Test connection from App Service
az webapp ssh \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# Inside container:
apt-get update && apt-get install -y postgresql-client
psql "Host=tt-dev-postgres-abc123.postgres.database.azure.com;Database=timetracker;Username=ttadmin;Password=YourPassword;SSL Mode=Require"
```

**Common Causes & Solutions**:

1. **VNet configuration issue**
   ```bash
   # Verify VNet integration
   az webapp vnet-integration list \
     --name tt-dev-web-abc123 \
     --resource-group rg-timetracker-dev
   
   # If not integrated, add integration
   SUBNET_ID=$(az network vnet subnet show \
     --resource-group rg-timetracker-dev \
     --vnet-name tt-dev-vnet \
     --name appservice-subnet \
     --query id -o tsv)
   
   az webapp vnet-integration add \
     --name tt-dev-web-abc123 \
     --resource-group rg-timetracker-dev \
     --vnet $SUBNET_ID \
     --subnet appservice-subnet
   ```

2. **Incorrect connection string**
   ```bash
   # Get connection string from Key Vault
   az keyvault secret show \
     --vault-name kv-dev-abc123 \
     --name ConnectionStrings--DefaultConnection \
     --query value -o tsv
   
   # Verify format:
   # Host=server.postgres.database.azure.com;Database=timetracker;Username=ttadmin;Password=xxx;SSL Mode=Require
   ```

3. **PostgreSQL server stopped**
   ```bash
   # Start the server
   az postgres flexible-server start \
     --name tt-dev-postgres-abc123 \
     --resource-group rg-timetracker-dev
   ```

4. **SSL/TLS configuration issue**
   ```bash
   # Update connection string to include SSL settings
   ConnectionString="Host=...;SSL Mode=Require;Trust Server Certificate=true"
   ```

### Issue: Database Migrations Fail

**Symptoms**:
```
An error occurred while applying migrations
```

**Diagnostic Steps**:
```bash
# SSH into container
az webapp ssh \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# Check EF tools
dotnet ef --version

# List pending migrations
cd /app
dotnet ef migrations list
```

**Solutions**:

1. **Run migrations manually**
   ```bash
   # From within container
   cd /app
   dotnet ef database update --verbose
   
   # Or from local machine (requires connection string)
   cd src/TimeTracker.Infrastructure
   dotnet ef database update --startup-project ../TimeTracker.Web \
     --connection "Host=...;Database=...;Username=...;Password=..."
   ```

2. **Rollback to previous migration**
   ```bash
   dotnet ef database update PreviousMigrationName
   ```

---

## Container Registry Issues

### Issue: Cannot Push Image to ACR

**Symptoms**:
```
Error response from daemon: unauthorized: authentication required
```

**Solution**:
```bash
# Login to ACR
az acr login --name acrdevabc123

# If still failing, use admin credentials
ACR_USERNAME=$(az acr show --name acrdevabc123 --query name -o tsv)
ACR_PASSWORD=$(az acr credential show --name acrdevabc123 --query passwords[0].value -o tsv)

docker login acrdevabc123.azurecr.io \
  --username $ACR_USERNAME \
  --password $ACR_PASSWORD
```

### Issue: Image Not Found in ACR

**Symptoms**:
```
Error: manifest for image not found
```

**Diagnostic Steps**:
```bash
# List all repositories
az acr repository list --name acrdevabc123

# List tags for repository
az acr repository show-tags \
  --name acrdevabc123 \
  --repository timetracker
```

**Solution**:
```bash
# Build and push image
./deployment/scripts/build-and-push.sh \
  --resource-group rg-timetracker-dev \
  --tag latest
```

### Issue: ACR Rate Limiting

**Symptoms**:
```
Error: Too many requests
```

**Solution**:
Upgrade ACR SKU:
```bash
az acr update \
  --name acrdevabc123 \
  --sku Standard
```

---

## Key Vault Issues

### Issue: Access Denied to Key Vault

**Symptoms**:
```
Azure.RequestFailedException: The user, group or application 'xxx' does not have secrets get permission
```

**Diagnostic Steps**:
```bash
# Check if managed identity is enabled
az webapp identity show \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# Get principal ID
PRINCIPAL_ID=$(az webapp identity show \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev \
  --query principalId -o tsv)

# Check role assignments
az role assignment list \
  --assignee $PRINCIPAL_ID \
  --all
```

**Solution**:
```bash
# Assign Key Vault Secrets User role
az role assignment create \
  --role "Key Vault Secrets User" \
  --assignee $PRINCIPAL_ID \
  --scope $(az keyvault show --name kv-dev-abc123 --query id -o tsv)

# Wait a few minutes for propagation, then restart app
az webapp restart \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev
```

### Issue: Key Vault Reference Not Resolving

**Symptoms**:
App settings show `@Microsoft.KeyVault(...)` instead of actual value

**Diagnostic Steps**:
```bash
# Check Key Vault reference status
az webapp config appsettings list \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev
```

**Solutions**:

1. **Verify reference format**
   ```
   Correct: @Microsoft.KeyVault(SecretUri=https://kv-dev-abc123.vault.azure.net/secrets/SecretName/)
   Wrong: @Microsoft.KeyVault(VaultName=kv-dev-abc123;SecretName=SecretName)
   ```

2. **Check secret exists**
   ```bash
   az keyvault secret list \
     --vault-name kv-dev-abc123 \
     --query "[].name"
   ```

3. **Verify network access**
   ```bash
   # Check if Key Vault allows public access
   az keyvault show \
     --name kv-dev-abc123 \
     --query properties.publicNetworkAccess
   ```

---

## Networking Issues

### Issue: VNet Integration Fails

**Symptoms**:
```
Error: Subnet is not valid for integration
```

**Solution**:
```bash
# Check if subnet is delegated to Microsoft.Web/serverFarms
az network vnet subnet show \
  --resource-group rg-timetracker-dev \
  --vnet-name tt-dev-vnet \
  --name appservice-subnet \
  --query delegations

# If not delegated, update subnet
az network vnet subnet update \
  --resource-group rg-timetracker-dev \
  --vnet-name tt-dev-vnet \
  --name appservice-subnet \
  --delegations Microsoft.Web/serverFarms
```

### Issue: Cannot Reach External Services

**Symptoms**:
App can't connect to external APIs or services

**Diagnostic Steps**:
```bash
# SSH into container and test connectivity
az webapp ssh \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# Inside container:
curl -v https://api.example.com
```

**Solution**:
Check Network Security Group (NSG) rules if using VNet integration

---

## Performance Issues

### Issue: Slow Response Times

**Symptoms**:
- High latency (> 1 second)
- Timeouts

**Diagnostic Steps**:
```bash
# Check Application Insights metrics
az monitor app-insights metrics show \
  --app tt-dev-ai \
  --resource-group rg-timetracker-dev \
  --metric requests/duration \
  --aggregation avg

# Check CPU and memory usage
az monitor metrics list \
  --resource $(az webapp show --name tt-dev-web-abc123 --resource-group rg-timetracker-dev --query id -o tsv) \
  --metric "CpuPercentage" "MemoryPercentage" \
  --aggregation Average
```

**Solutions**:

1. **Scale out App Service**
   ```bash
   az appservice plan update \
     --name tt-dev-asp \
     --resource-group rg-timetracker-dev \
     --number-of-workers 2
   ```

2. **Upgrade App Service SKU**
   ```bash
   az appservice plan update \
     --name tt-dev-asp \
     --resource-group rg-timetracker-dev \
     --sku S1
   ```

3. **Optimize database queries**
   - Add indexes
   - Review slow query logs
   - Enable connection pooling

4. **Enable caching**
   - Add Redis cache for session state
   - Implement response caching

### Issue: High Memory Usage

**Symptoms**:
```
Application restarts frequently
OutOfMemoryException in logs
```

**Solutions**:

1. **Increase memory limit**
   ```bash
   az appservice plan update \
     --name tt-dev-asp \
     --resource-group rg-timetracker-dev \
     --sku P1V3  # 8GB RAM
   ```

2. **Profile memory usage**
   - Use Application Insights Profiler
   - Review heap dumps
   - Check for memory leaks

---

## Monitoring and Logging

### Viewing Real-Time Logs

```bash
# Stream application logs
az webapp log tail \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev

# Stream specific log type
az webapp log tail \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev \
  --filter "Error"
```

### Downloading Logs

```bash
# Download all logs
az webapp log download \
  --name tt-dev-web-abc123 \
  --resource-group rg-timetracker-dev \
  --log-file logs.zip

# Extract and view
unzip logs.zip
cat LogFiles/Application/*.log
```

### Querying Log Analytics

```bash
# Query for errors in last hour
az monitor log-analytics query \
  --workspace $(az monitor log-analytics workspace show --resource-group rg-timetracker-dev --workspace-name tt-dev-logs --query customerId -o tsv) \
  --analytics-query "AppServiceConsoleLogs | where TimeGenerated > ago(1h) | where ResultDescription contains 'Error'" \
  --timespan PT1H
```

### Application Insights Queries

Use Azure Portal > Application Insights > Logs:

```kusto
// Failed requests in last 24 hours
requests
| where timestamp > ago(24h)
| where success == false
| summarize count() by resultCode, operation_Name

// Slow requests (> 1 second)
requests
| where timestamp > ago(1h)
| where duration > 1000
| project timestamp, name, duration, resultCode

// Exception details
exceptions
| where timestamp > ago(24h)
| project timestamp, type, outerMessage, innermostMessage
| order by timestamp desc
```

---

## Emergency Procedures

### Complete Service Restart

```bash
# Stop app
az webapp stop --name tt-dev-web-abc123 --resource-group rg-timetracker-dev

# Wait 30 seconds
sleep 30

# Start app
az webapp start --name tt-dev-web-abc123 --resource-group rg-timetracker-dev
```

### Quick Rollback

```bash
# Rollback to previous known-good version
./deployment/scripts/rollback.sh \
  --resource-group rg-timetracker-prod \
  --tag PREVIOUS_TAG
```

### Delete and Redeploy

```bash
# CAUTION: This deletes ALL resources!
az group delete \
  --name rg-timetracker-dev \
  --yes --no-wait

# Redeploy
./deployment/scripts/deploy.sh \
  --environment dev \
  --location eastus
```

---

## Getting Help

If issues persist:

1. **Check Azure Service Health**: https://status.azure.com/
2. **Review Azure Documentation**: https://docs.microsoft.com/azure/
3. **Open Azure Support Ticket**: Portal > Help + support > New support request
4. **Contact Development Team**: Open GitHub issue

---

## Useful Commands Reference

```bash
# List all resources in resource group
az resource list --resource-group rg-timetracker-dev --output table

# Check all deployment operations
az deployment group list --resource-group rg-timetracker-dev --output table

# Get web app configuration
az webapp config show --name tt-dev-web-abc123 --resource-group rg-timetracker-dev

# Check PostgreSQL parameters
az postgres flexible-server parameter list --server-name tt-dev-postgres-abc123 --resource-group rg-timetracker-dev

# List all Key Vault secrets
az keyvault secret list --vault-name kv-dev-abc123 --query "[].name" -o table

# Check ACR repositories and tags
az acr repository list --name acrdevabc123 -o table
az acr repository show-tags --name acrdevabc123 --repository timetracker -o table
```
