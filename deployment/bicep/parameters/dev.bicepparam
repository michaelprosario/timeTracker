using '../main.bicep'

// Development Environment Parameters
param stampName = 'dev'
param environment = 'dev'
param postgresqlAdminUsername = 'ttadmin'
param postgresqlAdminPassword = readEnvironmentVariable('POSTGRESQL_ADMIN_PASSWORD', 'CHANGE_ME_IN_PRODUCTION')
param dockerImageTag = 'latest'

// Docker Hub Configuration
param dockerHubRepository = readEnvironmentVariable('DOCKERHUB_REPOSITORY', 'michaelprosario/timetracker')
param dockerHubUsername = readEnvironmentVariable('DOCKERHUB_USERNAME', '')
param dockerHubPassword = readEnvironmentVariable('DOCKERHUB_PASSWORD', '')
