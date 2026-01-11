#!/usr/bin/env bash
set -euo pipefail

echo "=============================================="
echo " Docker local development cleanup"
echo "=============================================="
echo

# Safety prompt
read -p "This will REMOVE containers, volumes, networks, and images used by Docker. Continue? (y/N): " confirm
if [[ "$confirm" != "y" && "$confirm" != "Y" ]]; then
  echo "Aborted."
  exit 0
fi

echo
echo "Stopping all running containers..."
docker ps -q | xargs -r docker stop

echo
echo "Removing all containers..."
docker ps -aq | xargs -r docker rm -f

echo
echo "Removing named volumes..."
docker volume ls -q | xargs -r docker volume rm

echo
echo "Removing unused networks..."
docker network prune -f

echo
echo "Removing dangling images..."
docker image prune -f

echo
echo "Removing build cache..."
docker builder prune -f

echo
echo "=============================================="
echo " Docker cleanup complete"
echo "=============================================="
echo
echo "IMPORTANT NEXT STEPS:"
echo "1. Restart Aspire"
echo "2. Restart all .NET apps"
echo
