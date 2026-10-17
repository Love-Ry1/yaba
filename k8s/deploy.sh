#!/usr/bin/env bash
set -euo pipefail

NAMESPACE=yaba
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

minikube status >/dev/null 2>&1 || minikube start --driver=docker

kubectl apply -k "$DIR"

kubectl -n "$NAMESPACE" rollout status statefulset/db --timeout=180s
for d in event-service booking-service frontend; do
  kubectl -n "$NAMESPACE" rollout status "deploy/$d" --timeout=180s
done

echo "Deployed. Starting minikube tunnel (frontend: http://localhost:3000, Ctrl+C to stop)"
exec minikube tunnel
