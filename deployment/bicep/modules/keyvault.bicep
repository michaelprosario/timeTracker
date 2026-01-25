// Azure Key Vault Module
// Creates a Key Vault with RBAC-based access control

@description('The name of the Key Vault')
param keyVaultName string

@description('The location for the Key Vault')
param location string

@description('The tenant ID for the Key Vault')
param tenantId string

@description('The object ID of the App Service managed identity')
param appServicePrincipalId string = ''

@description('Enable public network access')
param enablePublicAccess bool = true

@description('Tags to apply to resources')
param tags object = {}

// Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enablePurgeProtection: true
    publicNetworkAccess: enablePublicAccess ? 'Enabled' : 'Disabled'
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: enablePublicAccess ? 'Allow' : 'Deny'
    }
  }
}

// Role assignment for App Service to read secrets
// Key Vault Secrets User role (RBAC)
resource appServiceSecretUserRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(appServicePrincipalId)) {
  name: guid(keyVault.id, appServicePrincipalId, 'SecretsUser')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
    principalId: appServicePrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Outputs
output keyVaultId string = keyVault.id
output keyVaultName string = keyVault.name
output keyVaultUri string = keyVault.properties.vaultUri
