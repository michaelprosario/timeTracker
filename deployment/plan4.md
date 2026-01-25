# Plan 4: Docker Hub Deployment Strategy

## Overview
Transition from Azure Container Registry (ACR) to Docker Hub for hosting container images, enabling public access and broader software distribution.

## Benefits of Using Docker Hub

1. **Public Distribution**: Share the application with the broader community
2. **Simplified Access**: No Azure-specific authentication required for pulling images
3. **Cost Reduction**: Eliminate ACR costs (especially for dev/staging)
4. **Multi-Cloud Ready**: Easier deployment to non-Azure environments
5. **Developer Friendly**: Standard Docker workflow familiar to all developers

## Current Architecture Issues

The current deployment has tight coupling with ACR:
- **main.bicep**: Creates ACR module and passes credentials to App Service
- **appservice.bicep**: Configured with ACR-specific authentication
- **build-and-push.sh**: Builds and pushes to ACR
- **deploy.sh**: Assumes ACR exists and retrieves credentials

## Required Changes

### 1. Infrastructure Changes (Bicep Templates)

#### 1.1 Remove ACR Module
- **File**: [deployment/bicep/main.bicep](deployment/bicep/main.bicep)
- **Action**: Remove ACR module deployment and references
- **Impact**: Reduces infrastructure complexity and cost

#### 1.2 Update App Service Module
- **File**: [deployment/bicep/modules/appservice.bicep](deployment/bicep/modules/appservice.bicep)
- **Changes**:
  - Change `linuxFxVersion` to use Docker Hub image format
  - Remove ACR authentication parameters (username/password)
  - For public images: No DOCKER_REGISTRY_SERVER_* settings needed
  - For private images: Use Docker Hub credentials instead of ACR

**Example configuration:**
```bicep
// Public Docker Hub image (no auth needed)
linuxFxVersion: 'DOCKER|<dockerhub-username>/timetracker:${dockerImageTag}'

// Private Docker Hub image (requires auth)
linuxFxVersion: 'DOCKER|<dockerhub-username>/timetracker:${dockerImageTag}'
appSettings: [
  {
    name: 'DOCKER_REGISTRY_SERVER_URL'
    value: 'https://index.docker.io/v1'
  }
  {
    name: 'DOCKER_REGISTRY_SERVER_USERNAME'
    value: dockerhubUsername
  }
  {
    name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
    value: dockerhubPassword
  }
]
```

#### 1.3 Update Main Bicep Parameters
- **File**: [deployment/bicep/main.bicep](deployment/bicep/main.bicep)
- **Add**:
  - `dockerhubUsername` parameter (optional for public images)
  - `dockerhubPassword` parameter (optional, secure)
  - `dockerhubRepository` parameter (e.g., 'username/timetracker')
- **Remove**:
  - ACR-related variables
  - ACR module references

#### 1.4 Update Parameter Files
- **Files**: 
  - [deployment/bicep/parameters/dev.bicepparam](deployment/bicep/parameters/dev.bicepparam)
  - [deployment/bicep/parameters/staging.bicepparam](deployment/bicep/parameters/staging.bicepparam)
  - [deployment/bicep/parameters/prod.bicepparam](deployment/bicep/parameters/prod.bicepparam)
- **Changes**:
  - Remove ACR references
  - Add Docker Hub repository name
  - Add Docker Hub credentials reference (Key Vault or parameter)

### 2. Build and Push Script Changes

#### 2.1 Update build-and-push.sh
- **File**: [deployment/scripts/build-and-push.sh](deployment/scripts/build-and-push.sh)
- **Changes**:
  - Remove ACR detection and login logic
  - Focus on Docker Hub authentication
  - Update image naming convention
  - Support both public and private repositories

**Key updates:**
```bash
# Docker Hub login
docker login -u "${DOCKERHUB_USERNAME}" -p "${DOCKERHUB_PASSWORD}"

# Build image
docker build -t "${DOCKERHUB_USERNAME}/timetracker:${IMAGE_TAG}" \
  -f deployment/docker/Dockerfile.production "${PROJECT_ROOT}"

# Tag additional versions
docker tag "${DOCKERHUB_USERNAME}/timetracker:${IMAGE_TAG}" \
  "${DOCKERHUB_USERNAME}/timetracker:latest"

# Push to Docker Hub
docker push "${DOCKERHUB_USERNAME}/timetracker:${IMAGE_TAG}"
docker push "${DOCKERHUB_USERNAME}/timetracker:latest"
```

### 3. Deployment Script Changes

#### 3.1 Update deploy.sh
- **File**: [deployment/scripts/deploy.sh](deployment/scripts/deploy.sh)
- **Changes**:
  - Remove ACR credential retrieval
  - Add Docker Hub credential handling
  - Update Bicep parameter passing
  - Simplify deployment flow

### 4. Documentation Updates

#### 4.1 Update DEPLOYMENT.md
- **File**: [deployment/docs/DEPLOYMENT.md](deployment/docs/DEPLOYMENT.md)
- **Sections to update**:
  - Prerequisites: Add Docker Hub account setup
  - Architecture diagram: Remove ACR component
  - Deployment steps: Update for Docker Hub workflow
  - Credential management: Docker Hub instead of ACR

#### 4.2 Update ARCHITECTURE.md
- **File**: [deployment/docs/ARCHITECTURE.md](deployment/docs/ARCHITECTURE.md)
- **Changes**:
  - Remove ACR from architecture diagrams
  - Update deployment flow diagrams
  - Document Docker Hub integration

### 5. Security Considerations

#### 5.1 Credential Management
- **Options**:
  1. **Public Repository**: No credentials needed, but image is public
  2. **Private Repository with Key Vault**: Store Docker Hub token in Azure Key Vault
  3. **GitHub Secrets**: For CI/CD pipelines

#### 5.2 Recommended Approach
- Use Docker Hub Access Tokens (not passwords)
- Store tokens in Azure Key Vault
- Reference from App Service via Key Vault references
- Use different tokens for different environments

**Key Vault secret reference example:**
```bicep
{
  name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
  value: '@Microsoft.KeyVault(SecretUri=https://${keyVaultName}.vault.azure.net/secrets/dockerhub-token)'
}
```

### 6. CI/CD Pipeline Updates

#### 6.1 GitHub Actions / Azure DevOps
- Update CI pipeline to:
  - Build Docker image
  - Login to Docker Hub
  - Push to Docker Hub
  - Trigger Azure deployment with new image tag

#### 6.2 Example GitHub Actions Workflow
```yaml
name: Build and Deploy

on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Login to Docker Hub
        uses: docker/login-action@v2
        with:
          username: ${{ secrets.DOCKERHUB_USERNAME }}
          password: ${{ secrets.DOCKERHUB_TOKEN }}
      
      - name: Build and Push
        run: |
          docker build -t ${{ secrets.DOCKERHUB_USERNAME }}/timetracker:${{ github.sha }} .
          docker push ${{ secrets.DOCKERHUB_USERNAME }}/timetracker:${{ github.sha }}
      
      - name: Deploy to Azure
        run: |
          ./deployment/scripts/deploy.sh \
            --resource-group rg-timetracker-prod \
            --image-tag ${{ github.sha }}
```

### 7. Testing Strategy

#### 7.1 Local Testing
```bash
# Pull image from Docker Hub
docker pull <username>/timetracker:latest

# Run locally
docker run -p 8080:8080 <username>/timetracker:latest
```

#### 7.2 Azure App Service Testing
- Deploy to dev environment first
- Verify image pull from Docker Hub
- Check application startup and functionality
- Monitor for any credential or access issues

## Implementation Steps

### Phase 1: Preparation (Day 1)
1. [ ] Create Docker Hub account (if not exists)
2. [ ] Create Docker Hub repository: `<username>/timetracker`
3. [ ] Generate Docker Hub access token
4. [ ] Store token in Azure Key Vault for each environment
5. [ ] Document Docker Hub credentials management

### Phase 2: Code Changes (Day 1-2)
6. [ ] Update [deployment/bicep/main.bicep](deployment/bicep/main.bicep) to remove ACR module
7. [ ] Update [deployment/bicep/modules/appservice.bicep](deployment/bicep/modules/appservice.bicep) for Docker Hub
8. [ ] Update parameter files (dev, staging, prod)
9. [ ] Update [deployment/scripts/build-and-push.sh](deployment/scripts/build-and-push.sh)
10. [ ] Update [deployment/scripts/deploy.sh](deployment/scripts/deploy.sh)
11. [ ] Review and update [deployment/scripts/rollback.sh](deployment/scripts/rollback.sh)

### Phase 3: Documentation (Day 2)
12. [ ] Update [deployment/docs/DEPLOYMENT.md](deployment/docs/DEPLOYMENT.md)
13. [ ] Update [deployment/docs/ARCHITECTURE.md](deployment/docs/ARCHITECTURE.md)
14. [ ] Update [README.md](README.md) with Docker Hub instructions
15. [ ] Create Docker Hub quick reference guide

### Phase 4: Testing (Day 3)
16. [ ] Build and push first image to Docker Hub
17. [ ] Deploy to dev environment using Docker Hub image
18. [ ] Verify application functionality
19. [ ] Test image pulls and authentication
20. [ ] Deploy to staging for validation

### Phase 5: Production Rollout (Day 4)
21. [ ] Deploy to production environment
22. [ ] Monitor startup and performance
23. [ ] Verify all functionality
24. [ ] Update runbooks and operational docs
25. [ ] Archive ACR resources (after validation period)

## Decision Points

### Public vs Private Repository

**Public Repository:**
- ✅ Pros: No auth needed, easy sharing, promotes open source
- ❌ Cons: Source code visibility, potential security exposure

**Private Repository:**
- ✅ Pros: Security, access control
- ❌ Cons: Requires credential management, limited pull rates

**Recommendation**: Start with private repository, transition to public after security review.

### Image Tagging Strategy

**Options:**
1. **Git SHA**: `timetracker:abc123def` (immutable, traceable)
2. **Semantic Version**: `timetracker:v1.2.3` (clear versioning)
3. **Environment + Version**: `timetracker:prod-v1.2.3` (environment-specific)
4. **Latest + Version**: Both `latest` and specific tag

**Recommendation**: Use Git SHA + semantic version + latest
```bash
docker tag image username/timetracker:v1.2.3
docker tag image username/timetracker:abc123def
docker tag image username/timetracker:latest
```

### Credential Storage

**Options:**
1. **Azure Key Vault** (recommended for Azure deployments)
2. **GitHub Secrets** (for CI/CD)
3. **Azure App Service Configuration** (encrypted at rest)
4. **Environment Variables** (local development only)

**Recommendation**: Azure Key Vault with references in Bicep templates

## Rollback Plan

If Docker Hub deployment has issues:

1. **Immediate**: Keep ACR resources during transition period
2. **Fallback**: Bicep templates can switch back to ACR with parameter change
3. **Timeline**: 30-day validation period before decommissioning ACR

## Cost Analysis

### Current (ACR-based)
- Dev ACR: ~$5/month (Basic)
- Staging ACR: ~$20/month (Standard)
- Prod ACR: ~$20/month (Standard)
- **Total**: ~$45/month

### Proposed (Docker Hub)
- Free tier: Unlimited public repositories
- Pro account: $5/month (private repositories, better performance)
- **Total**: $0-5/month

**Savings**: ~$40-45/month (~$480-540/year)

## Risk Mitigation

### Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Docker Hub outage | High | Cache images in App Service, maintain ACR backup |
| Rate limiting | Medium | Use Pro account, implement retry logic |
| Authentication failure | High | Test thoroughly, use Key Vault references |
| Image pull slow | Low | Use Azure regions close to Docker Hub CDN |
| Security exposure | High | Use private repository, implement security scanning |

## Success Criteria

- [ ] Application successfully deploys from Docker Hub image
- [ ] No degradation in deployment time (< 5 minutes)
- [ ] All environments (dev, staging, prod) migrated
- [ ] Documentation updated and validated
- [ ] Team trained on new workflow
- [ ] Cost savings realized
- [ ] ACR resources decommissioned (after validation period)

## Next Actions

1. **Immediate**: Review and approve this plan
2. **Week 1**: Complete implementation phases 1-3
3. **Week 2**: Complete testing and production rollout
4. **Week 3**: Monitor and optimize
5. **Week 4**: Decommission ACR resources

## References

- [Docker Hub Documentation](https://docs.docker.com/docker-hub/)
- [Azure App Service Container Configuration](https://learn.microsoft.com/en-us/azure/app-service/configure-custom-container)
- [Azure Key Vault References](https://learn.microsoft.com/en-us/azure/app-service/app-service-key-vault-references)
- [Docker Hub Rate Limits](https://docs.docker.com/docker-hub/download-rate-limit/)
