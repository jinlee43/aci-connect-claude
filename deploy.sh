#!/bin/bash
set -e

APP_DIR="/var/www/aci-connect"
SOURCE_DIR="$HOME/aci-connect-source"
REPO_URL="https://github.com/jinlee43/aci-connect-claude.git"
DB_NAME="aci_v4"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
RELEASE="$APP_DIR/releases/$TIMESTAMP"

log() {
  echo "$(date '+%Y-%m-%d %H:%M:%S') $1"
}

log "========== Deploy Start: $TIMESTAMP =========="

# ── 1. Prepare source (clone if not exists, pull if exists) ──────────────────
if [ ! -d "$SOURCE_DIR/.git" ]; then
  log "Source not found — cloning from GitHub..."
  git clone "$REPO_URL" "$SOURCE_DIR"
  cd "$SOURCE_DIR"
else
  log "Source found — pulling latest code..."
  cd "$SOURCE_DIR"
  git pull origin main
fi

# Verify we are on main branch
BRANCH=$(git rev-parse --abbrev-ref HEAD)
if [ "$BRANCH" != "main" ]; then
  log "ERROR: Current branch is '$BRANCH'. Must be on main branch."
  exit 1
fi
log "Branch check: $BRANCH ✓"

# ── 2. Build ──────────────────────────────────────────────────────────────────
log "Building..."
dotnet publish ACI-System/ACI.Web/ACI.Web.csproj \
    -c Release -o "$RELEASE" --no-self-contained
log "Build complete → $RELEASE"

# ── 3. Link shared files ──────────────────────────────────────────────────────
ln -sf "$APP_DIR/shared/appsettings.Production.json" \
       "$RELEASE/appsettings.Production.json"
ln -sf "$APP_DIR/shared/uploads" \
       "$RELEASE/wwwroot/uploads"
log "Shared file links created"

# ── 4. DB backup ──────────────────────────────────────────────────────────────
mkdir -p "$APP_DIR/shared/backups"
sudo -u postgres pg_dump "$DB_NAME" > "$APP_DIR/shared/backups/db_$TIMESTAMP.sql"
log "DB backup complete → db_$TIMESTAMP.sql"

# ── 5. DB migration ───────────────────────────────────────────────────────────
log "Running DB migration..."
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update --project ACI-System/ACI.Web
log "Migration complete"

# ── 6. Swap symlink and restart service ───────────────────────────────────────
ln -sfn "$RELEASE" "$APP_DIR/current"
sudo systemctl restart aci-connect
log "Service restarted"

# ── 7. Clean up old releases (keep last 5) ────────────────────────────────────
ls -dt "$APP_DIR/releases"/* | tail -n +6 | xargs rm -rf 2>/dev/null || true
log "Old releases cleaned up"

log "========== Deploy Complete: $TIMESTAMP =========="
echo ""
echo "Current releases:"
ls -dt "$APP_DIR/releases"/* | xargs -I{} basename {}
