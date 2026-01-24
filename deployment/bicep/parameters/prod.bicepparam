using './main.bicep'

// Production Environment Parameters
param stampName = 'prod'
param environment = 'prod'
param postgresqlAdminUsername = 'ttadmin'
param postgresqlAdminPassword = readEnvironmentVariable('POSTGRESQL_ADMIN_PASSWORD')
param dockerImageTag = readEnvironmentVariable('DOCKER_IMAGE_TAG')
