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

log "Current branch: dev ✓"

# ACI-System/ 및 루트 설정 파일 자동 스테이징
git add ACI-System/ CLAUDE.md .gitignore 2>/dev/null || true

# 스테이징된 변경사항이 있으면 커밋
if ! git diff --cached --quiet; then
  # 커밋 메시지: 인자로 전달하거나 직접 입력
  if [ -n "$1" ]; then
    COMMIT_MSG="$1"
  else
    echo -n "Commit message: "
    read COMMIT_MSG
    if [ -z "$COMMIT_MSG" ]; then
      COMMIT_MSG="deploy: $(date '+%Y-%m-%d')"
    fi
  fi
  log "Committing: $COMMIT_MSG"
  git commit -m "$COMMIT_MSG"
else
  log "Nothing to commit."
fi

# dev → origin push
log "Pushing dev to GitHub..."
git push origin dev

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
