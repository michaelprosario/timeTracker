# Time Tracker Azure Architecture

## Overview

The Time Tracker application is deployed to Microsoft Azure using a modern cloud-native architecture following best practices for security, scalability, and maintainability. The deployment uses the **Deployment Stamps Pattern**, allowing for multiple isolated instances that can be deployed across regions or environments.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          Azure Subscription                              │
│                                                                          │
│  ┌────────────────────────────────────────────────────────────────┐   │
│  │            Resource Group: rg-timetracker-{env}                │   │
│  │                                                                 │   │
│  │  ┌───────────────────────────────────────────────────────┐    │   │
│  │  │         Virtual Network (10.0.0.0/16)                 │    │   │
│  │  │                                                        │    │   │
│  │  │  ┌──────────────────────┐  ┌─────────────────────┐  │    │   │
│  │  │  │ App Service Subnet   │  │ PostgreSQL Subnet   │  │    │   │
│  │  │  │ 10.0.1.0/24         │  │ 10.0.2.0/24        │  │    │   │
│  │  │  │ (Delegated to       │  │ (Delegated to      │  │    │   │
│  │  │  │  Microsoft.Web)     │  │  Microsoft.DB)     │  │    │   │
│  │  │  └──────────┬───────────┘  └────────┬───────────┘  │    │   │
│  │  │             │                        │              │    │   │
│  │  │  ┌──────────▼───────────┐  ┌────────▼───────────┐ │    │   │
│  │  │  │ Private Endpoints    │  │                    │ │    │   │
│  │  │  │ Subnet 10.0.3.0/24  │  │                    │ │    │   │
│  │  │  └──────────────────────┘  │                    │ │    │   │
│  │  └───────────────────────────────────────────────────┘    │   │
│  │                                                             │   │
│  │  ┌─────────────────────────────────────────────────────┐  │   │
│  │  │            Compute Layer                            │  │   │
│  │  │                                                      │  │   │
│  │  │  ┌────────────────────────────────────────────┐    │  │   │
│  │  │  │  App Service Plan (Linux)                  │    │  │   │
│  │  │  │  - SKU: B1 (Dev) / P1V3 (Prod)           │    │  │   │
│  │  │  │  - Auto-scaling enabled (Prod)            │    │  │   │
│  │  │  └────────────────────────────────────────────┘    │  │   │
│  │  │           │                                         │  │   │
│  │  │           ▼                                         │  │   │
│  │  │  ┌────────────────────────────────────────────┐    │  │   │
│  │  │  │  App Service (Web App)                     │    │  │   │
│  │  │  │  - Container: .NET 10.0 Alpine             │    │  │   │
│  │  │  │  - System-Assigned Managed Identity        │    │  │   │
│  │  │  │  - VNet Integration                        │    │  │   │
│  │  │  │  - Health Check: /health                   │    │  │   │
│  │  │  │  - Deployment Slots (Prod only)            │    │  │   │
│  │  │  └────────────────────────────────────────────┘    │  │   │
│  │  └─────────────────────────────────────────────────────┘  │   │
│  │                                                             │   │
│  │  ┌─────────────────────────────────────────────────────┐  │   │
│  │  │            Data Layer                               │  │   │
│  │  │                                                      │  │   │
│  │  │  ┌────────────────────────────────────────────┐    │  │   │
│  │  │  │  PostgreSQL Flexible Server                │    │  │   │
│  │  │  │  - Version: 16                             │    │  │   │
│  │  │  │  - SKU: Burstable B1ms (Dev)              │    │  │   │
│  │  │  │        General Purpose D2s (Prod)         │    │  │   │
│  │  │  │  - Private Network Only                    │    │  │   │
│  │  │  │  - Automated Backups (7-35 days)          │    │  │   │
│  │  │  │  - Geo-Redundant (Prod)                   │    │  │   │
│  │  │  └────────────────────────────────────────────┘    │  │   │
│  │  └─────────────────────────────────────────────────────┘  │   │
│  │                                                             │   │
│  │  ┌─────────────────────────────────────────────────────┐  │   │
│  │  │            Security Layer                           │  │   │
│  │  │                                                      │  │   │
│  │  │  ┌────────────────────────────────────────────┐    │  │   │
│  │  │  │  Azure Key Vault                           │    │  │   │
│  │  │  │  - RBAC-based access                       │    │  │   │
│  │  │  │  - Soft delete enabled                     │    │  │   │
│  │  │  │  - Secrets:                                │    │  │   │
│  │  │  │    • DB Connection String                  │    │  │   │
│  │  │  │    • PostgreSQL Admin Password             │    │  │   │
│  │  │  │    • ACR Credentials                       │    │  │   │
│  │  │  └────────────────────────────────────────────┘    │  │   │
│  │  │           ▲                                         │  │   │
│  │  │           │ Managed Identity Access                │  │   │
│  │  │           │                                         │  │   │
│  │  └───────────┼─────────────────────────────────────────┘  │   │
│  │              │                                             │   │
│  │  ┌───────────┴─────────────────────────────────────────┐  │   │
│  │  │            Container Registry                       │  │   │
│  │  │                                                      │  │   │
│  │  │  ┌────────────────────────────────────────────┐    │  │   │
│  │  │  │  Azure Container Registry (ACR)            │    │  │   │
│  │  │  │  - Private registry for app images         │    │  │   │
│  │  │  │  - Integrated with App Service             │    │  │   │
│  │  │  │  - Image retention: 7 days                 │    │  │   │
│  │  │  │  - Webhook for automated deployments       │    │  │   │
│  │  │  └────────────────────────────────────────────┘    │  │   │
│  │  └─────────────────────────────────────────────────────┘  │   │
│  │                                                             │   │
│  │  ┌─────────────────────────────────────────────────────┐  │   │
│  │  │            Monitoring & Observability               │  │   │
│  │  │                                                      │  │   │
│  │  │  ┌────────────────────────────────────────────┐    │  │   │
│  │  │  │  Log Analytics Workspace                   │    │  │   │
│  │  │  │  - Centralized log aggregation             │    │  │   │
│  │  │  │  - 30-day retention                        │    │  │   │
│  │  │  │  - Query interface for analysis            │    │  │   │
│  │  │  └────────────────────────────────────────────┘    │  │   │
│  │  │           │                                         │  │   │
│  │  │           ▼                                         │  │   │
│  │  │  ┌────────────────────────────────────────────┐    │  │   │
│  │  │  │  Application Insights                      │    │  │   │
│  │  │  │  - Performance monitoring                  │    │  │   │
│  │  │  │  - Distributed tracing                     │    │  │   │
│  │  │  │  - Exception tracking                      │    │  │   │
│  │  │  │  - Custom metrics & events                 │    │  │   │
│  │  │  │  - Availability tests                      │    │  │   │
│  │  │  └────────────────────────────────────────────┘    │  │   │
│  │  └─────────────────────────────────────────────────────┘  │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
```

## Components

### 1. Networking (VNet)

**Purpose**: Isolate resources and control network traffic

**Configuration**:
- **Address Space**: 10.0.0.0/16
- **Subnets**:
  - App Service Subnet (10.0.1.0/24) - Delegated to Microsoft.Web/serverFarms
  - PostgreSQL Subnet (10.0.2.0/24) - Delegated to Microsoft.DBforPostgreSQL/flexibleServers
  - Private Endpoints Subnet (10.0.3.0/24) - For future private endpoints

**Security Features**:
- Service endpoints for Key Vault and Storage
- Network Security Groups (NSGs) for traffic filtering
- No public access to database
- VNet integration for App Service

### 2. App Service Plan & Web App

**Purpose**: Host the containerized .NET application

**App Service Plan**:
- **Development**: B1 (Basic) - $13/month
- **Staging**: S1 (Standard) - $70/month
- **Production**: P1V3 (Premium V3) - $115/month

**Web App Configuration**:
- **Runtime**: Linux container
- **Container Source**: Azure Container Registry
- **Identity**: System-assigned managed identity
- **VNet Integration**: Connected to App Service subnet
- **Health Check**: /health endpoint
- **Deployment Slots**: Production environment has staging slot

**Environment Variables**:
```
ASPNETCORE_ENVIRONMENT=Production
WEBSITES_PORT=8080
APPLICATIONINSIGHTS_CONNECTION_STRING=@Microsoft.KeyVault(...)
ConnectionStrings__DefaultConnection=@Microsoft.KeyVault(...)
```

### 3. PostgreSQL Flexible Server

**Purpose**: Managed relational database for application data

**Configuration**:
- **Version**: PostgreSQL 16
- **Development**:
  - SKU: Standard_B1ms (Burstable, 1 vCore, 2GB RAM)
  - Storage: 32 GB
  - Backup: 7 days retention
  - Cost: ~$12/month

- **Production**:
  - SKU: Standard_D2s_v3 (General Purpose, 2 vCores, 8GB RAM)
  - Storage: 128 GB with auto-grow
  - Backup: 35 days retention, geo-redundant
  - High Availability: Zone-redundant
  - Cost: ~$150/month

**Security**:
- Private network access only (VNet integrated)
- TLS 1.2+ enforced
- Firewall rules for Azure services (migrations only)
- Automated backups with point-in-time restore

**Database Schema**:
- Managed by Entity Framework Core migrations
- Includes tables for Users, Projects, TimeSheets, TimeEntries, WorkTypes

### 4. Azure Key Vault

**Purpose**: Secure storage and management of secrets

**Configuration**:
- **SKU**: Standard
- **Access Model**: Azure RBAC (Role-Based Access Control)
- **Soft Delete**: Enabled (7-day retention)
- **Purge Protection**: Enabled in production

**Stored Secrets**:
1. `ConnectionStrings--DefaultConnection`: PostgreSQL connection string
2. `PostgreSQL--AdminPassword`: Database admin password
3. `ACR--Password`: Container registry credentials

**Access Control**:
- App Service managed identity has "Key Vault Secrets User" role
- Developers have "Key Vault Secrets Officer" role (dev/staging only)
- Audit logs enabled for all secret access

### 5. Azure Container Registry (ACR)

**Purpose**: Private registry for Docker images

**Configuration**:
- **SKU**: Basic (dev) / Standard (prod)
- **Admin User**: Enabled for initial setup
- **Webhook**: Triggers App Service deployment on push
- **Retention Policy**: 7 days for untagged images

**Image Tags**:
- `latest`: Most recent build
- `YYYYMMDD-HHMMSS`: Timestamp tags for versioning
- `v1.0.0`: Semantic version tags

### 6. Monitoring Stack

#### Log Analytics Workspace

**Purpose**: Centralized log aggregation and analysis

**Configuration**:
- **Retention**: 30 days
- **Pricing**: Pay-per-GB (first 5GB/day free)

**Log Sources**:
- App Service diagnostic logs
- PostgreSQL audit logs
- Resource activity logs
- Custom application logs

#### Application Insights

**Purpose**: Application performance monitoring (APM)

**Features**:
- Request tracking and latency monitoring
- Dependency tracking (database, external APIs)
- Exception tracking with stack traces
- Custom events and metrics
- Live metrics stream
- Availability tests

**Key Metrics Tracked**:
- Request rate and response time
- Failed request rate
- Server response time
- Database query performance
- Exception rate
- Active user sessions

## Security Architecture

### Defense in Depth

```
┌─────────────────────────────────────────────────┐
│  Layer 1: Network Security                     │
│  - VNet isolation                               │
│  - NSG rules                                    │
│  - No public database access                   │
└─────────────────────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────┐
│  Layer 2: Identity & Access Management         │
│  - Managed Identity (no credentials in code)   │
│  - RBAC for all resources                      │
│  - Azure AD integration                        │
└─────────────────────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────┐
│  Layer 3: Data Protection                      │
│  - TLS/SSL for all connections                 │
│  - Encryption at rest                          │
│  - Key Vault for secrets                       │
│  - Automated backups                           │
└─────────────────────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────┐
│  Layer 4: Application Security                 │
│  - Secure coding practices                     │
│  - Input validation                            │
│  - HTTPS only                                  │
│  - Security headers                            │
└─────────────────────────────────────────────────┘
                     ▼
┌─────────────────────────────────────────────────┐
│  Layer 5: Monitoring & Response                │
│  - Security audit logs                         │
│  - Alert rules for anomalies                   │
│  - Diagnostic logging                          │
└─────────────────────────────────────────────────┘
```

### Managed Identity Flow

```
┌──────────────┐
│  App Service │
│  (Web App)   │
└───────┬──────┘
        │ 1. System-Assigned
        │    Managed Identity
        ▼
┌─────────────────────┐
│  Azure AD           │
│  (Identity Provider)│
└─────────┬───────────┘
          │ 2. Token Request
          │    (no credentials needed)
          ▼
┌─────────────────────┐     3. Access Token
│  Azure Key Vault    │◄──────────────────
│                     │
│  4. Retrieve Secret │
│     (if authorized) │
└─────────────────────┘
          │
          │ 5. Secret Value
          ▼
┌──────────────┐
│  App Service │
│  (uses secret│
│   in config) │
└──────────────┘
```

## Deployment Stamps Pattern

### What is a Deployment Stamp?

A **deployment stamp** is a complete, isolated unit of deployment that includes all components needed to run the application. Each stamp is independent and can scale horizontally.

### Benefits

1. **Isolation**: Issues in one stamp don't affect others
2. **Scalability**: Add new stamps to handle more load
3. **Geographic Distribution**: Deploy stamps in different regions
4. **Testing**: Use stamps for different environments (dev/staging/prod)
5. **Blue-Green Deployments**: Deploy to new stamp, then switch traffic

### Example Multi-Stamp Deployment

```
┌────────────────────────────────────────────────────────┐
│                  Azure Front Door / CDN                │
│              (Global Load Balancer)                     │
└───────┬───────────────────────┬────────────────────────┘
        │                       │
        ▼                       ▼
┌───────────────────┐   ┌───────────────────┐
│  Stamp: East US   │   │  Stamp: West US   │
│                   │   │                   │
│  ┌─────────────┐ │   │  ┌─────────────┐  │
│  │ App Service │ │   │  │ App Service │  │
│  │ PostgreSQL  │ │   │  │ PostgreSQL  │  │
│  │ Key Vault   │ │   │  │ Key Vault   │  │
│  └─────────────┘ │   │  └─────────────┘  │
└───────────────────┘   └───────────────────┘
```

### Current Implementation

Our deployment uses a single stamp per environment:
- `dev` stamp: For development and testing
- `staging` stamp: For pre-production validation
- `prod` stamp: For production workload

Each stamp is completely independent with its own:
- Resource group
- VNet
- Database
- Key Vault
- Monitoring

## Scalability

### Horizontal Scaling (App Service)

```bash
# Auto-scale based on CPU
az monitor autoscale create \
  --resource-group rg-timetracker-prod \
  --resource tt-prod-asp \
  --resource-type Microsoft.Web/serverFarms \
  --name autoscale-prod \
  --min-count 2 \
  --max-count 10 \
  --count 2

# Scale out when CPU > 70%
az monitor autoscale rule create \
  --resource-group rg-timetracker-prod \
  --autoscale-name autoscale-prod \
  --condition "Percentage CPU > 70 avg 5m" \
  --scale out 1

# Scale in when CPU < 30%
az monitor autoscale rule create \
  --resource-group rg-timetracker-prod \
  --autoscale-name autoscale-prod \
  --condition "Percentage CPU < 30 avg 5m" \
  --scale in 1
```

### Database Scaling

**Vertical Scaling** (increase SKU):
```bash
az postgres flexible-server update \
  --resource-group rg-timetracker-prod \
  --name tt-prod-postgres \
  --sku-name Standard_D4s_v3 \
  --tier GeneralPurpose
```

**Storage Auto-Growth**: Enabled by default
- Automatically increases storage when threshold is reached
- No downtime required

### Read Replicas (Future Enhancement)

For read-heavy workloads:
```bash
az postgres flexible-server replica create \
  --replica-name tt-prod-postgres-replica \
  --resource-group rg-timetracker-prod \
  --source-server tt-prod-postgres
```

## Disaster Recovery

### Backup Strategy

**Database Backups**:
- Automated daily backups
- Retention: 7 days (dev), 35 days (prod)
- Geo-redundant storage (prod only)
- Point-in-time restore capability

**Infrastructure as Code**:
- All infrastructure defined in Bicep templates
- Version controlled in Git
- Can recreate entire environment from scratch

### Recovery Procedures

**Scenario 1: Database Corruption**
```bash
# Restore to specific point in time
az postgres flexible-server restore \
  --resource-group rg-timetracker-prod \
  --name tt-prod-postgres-restored \
  --source-server tt-prod-postgres \
  --restore-time "2026-01-24T12:00:00Z"
```

**Scenario 2: Application Failure**
```bash
# Rollback to previous container image
./deployment/scripts/rollback.sh \
  --resource-group rg-timetracker-prod \
  --tag 20260123-150000
```

**Scenario 3: Regional Outage**
- Deploy new stamp in different region
- Restore database from geo-redundant backup
- Update DNS to point to new region

### RTO and RPO

- **RTO (Recovery Time Objective)**: < 1 hour
- **RPO (Recovery Point Objective)**: < 5 minutes (based on backup frequency)

## Cost Optimization

### Development Environment (~$28/month)
- B1 App Service Plan: ~$13
- B1ms PostgreSQL: ~$12
- Key Vault: ~$3
- Monitoring: Minimal (free tier)

### Production Environment (~$278/month)
- P1V3 App Service Plan: ~$115
- D2s_v3 PostgreSQL: ~$150
- Standard ACR: ~$5
- Key Vault: ~$3
- Application Insights: ~$5 (based on usage)

### Cost Saving Tips

1. **Use Dev/Test Pricing**: Apply for Azure Dev/Test subscription
2. **Reserved Instances**: 1-year or 3-year commitments save 30-70%
3. **Auto-shutdown**: Schedule App Service to stop during non-business hours (dev/staging)
4. **Right-Size Resources**: Monitor usage and adjust SKUs accordingly
5. **Use Burstable SKUs**: For non-production environments

## Performance Characteristics

### Expected Metrics (Production)

- **Response Time**: < 200ms (p95)
- **Throughput**: 1000 requests/minute per instance
- **Availability**: 99.95% SLA
- **Database Connections**: Up to 100 concurrent
- **Storage IOPS**: 3000 IOPS (D2s_v3 PostgreSQL)

### Bottlenecks and Mitigation

| Bottleneck | Symptom | Mitigation |
|------------|---------|------------|
| CPU | High response time | Scale out App Service |
| Memory | Application restarts | Upgrade to higher SKU |
| Database | Slow queries | Add indexes, increase IOPS |
| Network | Connection timeouts | Check VNet configuration |
| Storage | Disk full | Enable auto-grow |

## Future Enhancements

1. **Azure Front Door**: Global load balancing and CDN
2. **Redis Cache**: Distributed caching for session state
3. **Service Bus**: Async processing and event-driven architecture
4. **Azure AD B2C**: User authentication and authorization
5. **Cosmos DB**: Multi-region, low-latency data store
6. **API Management**: API gateway with throttling and analytics
7. **Azure DevOps**: Full CI/CD pipeline
8. **Terraform**: Alternative IaC for multi-cloud support

## Compliance and Governance

### Azure Policy

Apply organizational standards:
```bash
# Require tags on all resources
az policy assignment create \
  --name 'require-tags' \
  --policy 'require-tags-policy' \
  --scope /subscriptions/SUBSCRIPTION_ID/resourceGroups/rg-timetracker-prod
```

### Cost Management

- Budget alerts at 80% and 100% of monthly limit
- Cost analysis reports sent weekly
- Resource tagging for cost allocation

### Audit Logging

- All administrative actions logged
- Key Vault access audited
- Database query auditing enabled
- Retention: 90 days

---

## Summary

This architecture provides a secure, scalable, and maintainable platform for the Time Tracker application with:

✅ **Security**: Multi-layered security with managed identities and Key Vault
✅ **Scalability**: Auto-scaling and horizontal stamp deployment
✅ **Reliability**: Automated backups and geo-redundancy
✅ **Observability**: Comprehensive monitoring and logging
✅ **Cost-Effective**: Right-sized for each environment
✅ **Maintainable**: Infrastructure as Code with Bicep

The deployment follows Azure best practices and industry standards for cloud-native applications.
