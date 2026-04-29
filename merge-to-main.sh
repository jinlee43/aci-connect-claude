#!/bin/bash
set -e

LOG="$HOME/aci-connect-merge.log"

log() {
  echo "$(date '+%Y-%m-%d %H:%M:%S') $1" | tee -a "$LOG"
}

cd "$(dirname "$0")"

log "========== Merge to Main Start =========="

# Verify we are on dev branch
BRANCH=$(git rev-parse --abbrev-ref HEAD)
if [ "$BRANCH" != "dev" ]; then
  log "ERROR: Current branch is '$BRANCH'. Must be on dev branch."
  exit 1
fi

# Check for uncommitted changes
STATUS=$(git status --porcelain)
if [ -n "$STATUS" ]; then
  log "ERROR: Uncommitted changes detected. Commit or stash before merging."
  git status --short
  exit 1
fi

log "Current branch: dev ✓"

# Switch to main and merge
log "Switching to main..."
git checkout main

log "Pulling latest main..."
git pull origin main

log "Merging dev into main..."
git merge dev --no-ff -m "merge: deploy $(date +%Y-%m-%d)"

log "Pushing main to GitHub..."
git push origin main

log "Merge and push complete ✓"

# Return to dev
log "Returning to dev branch..."
git checkout dev

log "========== Merge to Main Complete =========="
