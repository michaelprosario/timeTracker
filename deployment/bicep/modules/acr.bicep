// Azure Container Registry Module
// Creates a private container registry for the application

@description('The name of the container registry')
param acrName string

@description('The location for the container registry')
param location string

@description('The SKU of the container registry')
@allowed([
  'Basic'
  'Standard'
  'Premium'
])
param sku string = 'Basic'

@description('Enable admin user')
param adminUserEnabled bool = true

@description('Tags to apply to resources')
param tags object = {}

// Container Registry
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: acrName
  location: location
  tags: tags
  sku: {
    name: sku
  }
  properties: {
    adminUserEnabled: adminUserEnabled
    publicNetworkAccess: 'Enabled'
    networkRuleBypassOptions: 'AzureServices'
    policies: {
      retentionPolicy: {
        days: 7
        status: 'enabled'
      }
    }
  }
}

// Outputs
output acrId string = containerRegistry.id
output acrName string = containerRegistry.name
output acrLoginServer string = containerRegistry.properties.loginServer
output acrPassword string = listCredentials(containerRegistry.id, '2023-07-01').passwords[0].value
