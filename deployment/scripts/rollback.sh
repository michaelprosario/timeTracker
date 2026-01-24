#!/bin/bash
set -e

# ============================================================================
# Rollback Deployment
# ============================================================================

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

print_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_warning() { echo -e "${YELLOW}[WARNING]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Rollback to a previous deployment.

OPTIONS:
    -r, --resource-group    Resource group name [required]
    -w, --web-app          Web app name [optional, will auto-detect]
    -t, --tag              Docker image tag to rollback to [required]
    -s, --use-slot         Use deployment slot for rollback
    -h, --help             Display this help message

EXAMPLES:
    # Rollback to previous tag
    $0 --resource-group rg-timetracker-prod --tag 20260124-120000

    # Rollback using deployment slot
    $0 --resource-group rg-timetracker-prod --use-slot

EOF
}

RESOURCE_GROUP=""
WEB_APP_NAME=""
IMAGE_TAG=""
USE_SLOT=false

while [[ $# -gt 0 ]]; do
    case $1 in
        -r|--resource-group)
            RESOURCE_GROUP="$2"
            shift 2
            ;;
        -w|--web-app)
            WEB_APP_NAME="$2"
            shift 2
            ;;
        -t|--tag)
            IMAGE_TAG="$2"
            shift 2
            ;;
        -s|--use-slot)
            USE_SLOT=true
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

# Auto-detect web app if not provided
if [ -z "$WEB_APP_NAME" ]; then
    print_info "Auto-detecting web app..."
    WEB_APP_NAME=$(az webapp list --resource-group "${RESOURCE_GROUP}" --query "[0].name" -o tsv)
    
    if [ -z "$WEB_APP_NAME" ]; then
        print_error "No web app found in resource group ${RESOURCE_GROUP}"
        exit 1
    fi
fi

print_warning "========================================" 
print_warning "ROLLBACK OPERATION"
print_warning "========================================"
print_warning "Resource Group: ${RESOURCE_GROUP}"
print_warning "Web App: ${WEB_APP_NAME}"

if [ "$USE_SLOT" = true ]; then
    print_info "Using deployment slot swap for rollback..."
    
    # Swap staging slot with production
    print_info "Swapping 'staging' slot with 'production'..."
    az webapp deployment slot swap \
        --name "${WEB_APP_NAME}" \
        --resource-group "${RESOURCE_GROUP}" \
        --slot staging \
        --target-slot production
    
    print_success "Slot swap completed"
    
elif [ -n "$IMAGE_TAG" ]; then
    print_info "Rolling back to image tag: ${IMAGE_TAG}"
    
    # Get ACR details
    ACR_NAME=$(az acr list --resource-group "${RESOURCE_GROUP}" --query "[0].name" -o tsv)
    ACR_LOGIN_SERVER=$(az acr show --name "${ACR_NAME}" --query loginServer -o tsv)
    
    # Update web app to use previous image
    print_info "Updating web app configuration..."
    az webapp config container set \
        --name "${WEB_APP_NAME}" \
        --resource-group "${RESOURCE_GROUP}" \
        --docker-custom-image-name "${ACR_LOGIN_SERVER}/timetracker:${IMAGE_TAG}"
    
    print_info "Restarting web app..."
    az webapp restart --name "${WEB_APP_NAME}" --resource-group "${RESOURCE_GROUP}"
    
    print_success "Rollback to tag ${IMAGE_TAG} completed"
else
    print_error "Either --tag or --use-slot must be specified"
    usage
    exit 1
fi

WEB_APP_URL="https://$(az webapp show --name "${WEB_APP_NAME}" --resource-group "${RESOURCE_GROUP}" --query defaultHostName -o tsv)"

print_success "Rollback completed successfully!"
print_info "Web App URL: ${WEB_APP_URL}"
print_info "Monitor the application: az webapp log tail --name ${WEB_APP_NAME} --resource-group ${RESOURCE_GROUP}"
