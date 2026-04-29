# ACI Connect — Claude Working Principles

## File Modification Policy
**Never create, modify, or delete any file without explicit permission from the user.**
Always ask first: "May I modify this file?" or "Shall I create this file?" — even when it seems obviously necessary from the context.

## UI Language
All user-facing UI strings must be in **English only**.
- Applies to: HTML text, button labels, placeholders, validation messages, TempData messages, API error responses
- Korean is allowed in: code comments, server logs, developer docs

## EF Migrations
**Never directly edit `AppDbContextModelSnapshot.cs`.**
Always use `dotnet ef migrations add <Name>` to generate migrations. EF Core will update the snapshot automatically.

## Capability Questions
When the user asks "Can we...?" or "Is it possible to...?", answer with **Yes/No first**, then follow with details or recommendations.

## Tech Stack
- ASP.NET Core Razor Pages, .NET 10
- PostgreSQL (user: bpms)
- EF Core (code-first, migrations)
- Bootstrap 5

## Branch Strategy
- `dev` — daily development, nightly auto-push via `git-autopush.sh`
- `main` — production only, merged via `merge-to-main.sh` before each deploy

## Deployment
- Production server: Ubuntu, `/var/www/aci-connect/`
- Deploy script: `~/deploy.sh` on production server
- Shared config: `/var/www/aci-connect/shared/appsettings.Production.json`
- Rollback: symlink swap via `~/rollback.sh`

## Editor Preference
Use `vi` (not `nano`) when suggesting terminal text editor commands.
