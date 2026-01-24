# Refined Azure Deployment Plan for Time Tracker Application

## Overview
This plan outlines the deployment of the Time Tracker application to Microsoft Azure using Infrastructure as Code (IaC) with Bicep templates. The deployment follows the Deployment Stamps pattern, allowing for scalable, isolated deployments across multiple regions or environments.

## Architecture Components

### 1. Core Infrastructure
- **Azure Container Registry (ACR)**: Private container registry for the application image
- **Azure App Service (Linux)**: Hosts the .NET application container
- **Azure Database for PostgreSQL Flexible Server**: Managed PostgreSQL database
- **Azure Key Vault**: Secure storage for secrets and connection strings
- **Azure Virtual Network**: Network isolation and security
- **Azure Log Analytics Workspace**: Centralized logging and monitoring

### 2. Deployment Stamps Pattern
Each "stamp" represents a complete, isolated deployment of the application stack:
- Stamp = App Service + PostgreSQL + Key Vault + VNet
- Stamps can be deployed to different regions or environments
- Each stamp is independent and can scale horizontally
- Enables blue-green deployments and A/B testing

### 3. Security Model
- **Managed Identity**: App Service uses System-Assigned Managed Identity
- **Key Vault Access**: App Service accesses secrets via Managed Identity (no connection strings in code)
- **VNet Integration**: App Service and PostgreSQL communicate over private network
- **PostgreSQL Firewall**: Only accessible from App Service subnet
- **TLS/SSL**: All connections encrypted in transit

## Implementation Structure

```
deployment/
├── bicep/
│   ├── main.bicep                    # Main orchestration template
│   ├── modules/
│   │   ├── keyvault.bicep           # Key Vault with access policies
│   │   ├── postgresql.bicep         # PostgreSQL Flexible Server
│   │   ├── appservice.bicep         # App Service Plan + Web App
│   │   ├── vnet.bicep               # Virtual Network + Subnets
│   │   ├── acr.bicep                # Container Registry
│   │   └── monitoring.bicep         # Log Analytics + App Insights
│   └── parameters/
│       ├── dev.bicepparam           # Development parameters
│       ├── staging.bicepparam       # Staging parameters
│       └── prod.bicepparam          # Production parameters
├── scripts/
│   ├── deploy.sh                    # Main deployment script
│   ├── build-and-push.sh           # Build Docker image and push to ACR
│   ├── run-migrations.sh           # Execute EF migrations
│   └── rollback.sh                 # Rollback script
├── docker/
│   ├── Dockerfile.production       # Optimized production Dockerfile
│   └── .dockerignore              # Docker ignore patterns
└── docs/
    ├── DEPLOYMENT.md               # Step-by-step deployment guide
    ├── ARCHITECTURE.md             # Architecture documentation
    └── TROUBLESHOOTING.md          # Common issues and solutions
```

## Deployment Workflow

### Phase 1: Prerequisites
1. Azure subscription with appropriate permissions
2. Azure CLI installed and authenticated
3. Docker installed for local testing
4. .NET SDK 10.0 installed

### Phase 2: Infrastructure Provisioning
1. Create resource group(s) for stamp(s)
2. Deploy Bicep templates (networking → data → compute → security)
3. Configure Key Vault with initial secrets
4. Set up managed identities and access policies

### Phase 3: Application Deployment
1. Build Docker image with production optimizations
2. Push image to Azure Container Registry
3. Deploy image to App Service
4. Run database migrations
5. Configure App Service settings to use Key Vault references

### Phase 4: Verification & Monitoring
1. Health check endpoints
2. Validate database connectivity
3. Verify Key Vault secret retrieval
4. Configure Application Insights
5. Set up alerts and dashboards

## Key Design Decisions

### 1. Bicep vs Terraform
**Decision**: Use Bicep
**Rationale**: Native Azure IaC language, better type checking, simpler syntax for Azure resources

### 2. Container Registry
**Decision**: Use Azure Container Registry instead of Docker Hub
**Rationale**: 
- Better security (private registry)
- No rate limiting
- Integrated with Azure RBAC
- VNet integration for secure pulls
- Note: Can still publish to Docker Hub for public reference

### 3. Database Connection Management
**Decision**: Use Key Vault references in App Service configuration
**Rationale**:
- No secrets in environment variables or code
- Automatic secret rotation support
- Centralized secret management
- Audit trail for secret access

### 4. Networking
**Decision**: VNet integration with private endpoints
**Rationale**:
- Database not exposed to public internet
- Improved security posture
- Better compliance with security standards

### 5. App Service Plan
**Decision**: Premium V3 (P1V3) tier for production
**Rationale**:
- VNet integration support
- Better performance
- Auto-scaling capabilities
- Deployment slots for zero-downtime deployments

## Cost Estimation (Monthly - USD)

### Development Stamp
- App Service Plan (B1): ~$13
- PostgreSQL Flexible Server (Burstable B1ms): ~$12
- Key Vault: ~$3 (secrets storage)
- Total: ~$28/month

### Production Stamp
- App Service Plan (P1V3): ~$115
- PostgreSQL Flexible Server (General Purpose D2s_v3): ~$150
- Key Vault: ~$3
- Application Insights: ~$10
- Total: ~$278/month

## Security Checklist

- [ ] All secrets stored in Key Vault
- [ ] Managed Identity configured for App Service
- [ ] PostgreSQL accessible only via private endpoint
- [ ] TLS 1.2+ enforced for all connections
- [ ] Database credentials rotated regularly
- [ ] Role-Based Access Control (RBAC) configured
- [ ] Diagnostic logging enabled
- [ ] Network Security Groups (NSG) configured
- [ ] Regular security scans scheduled

## Rollback Strategy

1. **App Service**: Use deployment slots for instant rollback
2. **Database**: Maintain automated backups (7-35 days retention)
3. **Infrastructure**: Tag Bicep deployments for version tracking
4. **Monitoring**: Set up alerts for degraded health metrics

## Next Steps

1. ✅ Create detailed Bicep templates for each module
2. ✅ Write deployment automation scripts
3. ✅ Create parameter files for each environment
4. ✅ Update application code for Key Vault integration
5. ✅ Write comprehensive documentation
6. Test deployment in development environment
7. Create CI/CD pipeline (GitHub Actions or Azure DevOps)
8. Deploy to staging for validation
9. Production deployment with monitoring

## References

- [Azure Deployment Stamps Pattern](https://learn.microsoft.com/azure/architecture/patterns/deployment-stamp)
- [App Service Key Vault References](https://learn.microsoft.com/azure/app-service/app-service-key-vault-references)
- [Azure PostgreSQL Flexible Server](https://learn.microsoft.com/azure/postgresql/flexible-server/)
- [Bicep Documentation](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)
