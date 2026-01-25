// App Service Module
// Creates App Service Plan and Web App with container support

@description('The name of the App Service Plan')
param appServicePlanName string

@description('The name of the Web App')
param webAppName string

@description('The location for the resources')
param location string

@description('The SKU for the App Service Plan')
param sku string = 'B1'

@description('Docker Hub repository (e.g., username/timetracker)')
param dockerHubRepository string

@description('The Docker image tag')
param dockerImageTag string = 'latest'

@description('Docker Hub username (optional for public repos)')
param dockerHubUsername string = ''

@description('Docker Hub password or token (optional for public repos)')
@secure()
param dockerHubPassword string = ''

@description('The subnet ID for VNet integration')
param subnetId string = ''

@description('Application Insights connection string')
param appInsightsConnectionString string = ''

@description('Tags to apply to resources')
param tags object = {}

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  kind: 'linux'
  sku: {
    name: sku
  }
  properties: {
    reserved: true // Required for Linux
  }
}

// Web App
resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: webAppName
  location: location
  tags: tags
  kind: 'app,linux,container'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    virtualNetworkSubnetId: !empty(subnetId) ? subnetId : null
    siteConfig: {
      linuxFxVersion: 'DOCKER|${dockerHubRepository}:${dockerImageTag}'
      alwaysOn: true
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      appSettings: concat([
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
        {
          name: 'WEBSITES_PORT'
          value: '8080'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
      ], !empty(dockerHubUsername) ? [
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://index.docker.io/v1'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: dockerHubUsername
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: dockerHubPassword
        }
      ] : [])
    }
  }
}

// Configure diagnostic settings
resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${webAppName}-diagnostics'
  scope: webApp
  properties: {
    logs: [
      {
        category: 'AppServiceHTTPLogs'
        enabled: true
      }
      {
        category: 'AppServiceConsoleLogs'
        enabled: true
      }
      {
        category: 'AppServiceAppLogs'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

// Outputs
output appServicePlanId string = appServicePlan.id
output webAppId string = webApp.id
output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output webAppPrincipalId string = webApp.identity.principalId
output webAppDefaultHostName string = webApp.properties.defaultHostName
