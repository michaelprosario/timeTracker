#!/bin/bash
set -e

# ============================================================================
# Build and Push Docker Image to Docker Hub
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

Build Docker image and push to Docker Hub.

OPTIONS:
    -u, --dockerhub-username  Docker Hub username [required]
    -r, --repository          Repository name (default: timetracker)
    -t, --tag                 Docker image tag (default: latest)
    -h, --help                Display this help message

ENVIRONMENT VARIABLES:
    DOCKERHUB_USERNAME       Docker Hub username
    DOCKERHUB_PASSWORD       Docker Hub password or access token
    DOCKERHUB_REPOSITORY     Docker Hub repository (default: timetracker)

EXAMPLES:
    # Build and push to Docker Hub
    $0 --dockerhub-username myuser --tag v1.0.0

    # Using environment variables
    export DOCKERHUB_USERNAME=myuser
    export DOCKERHUB_PASSWORD=mytoken
    $0 --tag latest

EOF
}

DOCKERHUB_USERNAME="${DOCKERHUB_USERNAME:-}"
DOCKERHUB_PASSWORD="${DOCKERHUB_PASSWORD:-}"
REPOSITORY="timetracker"
IMAGE_TAG="latest"

while [[ $# -gt 0 ]]; do
    case $1 in
        -u|--dockerhub-username)
            DOCKERHUB_USERNAME="$2"
            shift 2
            ;;
        -r|--repository)
            REPOSITORY="$2"
            shift 2
            ;;
        -t|--tag)
            IMAGE_TAG="$2"
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

if [ -z "$DOCKERHUB_USERNAME" ]; then
    print_error "Docker Hub username is required"
    usage
    exit 1
fi

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$(dirname "$(dirname "$SCRIPT_DIR")")"

print_info "Building and pushing Docker image to Docker Hub..."
print_info "Docker Hub Username: ${DOCKERHUB_USERNAME}"
print_info "Repository: ${REPOSITORY}"
print_info "Image Tag: ${IMAGE_TAG}"

print_info "Docker Hub Username: ${DOCKERHUB_USERNAME}"
print_info "Repository: ${REPOSITORY}"
print_info "Image Tag: ${IMAGE_TAG}"

# Login to Docker Hub
print_info "Logging in to Docker Hub..."
if [ -n "$DOCKERHUB_PASSWORD" ]; then
    echo "$DOCKERHUB_PASSWORD" | docker login -u "${DOCKERHUB_USERNAME}" --password-stdin
else
    docker login -u "${DOCKERHUB_USERNAME}"
fi
print_success "Logged in to Docker Hub"

# Determine which Dockerfile to use
if [ -f "${PROJECT_ROOT}/deployment/docker/Dockerfile.production" ]; then
    DOCKERFILE="${PROJECT_ROOT}/deployment/docker/Dockerfile.production"
    print_info "Using production Dockerfile"
elif [ -f "${PROJECT_ROOT}/Dockerfile" ]; then
    DOCKERFILE="${PROJECT_ROOT}/Dockerfile"
    print_info "Using root Dockerfile"
else
    print_error "No Dockerfile found"
    exit 1
fi

# Build Docker image
print_info "Building Docker image..."
FULL_IMAGE_NAME="${DOCKERHUB_USERNAME}/${REPOSITORY}"

docker build \
    -t "${FULL_IMAGE_NAME}:${IMAGE_TAG}" \
    -t "${FULL_IMAGE_NAME}:latest" \
    -f "${DOCKERFILE}" \
    "${PROJECT_ROOT}"

print_success "Docker image built successfully"

# Push to Docker Hub
print_info "Pushing image to Docker Hub..."
docker push "${FULL_IMAGE_NAME}:${IMAGE_TAG}"

# Only push latest if not a specific version tag
if [ "${IMAGE_TAG}" != "latest" ]; then
    docker push "${FULL_IMAGE_NAME}:latest"
    print_success "Images pushed to Docker Hub:"
    print_success "  - ${FULL_IMAGE_NAME}:${IMAGE_TAG}"
    print_success "  - ${FULL_IMAGE_NAME}:latest"
else
    print_success "Image pushed to Docker Hub: ${FULL_IMAGE_NAME}:${IMAGE_TAG}"
fi

print_success "Build and push completed successfully!"
print_info ""
print_info "Next steps:"
print_info "  1. Deploy to Azure: ./deployment/scripts/deploy.sh -e dev -t ${IMAGE_TAG}"
print_info "  2. Or pull and run locally: docker pull ${FULL_IMAGE_NAME}:${IMAGE_TAG}"
