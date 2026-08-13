# Frontend (Turborepo / Next.js) Code Map

Frontend workspace: [frontend/turborepo-zizibot-console](../../frontend/turborepo-zizibot-console)

## Architecture

- Monorepo toolchain: pnpm workspaces + Turborepo
  - Workspace layout: [pnpm-workspace.yaml](../../frontend/turborepo-zizibot-console/pnpm-workspace.yaml#L1-L4)
  - Root scripts: [package.json](../../frontend/turborepo-zizibot-console/package.json#L1-L33)
- App types:
  - Next.js apps (console/web/docs)
  - A small router/service app (“pendekin-router”) acting as an API adapter/proxy
- Package types:
  - Shared UI packages (shadcn/ui-based)
  - Shared contracts (DTOs)
  - REST client library for calling backend APIs

## Apps

Directory: [apps](../../frontend/turborepo-zizibot-console/apps)

- `apps/shadcn-console`
  - What it is: Next.js admin/console UI (shadcn/ui components).
  - Port: 7130 (from app script).
  - Package: [apps/shadcn-console/package.json](../../frontend/turborepo-zizibot-console/apps/shadcn-console/package.json)
- `apps/web`
  - What it is: Next.js web app.
  - Port: 3001 (from app script).
  - Package: [apps/web/package.json](../../frontend/turborepo-zizibot-console/apps/web/package.json)
- `apps/docs`
  - What it is: Next.js docs app.
  - Package: [apps/docs/package.json](../../frontend/turborepo-zizibot-console/apps/docs/package.json)
- `apps/pendekin-router`
  - What it is: a small Node service used as an API router/proxy.
  - Main entry: [api/index.ts](../../frontend/turborepo-zizibot-console/apps/pendekin-router/api/index.ts#L1-L45)
  - Expects an upstream API base URL via environment variables (e.g., `API_BASE_URL`).

## Packages

Directory: [packages](../../frontend/turborepo-zizibot-console/packages)

Common internal packages used across the apps:

- `packages/zizibot-contracts`
  - What it is: shared DTOs and request/response types for client/server communication.
- `packages/zizibot-rest-client`
  - What it is: a REST client wrapper (Axios + SignalR client), depending on contracts/utils.
  - Package: [zizibot-rest-client/package.json](../../frontend/turborepo-zizibot-console/packages/zizibot-rest-client/package.json)
- `packages/zizibot-ui`, `packages/zizibot-shadcn`
  - What it is: shared UI components (shadcn/ui patterns and shared design primitives).
- `packages/zizibot-store`, `packages/zizibot-utils`
  - What it is: shared state and utility helpers.
