#!/bin/bash
set -e

# ============================================================================
# Build and Push Docker Image to Azure Container Registry
# ============================================================================

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m'

print_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Build Docker image and push to Azure Container Registry.

OPTIONS:
    -r, --resource-group    Resource group name [required]
    -a, --acr-name         ACR name [optional, will auto-detect]
    -t, --tag              Docker image tag (default: latest)
    -p, --push-dockerhub   Also push to Docker Hub (requires DOCKERHUB_USERNAME)
    -h, --help             Display this help message

EXAMPLES:
    # Build and push to ACR
    $0 --resource-group rg-timetracker-dev

    # Build and push to both ACR and Docker Hub
    $0 --resource-group rg-timetracker-prod --tag v1.0.0 --push-dockerhub

EOF
}

RESOURCE_GROUP=""
ACR_NAME=""
IMAGE_TAG="latest"
PUSH_DOCKERHUB=false

while [[ $# -gt 0 ]]; do
    case $1 in
        -r|--resource-group)
            RESOURCE_GROUP="$2"
            shift 2
            ;;
        -a|--acr-name)
            ACR_NAME="$2"
            shift 2
            ;;
        -t|--tag)
            IMAGE_TAG="$2"
            shift 2
            ;;
        -p|--push-dockerhub)
            PUSH_DOCKERHUB=true
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

if [ -z "$RESOURCE_GROUP" ]; then
    print_error "Resource group is required"
    usage
    exit 1
fi

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$(dirname "$(dirname "$SCRIPT_DIR")")"

print_info "Building and pushing Docker image..."
print_info "Resource Group: ${RESOURCE_GROUP}"
print_info "Image Tag: ${IMAGE_TAG}"

# Auto-detect ACR if not provided
if [ -z "$ACR_NAME" ]; then
    print_info "Auto-detecting Azure Container Registry..."
    ACR_NAME=$(az acr list --resource-group "${RESOURCE_GROUP}" --query "[0].name" -o tsv)
    
    if [ -z "$ACR_NAME" ]; then
        print_error "No ACR found in resource group ${RESOURCE_GROUP}"
        exit 1
    fi
fi

ACR_LOGIN_SERVER=$(az acr show --name "${ACR_NAME}" --query loginServer -o tsv)
print_info "Using ACR: ${ACR_NAME} (${ACR_LOGIN_SERVER})"

# Login to ACR
print_info "Logging in to Azure Container Registry..."
az acr login --name "${ACR_NAME}"
print_success "Logged in to ACR"

# Build Docker image
print_info "Building Docker image..."
docker build \
    -t "${ACR_LOGIN_SERVER}/timetracker:${IMAGE_TAG}" \
    -t "${ACR_LOGIN_SERVER}/timetracker:latest" \
    -f "${PROJECT_ROOT}/Dockerfile" \
    "${PROJECT_ROOT}"

print_success "Docker image built successfully"

# Push to ACR
print_info "Pushing image to Azure Container Registry..."
docker push "${ACR_LOGIN_SERVER}/timetracker:${IMAGE_TAG}"
docker push "${ACR_LOGIN_SERVER}/timetracker:latest"
print_success "Image pushed to ACR: ${ACR_LOGIN_SERVER}/timetracker:${IMAGE_TAG}"

# Optionally push to Docker Hub
if [ "$PUSH_DOCKERHUB" = true ]; then
    if [ -z "$DOCKERHUB_USERNAME" ]; then
        print_error "DOCKERHUB_USERNAME environment variable not set"
        exit 1
    fi
    
    print_info "Tagging image for Docker Hub..."
    docker tag "${ACR_LOGIN_SERVER}/timetracker:${IMAGE_TAG}" "${DOCKERHUB_USERNAME}/timetracker:${IMAGE_TAG}"
    docker tag "${ACR_LOGIN_SERVER}/timetracker:${IMAGE_TAG}" "${DOCKERHUB_USERNAME}/timetracker:latest"
    
    print_info "Logging in to Docker Hub..."
    docker login
    
    print_info "Pushing to Docker Hub..."
    docker push "${DOCKERHUB_USERNAME}/timetracker:${IMAGE_TAG}"
    docker push "${DOCKERHUB_USERNAME}/timetracker:latest"
    
    print_success "Image pushed to Docker Hub: ${DOCKERHUB_USERNAME}/timetracker:${IMAGE_TAG}"
fi

print_success "Build and push completed successfully!"
