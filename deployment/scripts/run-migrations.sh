#!/bin/bash
set -e

# ============================================================================
# Run Entity Framework Migrations on Azure Database
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

Run Entity Framework database migrations on Azure PostgreSQL.

OPTIONS:
    -r, --resource-group    Resource group name [required]
    -w, --web-app          Web app name [optional, will auto-detect]
    -h, --help             Display this help message

EXAMPLES:
    # Run migrations
    $0 --resource-group rg-timetracker-dev

    # Run migrations with specific web app
    $0 --resource-group rg-timetracker-prod --web-app tt-prod-web-abc123

EOF
}

RESOURCE_GROUP=""
WEB_APP_NAME=""

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

print_info "Running migrations on ${WEB_APP_NAME}..."

# Get connection string from Key Vault via App Service configuration
print_info "Retrieving connection string from app configuration..."
CONNECTION_STRING=$(az webapp config appsettings list \
    --name "${WEB_APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --query "[?name=='ConnectionStrings__DefaultConnection'].value" \
    -o tsv)

if [[ "$CONNECTION_STRING" == @Microsoft.KeyVault* ]]; then
    print_warning "Connection string is a Key Vault reference. Migrations will run inside the container."
    
    # Run migrations inside the container using az webapp ssh
    print_info "Connecting to container and running migrations..."
    
    az webapp ssh --name "${WEB_APP_NAME}" --resource-group "${RESOURCE_GROUP}" --command "cd /app && dotnet TimeTracker.Web.dll --migrate"
    
    print_success "Migrations executed inside container"
else
    print_error "Unable to retrieve connection string"
    exit 1
fi

print_success "Database migrations completed successfully!"
print_info "You can verify by checking the application logs:"
print_info "  az webapp log tail --name ${WEB_APP_NAME} --resource-group ${RESOURCE_GROUP}"
