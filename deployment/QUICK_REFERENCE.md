# 🚀 Quick Reference - Azure Deployment

## One-Command Deploy

```bash
# Development
export POSTGRESQL_ADMIN_PASSWORD="YourPassword123!"
cd deployment/scripts && ./deploy.sh --environment dev --location eastus --run-migrations
```

## Essential Commands

### 📦 Deploy
```bash
# Full deployment
./deploy.sh --environment {dev|staging|prod} --location {region}

# Skip infrastructure (app only)
./deploy.sh --environment dev --skip-bicep

# With migrations
./deploy.sh --environment dev --run-migrations
```

### 🐳 Docker
```bash
# Build and push
./build-and-push.sh --resource-group rg-timetracker-dev --tag v1.0.0

# Also push to Docker Hub
./build-and-push.sh --resource-group rg-timetracker-dev --push-dockerhub
```

### 🗄️ Database
```bash
# Run migrations
./run-migrations.sh --resource-group rg-timetracker-dev

# SSH to container
az webapp ssh --name {webapp-name} --resource-group {rg-name}
```

### ⏮️ Rollback
```bash
# To specific version
./rollback.sh --resource-group rg-timetracker-prod --tag v1.0.0

# Using deployment slot
./rollback.sh --resource-group rg-timetracker-prod --use-slot
```

## 🔍 Monitoring

### View Logs
```bash
# Live tail
az webapp log tail --name {webapp} --resource-group {rg}

# Download
az webapp log download --name {webapp} --resource-group {rg} --log-file logs.zip
```

### Health Check
```bash
curl https://{webapp}.azurewebsites.net/health
```

### Metrics
```bash
az monitor app-insights metrics show \
  --app {ai-name} \
  --resource-group {rg} \
  --metric requests/count
```

## 🎯 Resource Names

| Environment | Pattern |
|-------------|---------|
| Resource Group | `rg-timetracker-{env}` |
| Web App | `tt-{env}-web-{id}` |
| PostgreSQL | `tt-{env}-postgres-{id}` |
| Key Vault | `kv-{env}-{id}` |
| ACR | `acr{env}{id}` |

## 🔑 Secrets

### Environment Variables
```bash
export POSTGRESQL_ADMIN_PASSWORD="..."
export DOCKER_IMAGE_TAG="v1.0.0"
```

### Key Vault Secrets
- `ConnectionStrings--DefaultConnection`
- `PostgreSQL--AdminPassword`
- `ACR--Password`

## 💰 Monthly Costs

| Env | Cost |
|-----|------|
| Dev | ~$28 |
| Staging | ~$90 |
| Prod | ~$278 |

## 🆘 Quick Troubleshooting

### App won't start
```bash
az webapp restart --name {webapp} --resource-group {rg}
az webapp log tail --name {webapp} --resource-group {rg}
```

### Database connection fails
```bash
# Check VNet integration
az webapp vnet-integration list --name {webapp} --resource-group {rg}

# Check connection string
az keyvault secret show --vault-name {kv} --name ConnectionStrings--DefaultConnection
```

### Key Vault access denied
```bash
# Check managed identity
PRINCIPAL_ID=$(az webapp identity show --name {webapp} --resource-group {rg} --query principalId -o tsv)

# Assign role
az role assignment create \
  --role "Key Vault Secrets User" \
  --assignee $PRINCIPAL_ID \
  --scope $(az keyvault show --name {kv} --query id -o tsv)
```

## 📚 Documentation

- [Deployment Guide](docs/DEPLOYMENT.md) - Complete walkthrough
- [Architecture](docs/ARCHITECTURE.md) - Design & patterns
- [Troubleshooting](docs/TROUBLESHOOTING.md) - Common issues
- [README](README.md) - Overview & quick start

## 🎛️ Quick Scale

```bash
# Scale out (more instances)
az appservice plan update \
  --name tt-prod-asp \
  --resource-group rg-timetracker-prod \
  --number-of-workers 3

# Scale up (bigger SKU)
az appservice plan update \
  --name tt-prod-asp \
  --resource-group rg-timetracker-prod \
  --sku P2V3
```

## 🧹 Cleanup

```bash
# Delete resource group (⚠️ CAUTION!)
az group delete --name rg-timetracker-dev --yes --no-wait
```

## 📋 Pre-Deploy Checklist

- [ ] Azure CLI logged in: `az login`
- [ ] Subscription set: `az account set --subscription {id}`
- [ ] Password set: `export POSTGRESQL_ADMIN_PASSWORD="..."`
- [ ] Docker running: `docker ps`
- [ ] Scripts executable: `chmod +x deployment/scripts/*.sh`

## 📱 Azure Portal Quick Links

- Resource Groups: https://portal.azure.com/#view/HubsExtension/BrowseResourceGroups
- App Services: https://portal.azure.com/#view/HubsExtension/BrowseResource/resourceType/Microsoft.Web%2Fsites
- PostgreSQL: https://portal.azure.com/#view/HubsExtension/BrowseResource/resourceType/Microsoft.DBforPostgreSQL%2FflexibleServers
- Key Vaults: https://portal.azure.com/#view/HubsExtension/BrowseResource/resourceType/Microsoft.KeyVault%2Fvaults

## 🎯 Default Credentials

**⚠️ CHANGE IN PRODUCTION!**

- PostgreSQL User: `ttadmin`
- PostgreSQL Password: Set via `POSTGRESQL_ADMIN_PASSWORD`

---

**Need help?** See [TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md) or open a GitHub issue.
