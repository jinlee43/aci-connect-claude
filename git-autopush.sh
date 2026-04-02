#!/bin/bash
LOG="$HOME/aci-connect-autopush.log"

log() {
  echo "$(date '+%Y-%m-%d %H:%M:%S') $1" | tee -a "$LOG"
}

cd "$(dirname "$0")"

log "========== AutoPush Start =========="

STATUS=$(git status --porcelain)
if [ -z "$STATUS" ]; then
  log "No changes — skipped"
  exit 0
fi

log "Changes detected:"
git status --short 2>&1 | tee -a "$LOG"

git add .
git commit -m "auto: nightly backup $(date +%Y-%m-%d)" 2>&1 | tee -a "$LOG"
git push origin main 2>&1 | tee -a "$LOG"

if [ $? -eq 0 ]; then
  log "SUCCESS: pushed to GitHub"
else
  log "ERROR: push failed"
fi
