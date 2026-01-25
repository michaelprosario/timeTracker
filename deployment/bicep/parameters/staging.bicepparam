using '../main.bicep'

// Staging Environment Parameters
param stampName = 'staging'
param environment = 'staging'
param postgresqlAdminUsername = 'ttadmin'
param postgresqlAdminPassword = readEnvironmentVariable('POSTGRESQL_ADMIN_PASSWORD')
param dockerImageTag = readEnvironmentVariable('DOCKER_IMAGE_TAG', 'latest')

// Docker Hub Configuration
param dockerHubRepository = readEnvironmentVariable('DOCKERHUB_REPOSITORY', 'michaelprosario/timetracker')
param dockerHubUsername = readEnvironmentVariable('DOCKERHUB_USERNAME', '')
param dockerHubPassword = readEnvironmentVariable('DOCKERHUB_PASSWORD', '')
