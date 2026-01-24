using './main.bicep'

// Staging Environment Parameters
param stampName = 'staging'
param environment = 'staging'
param postgresqlAdminUsername = 'ttadmin'
param postgresqlAdminPassword = readEnvironmentVariable('POSTGRESQL_ADMIN_PASSWORD')
param dockerImageTag = readEnvironmentVariable('DOCKER_IMAGE_TAG', 'latest')
