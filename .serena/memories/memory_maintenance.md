# Memory maintenance notes

- Use `serena memories` commands to list, read, check and cleanup memories when Serena CLI is available.
- Stale memories can be detected after file deletes; run `serena memories check` for a report.
- Important memory files in this repo live under `.serena/memories/` in the repo root and in several subpackages (e.g. `backend/lib/ZiziBot.TelegramBot/.serena/memories`).

