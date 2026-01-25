using '../main.bicep'

// Development Environment Parameters
param stampName = 'dev'
param environment = 'dev'
param postgresqlAdminUsername = 'ttadmin'
param postgresqlAdminPassword = readEnvironmentVariable('POSTGRESQL_ADMIN_PASSWORD', 'CHANGE_ME_IN_PRODUCTION')
param dockerImageTag = 'latest'
