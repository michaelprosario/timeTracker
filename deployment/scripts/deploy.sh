#!/bin/bash
set -e

# ============================================================================
# Azure Deployment Script for Time Tracker Application
# ============================================================================
# This script deploys the Time Tracker application infrastructure to Azure
# using Bicep templates following the Deployment Stamps pattern.
# ============================================================================

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if required tools are installed
check_prerequisites() {
    print_info "Checking prerequisites..."
    
    if ! command -v az &> /dev/null; then
        print_error "Azure CLI is not installed. Please install it from https://docs.microsoft.com/cli/azure/install-azure-cli"
        exit 1
    fi
    
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed. Please install it from https://docs.docker.com/get-docker/"
        exit 1
    fi
    
    print_success "All prerequisites are met"
}

# Function to display usage
usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Deploy Time Tracker application to Azure using Bicep templates.

OPTIONS:
    -e, --environment       Environment to deploy (dev, staging, prod) [required]
    -r, --resource-group    Resource group name [optional, auto-generated if not provided]
    -l, --location          Azure region (default: eastus)
    -s, --skip-build        Skip Docker image build
    -b, --skip-bicep        Skip infrastructure deployment (use existing)
    -m, --run-migrations    Run database migrations after deployment
    -h, --help              Display this help message

EXAMPLES:
    # Deploy to development environment
    $0 --environment dev

    # Deploy to production with custom resource group
    $0 --environment prod --resource-group rg-timetracker-prod --location westus2

    # Deploy only the application (skip infrastructure)
    $0 --environment staging --skip-bicep --run-migrations

ENVIRONMENT VARIABLES:
    POSTGRESQL_ADMIN_PASSWORD   PostgreSQL administrator password (required)
    DOCKER_IMAGE_TAG            Docker image tag (default: latest)

EOF
}

# Default values
ENVIRONMENT=""
RESOURCE_GROUP=""
LOCATION="eastus"
SKIP_BUILD=false
SKIP_BICEP=false
RUN_MIGRATIONS=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -e|--environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -r|--resource-group)
            RESOURCE_GROUP="$2"
            shift 2
            ;;
        -l|--location)
            LOCATION="$2"
            shift 2
            ;;
        -s|--skip-build)
            SKIP_BUILD=true
            shift
            ;;
        -b|--skip-bicep)
            SKIP_BICEP=true
            shift
            ;;
        -m|--run-migrations)
            RUN_MIGRATIONS=true
            shift
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            usage
            exit 1
            ;;
    esac
done

# Validate required parameters
if [ -z "$ENVIRONMENT" ]; then
    print_error "Environment is required"
    usage
    exit 1
fi

if [[ ! "$ENVIRONMENT" =~ ^(dev|staging|prod)$ ]]; then
    print_error "Environment must be one of: dev, staging, prod"
    exit 1
fi

# Check for PostgreSQL password
if [ -z "$POSTGRESQL_ADMIN_PASSWORD" ]; then
    print_warning "POSTGRESQL_ADMIN_PASSWORD not set. Reading from input..."
    read -sp "Enter PostgreSQL admin password: " POSTGRESQL_ADMIN_PASSWORD
    echo
    export POSTGRESQL_ADMIN_PASSWORD
fi

# Set default resource group if not provided
if [ -z "$RESOURCE_GROUP" ]; then
    RESOURCE_GROUP="rg-timetracker-${ENVIRONMENT}"
fi

# Set Docker image tag
if [ -z "$DOCKER_IMAGE_TAG" ]; then
    DOCKER_IMAGE_TAG="$(date +%Y%m%d-%H%M%S)"
    export DOCKER_IMAGE_TAG
fi

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$(dirname "$(dirname "$SCRIPT_DIR")")"
BICEP_DIR="${PROJECT_ROOT}/deployment/bicep"

print_info "========================================"
print_info "Time Tracker Azure Deployment"
print_info "========================================"
print_info "Environment:      ${ENVIRONMENT}"
print_info "Resource Group:   ${RESOURCE_GROUP}"
print_info "Location:         ${LOCATION}"
print_info "Docker Tag:       ${DOCKER_IMAGE_TAG}"
print_info "Skip Build:       ${SKIP_BUILD}"
print_info "Skip Bicep:       ${SKIP_BICEP}"
print_info "Run Migrations:   ${RUN_MIGRATIONS}"
print_info "========================================"

# Check prerequisites
check_prerequisites

# Login to Azure
print_info "Checking Azure login status..."
if ! az account show &> /dev/null; then
    print_info "Not logged in to Azure. Logging in..."
    az login
fi

SUBSCRIPTION_ID=$(az account show --query id -o tsv)
print_success "Using Azure subscription: ${SUBSCRIPTION_ID}"

# Create resource group if it doesn't exist
print_info "Creating resource group '${RESOURCE_GROUP}' in '${LOCATION}'..."
az group create \
    --name "${RESOURCE_GROUP}" \
    --location "${LOCATION}" \
    --tags Environment="${ENVIRONMENT}" Application="TimeTracker" \
    --output none

print_success "Resource group ready"

# Deploy infrastructure with Bicep
if [ "$SKIP_BICEP" = false ]; then
    print_info "Deploying infrastructure with Bicep..."
    
    DEPLOYMENT_NAME="timetracker-${ENVIRONMENT}-$(date +%Y%m%d-%H%M%S)"
    
    az deployment group create \
        --name "${DEPLOYMENT_NAME}" \
        --resource-group "${RESOURCE_GROUP}" \
        --template-file "${BICEP_DIR}/main.bicep" \
        --parameters "${BICEP_DIR}/parameters/${ENVIRONMENT}.bicepparam" \
        --parameters dockerImageTag="${DOCKER_IMAGE_TAG}" \
        --output json > deployment-output.json
    
    print_success "Infrastructure deployed successfully"
    
    # Extract outputs
    ACR_NAME=$(jq -r '.properties.outputs.acrName.value' deployment-output.json)
    ACR_LOGIN_SERVER=$(jq -r '.properties.outputs.acrLoginServer.value' deployment-output.json)
    WEB_APP_NAME=$(jq -r '.properties.outputs.webAppName.value' deployment-output.json)
    WEB_APP_URL=$(jq -r '.properties.outputs.webAppUrl.value' deployment-output.json)
    POSTGRES_SERVER=$(jq -r '.properties.outputs.postgresqlServerName.value' deployment-output.json)
    POSTGRES_FQDN=$(jq -r '.properties.outputs.postgresqlServerFqdn.value' deployment-output.json)
    DATABASE_NAME=$(jq -r '.properties.outputs.databaseName.value' deployment-output.json)
    KEY_VAULT_NAME=$(jq -r '.properties.outputs.keyVaultName.value' deployment-output.json)
    
    print_info "ACR Name: ${ACR_NAME}"
    print_info "Web App Name: ${WEB_APP_NAME}"
    print_info "Web App URL: ${WEB_APP_URL}"
    print_info "PostgreSQL Server: ${POSTGRES_SERVER}"
    print_info "Key Vault: ${KEY_VAULT_NAME}"
else
    print_warning "Skipping infrastructure deployment"
    # Need to retrieve existing values
    print_info "Retrieving existing resource information..."
    
    ACR_NAME=$(az acr list --resource-group "${RESOURCE_GROUP}" --query "[0].name" -o tsv)
    ACR_LOGIN_SERVER=$(az acr show --name "${ACR_NAME}" --query loginServer -o tsv)
    WEB_APP_NAME=$(az webapp list --resource-group "${RESOURCE_GROUP}" --query "[0].name" -o tsv)
    WEB_APP_URL="https://$(az webapp show --name "${WEB_APP_NAME}" --resource-group "${RESOURCE_GROUP}" --query defaultHostName -o tsv)"
fi

# Build and push Docker image
if [ "$SKIP_BUILD" = false ]; then
    print_info "Building Docker image..."
    
    # Login to ACR
    az acr login --name "${ACR_NAME}"
    
    # Build the image
    docker build \
        -t "${ACR_LOGIN_SERVER}/timetracker:${DOCKER_IMAGE_TAG}" \
        -t "${ACR_LOGIN_SERVER}/timetracker:latest" \
        -f "${PROJECT_ROOT}/Dockerfile" \
        "${PROJECT_ROOT}"
    
    print_success "Docker image built successfully"
    
    # Push to ACR
    print_info "Pushing image to Azure Container Registry..."
    docker push "${ACR_LOGIN_SERVER}/timetracker:${DOCKER_IMAGE_TAG}"
    docker push "${ACR_LOGIN_SERVER}/timetracker:latest"
    
    print_success "Image pushed to ACR"
    
    # Restart web app to pull new image
    print_info "Restarting web app to pull new image..."
    az webapp restart --name "${WEB_APP_NAME}" --resource-group "${RESOURCE_GROUP}"
    
    print_success "Web app restarted"
else
    print_warning "Skipping Docker build and push"
fi

# Run database migrations
if [ "$RUN_MIGRATIONS" = true ]; then
    print_info "Running database migrations..."
    "${SCRIPT_DIR}/run-migrations.sh" \
        --resource-group "${RESOURCE_GROUP}" \
        --web-app "${WEB_APP_NAME}"
    print_success "Migrations completed"
fi

# Display deployment summary
print_info ""
print_info "========================================"
print_success "Deployment Complete!"
print_info "========================================"
print_info "Web Application URL: ${WEB_APP_URL}"
print_info "Resource Group: ${RESOURCE_GROUP}"
print_info "Environment: ${ENVIRONMENT}"
print_info "Docker Image Tag: ${DOCKER_IMAGE_TAG}"
print_info "========================================"
print_info ""
print_info "Next steps:"
print_info "1. Run migrations: ./scripts/run-migrations.sh --resource-group ${RESOURCE_GROUP} --web-app ${WEB_APP_NAME}"
print_info "2. View logs: az webapp log tail --name ${WEB_APP_NAME} --resource-group ${RESOURCE_GROUP}"
print_info "3. Open application: ${WEB_APP_URL}"
print_info ""

# Save deployment info
cat > deployment-info.txt << EOF
Deployment Information
=====================
Timestamp: $(date)
Environment: ${ENVIRONMENT}
Resource Group: ${RESOURCE_GROUP}
Location: ${LOCATION}
Docker Image Tag: ${DOCKER_IMAGE_TAG}

Resources:
- Web App: ${WEB_APP_NAME}
- Web App URL: ${WEB_APP_URL}
- ACR: ${ACR_NAME}
- ACR Login Server: ${ACR_LOGIN_SERVER}

Commands:
- View logs: az webapp log tail --name ${WEB_APP_NAME} --resource-group ${RESOURCE_GROUP}
- SSH to container: az webapp ssh --name ${WEB_APP_NAME} --resource-group ${RESOURCE_GROUP}
- Run migrations: ./scripts/run-migrations.sh --resource-group ${RESOURCE_GROUP} --web-app ${WEB_APP_NAME}
EOF

print_success "Deployment info saved to deployment-info.txt"
