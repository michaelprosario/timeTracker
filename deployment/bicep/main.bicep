// Main Bicep Template for Time Tracker Application
// Orchestrates deployment of all infrastructure components following the Deployment Stamps pattern

targetScope = 'resourceGroup'

// ============================================================================
// Parameters
// ============================================================================

@description('The name of the deployment stamp (e.g., dev, staging, prod, east, west)')
param stampName string

@description('The Azure region for deployment')
param location string = resourceGroup().location

@description('The environment name (dev, staging, prod)')
@allowed([
  'dev'
  'staging'
  'prod'
])
param environment string

@description('PostgreSQL administrator username')
@secure()
param postgresqlAdminUsername string

@description('PostgreSQL administrator password')
@minLength(8)
@secure()
param postgresqlAdminPassword string

@description('The Docker image tag to deploy')
param dockerImageTag string = 'latest'

@description('Common tags to apply to all resources')
param tags object = {
  Environment: environment
  Application: 'TimeTracker'
  ManagedBy: 'Bicep'
  Stamp: stampName
}

// ============================================================================
// Variables
// ============================================================================

var prefix = 'tt-${stampName}'
var uniqueSuffix = uniqueString(resourceGroup().id, stampName)

var naming = {
  vnet: '${prefix}-vnet'
  keyVault: 'kv-${stampName}-${uniqueSuffix}'
  postgresql: '${prefix}-postgres-${uniqueSuffix}'
  acr: 'acr${stampName}${uniqueSuffix}'
  appServicePlan: '${prefix}-asp'
  webApp: '${prefix}-web-${uniqueSuffix}'
  logAnalytics: '${prefix}-logs'
  appInsights: '${prefix}-ai'
}

var skuConfig = environment == 'prod' ? {
  appService: 'P1V3'
  postgresql: {
    name: 'Standard_D2s_v3'
    tier: 'GeneralPurpose'
  }
  acr: 'Standard'
} : environment == 'staging' ? {
  appService: 'S1'
  postgresql: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  acr: 'Basic'
} : {
  appService: 'B1'
  postgresql: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  acr: 'Basic'
}

// ============================================================================
// Module Deployments
// ============================================================================

// 1. Networking
module vnetModule 'modules/vnet.bicep' = {
  name: 'deploy-vnet'
  params: {
    vnetName: naming.vnet
    location: location
    addressPrefix: '10.0.0.0/16'
    tags: tags
  }
}

// 2. Monitoring (needed for App Service)
module monitoringModule 'modules/monitoring.bicep' = {
  name: 'deploy-monitoring'
  params: {
    workspaceName: naming.logAnalytics
    appInsightsName: naming.appInsights
    location: location
    tags: tags
  }
}

// 3. Container Registry
module acrModule 'modules/acr.bicep' = {
  name: 'deploy-acr'
  params: {
    acrName: naming.acr
    location: location
    sku: skuConfig.acr
    adminUserEnabled: true
    tags: tags
  }
}

// 4. PostgreSQL Database
module postgresqlModule 'modules/postgresql.bicep' = {
  name: 'deploy-postgresql'
  params: {
    serverName: naming.postgresql
    location: location
    administratorLogin: postgresqlAdminUsername
    administratorPassword: postgresqlAdminPassword
    postgresqlVersion: '16'
    skuName: skuConfig.postgresql.name
    tier: skuConfig.postgresql.tier
    storageSizeGB: environment == 'prod' ? 128 : 32
    backupRetentionDays: environment == 'prod' ? 35 : 7
    geoRedundantBackup: environment == 'prod'
    subnetId: vnetModule.outputs.postgresqlSubnetId
    databaseName: 'timetracker'
    tags: tags
  }
}

// 5. App Service
module appServiceModule 'modules/appservice.bicep' = {
  name: 'deploy-appservice'
  params: {
    appServicePlanName: naming.appServicePlan
    webAppName: naming.webApp
    location: location
    sku: skuConfig.appService
    acrLoginServer: acrModule.outputs.acrLoginServer
    dockerImageName: 'timetracker:${dockerImageTag}'
    acrUsername: naming.acr
    acrPassword: listCredentials(acrModule.outputs.acrId, '2023-07-01').passwords[0].value
    subnetId: vnetModule.outputs.appServiceSubnetId
    appInsightsConnectionString: monitoringModule.outputs.appInsightsConnectionString
    tags: tags
  }
}

// 6. Key Vault (after App Service for managed identity)
module keyVaultModule 'modules/keyvault.bicep' = {
  name: 'deploy-keyvault'
  params: {
    keyVaultName: naming.keyVault
    location: location
    tenantId: subscription().tenantId
    appServicePrincipalId: appServiceModule.outputs.webAppPrincipalId
    enablePublicAccess: environment != 'prod'
    tags: tags
  }
}

// ============================================================================
// Store secrets in Key Vault
// ============================================================================

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: naming.keyVault
  dependsOn: [
    keyVaultModule
  ]
}

// PostgreSQL Connection String Secret
resource postgresqlConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ConnectionStrings--DefaultConnection'
  properties: {
    value: postgresqlModule.outputs.connectionString
    contentType: 'text/plain'
  }
}

// PostgreSQL Admin Password Secret
resource postgresqlAdminPasswordSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'PostgreSQL--AdminPassword'
  properties: {
    value: postgresqlAdminPassword
    contentType: 'text/plain'
  }
}

// ACR Credentials
resource acrPasswordSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ACR--Password'
  properties: {
    value: listCredentials(acrModule.outputs.acrId, '2023-07-01').passwords[0].value
    contentType: 'text/plain'
  }
}

// ============================================================================
// Update App Service with Key Vault References
// ============================================================================

resource webApp 'Microsoft.Web/sites@2023-01-01' existing = {
  name: naming.webApp
  dependsOn: [
    appServiceModule
  ]
}

// Update app settings to use Key Vault references
resource webAppConfig 'Microsoft.Web/sites/config@2023-01-01' = {
  parent: webApp
  name: 'appsettings'
  properties: {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE: 'false'
    DOCKER_REGISTRY_SERVER_URL: 'https://${acrModule.outputs.acrLoginServer}'
    DOCKER_REGISTRY_SERVER_USERNAME: '@Microsoft.KeyVault(SecretUri=https://${naming.keyVault}.vault.azure.net/secrets/ACR--Password/)'
    DOCKER_REGISTRY_SERVER_PASSWORD: '@Microsoft.KeyVault(SecretUri=https://${naming.keyVault}.vault.azure.net/secrets/ACR--Password/)'
    WEBSITES_PORT: '8080'
    ASPNETCORE_ENVIRONMENT: environment == 'prod' ? 'Production' : 'Development'
    APPLICATIONINSIGHTS_CONNECTION_STRING: monitoringModule.outputs.appInsightsConnectionString
    ApplicationInsightsAgent_EXTENSION_VERSION: '~3'
    ConnectionStrings__DefaultConnection: '@Microsoft.KeyVault(SecretUri=https://${naming.keyVault}.vault.azure.net/secrets/ConnectionStrings--DefaultConnection/)'
  }
  dependsOn: [
    postgresqlConnectionStringSecret
    acrPasswordSecret
  ]
}

// ============================================================================
// Outputs
// ============================================================================

output resourceGroupName string = resourceGroup().name
output location string = location
output stampName string = stampName

// Networking
output vnetId string = vnetModule.outputs.vnetId
output vnetName string = vnetModule.outputs.vnetName

// App Service
output webAppUrl string = appServiceModule.outputs.webAppUrl
output webAppName string = appServiceModule.outputs.webAppName
output webAppPrincipalId string = appServiceModule.outputs.webAppPrincipalId

// Database
output postgresqlServerName string = postgresqlModule.outputs.serverName
output postgresqlServerFqdn string = postgresqlModule.outputs.serverFqdn
output databaseName string = postgresqlModule.outputs.databaseName

// Container Registry
output acrLoginServer string = acrModule.outputs.acrLoginServer
output acrName string = acrModule.outputs.acrName

// Key Vault
output keyVaultName string = keyVaultModule.outputs.keyVaultName
output keyVaultUri string = keyVaultModule.outputs.keyVaultUri

// Monitoring
output appInsightsName string = monitoringModule.outputs.appInsightsName
output appInsightsConnectionString string = monitoringModule.outputs.appInsightsConnectionString
output logAnalyticsWorkspaceName string = monitoringModule.outputs.workspaceName

// Deployment Information
output deploymentTimestamp string = utcNow()
output dockerImageTag string = dockerImageTag
